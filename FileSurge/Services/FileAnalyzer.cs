using System.IO;
using System.Security.Cryptography;

namespace FileSurge.Services;

public sealed record FileInfoResult(
    string Name,
    string Extension,
    string Type,
    long Size,
    DateTime Created,
    DateTime Modified,
    string? Sha256,
    string? Sha512,
    double? Entropy,
    bool CanRead,
    bool CanWrite,
    FileAttributes Attributes);

public static class FileAnalyzer
{
    public static async Task<FileInfoResult> AnalyzeAsync(string path, CancellationToken ct = default)
    {
        var fi = new FileInfo(path);
        if (!fi.Exists) throw new FileNotFoundException("File not found.", path);

        var sha256 = await HashService.ComputeFileHashAsync(path, HashAlgorithmName.SHA256, ct);
        var sha512 = await HashService.ComputeFileHashAsync(path, HashAlgorithmName.SHA512, ct);

        double? entropy = null;
        try { entropy = await ComputeEntropySampleAsync(path, ct); } catch { }

        return new FileInfoResult(
            fi.Name,
            fi.Extension,
            DetectType(fi.Extension),
            fi.Length,
            fi.CreationTime,
            fi.LastWriteTime,
            sha256,
            sha512,
            entropy,
            CanRead(fi),
            CanWrite(fi),
            fi.Attributes);
    }

    public static string DetectType(string ext) => ext.ToLowerInvariant() switch
    {
        ".exe" or ".dll" => "PE Executable",
        ".zip" => "ZIP Archive",
        ".rar" => "RAR Archive",
        ".7z" => "7-Zip Archive",
        ".png" => "PNG Image",
        ".jpg" or ".jpeg" => "JPEG Image",
        ".gif" => "GIF Image",
        ".pdf" => "PDF Document",
        ".txt" => "Text File",
        ".log" => "Log File",
        ".bin" or ".dat" => "Binary Data",
        ".mp4" or ".mkv" or ".avi" => "Video File",
        ".mp3" or ".wav" or ".flac" => "Audio File",
        "" => "Unknown",
        _ => $"{ext.TrimStart('.').ToUpperInvariant()} File"
    };

    private static bool CanRead(FileInfo fi)
    {
        try { using var s = fi.OpenRead(); return true; } catch { return false; }
    }

    private static bool CanWrite(FileInfo fi)
    {
        try { using var s = fi.OpenWrite(); return true; } catch { return false; }
    }

    private static async Task<double> ComputeEntropySampleAsync(string path, CancellationToken ct)
    {
        const int sampleSize = 1 << 20;
        await using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, true);
        var buf = new byte[Math.Min(sampleSize, fs.Length)];
        int read = await fs.ReadAsync(buf.AsMemory(), ct);
        if (read == 0) return 0;

        var counts = new int[256];
        for (int i = 0; i < read; i++) counts[buf[i]]++;
        double entropy = 0;
        for (int i = 0; i < 256; i++)
        {
            if (counts[i] == 0) continue;
            double p = (double)counts[i] / read;
            entropy -= p * Math.Log2(p);
        }
        return entropy;
    }
}