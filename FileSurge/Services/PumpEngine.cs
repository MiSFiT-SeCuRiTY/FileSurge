using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace FileSurge.Services;

/// <summary>
/// Streams source file to destination and appends padding. Never loads whole file into memory.
/// </summary>
public sealed class PumpEngine
{
    public event Action<string>? Log;

    private void Emit(string msg) => Log?.Invoke(msg);

    public async Task<PumpResult> ExecuteAsync(
        PumpOptions options,
        IProgress<PumpProgress>? progress = null,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        long originalSize = 0;
        long written = 0;
        string? srcHash = null;
        bool verified = false;

        try
        {
            if (!File.Exists(options.SourcePath))
                throw new FileNotFoundException("Source file not found.", options.SourcePath);

            originalSize = new FileInfo(options.SourcePath).Length;
            long padding = SizeCalculator.ComputePadding(originalSize, options.TargetSize);

            Emit($"[INFO] Source: {options.SourcePath}");
            Emit($"[INFO] Destination: {options.DestinationPath}");
            Emit($"[INFO] Original size: {SizeCalculator.FormatBytes(originalSize)}");
            Emit($"[INFO] Target size: {SizeCalculator.FormatBytes(options.TargetSize)}");
            Emit($"[INFO] Required padding: {SizeCalculator.FormatBytes(padding)}");

            var dir = Path.GetDirectoryName(options.DestinationPath)!;
            Directory.CreateDirectory(dir);

            var drive = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(dir))!);
            if (drive.IsReady && drive.AvailableFreeSpace < options.TargetSize)
                throw new IOException(
                    $"Insufficient disk space. Required: {SizeCalculator.FormatBytes(options.TargetSize)}, " +
                    $"Available: {SizeCalculator.FormatBytes(drive.AvailableFreeSpace)}.");

            // If source and destination are the same, refuse — would corrupt data.
            var srcFull = Path.GetFullPath(options.SourcePath);
            var dstFull = Path.GetFullPath(options.DestinationPath);
            if (string.Equals(srcFull, dstFull, StringComparison.OrdinalIgnoreCase))
                throw new IOException("Source and destination are the same file.");

            if (File.Exists(options.DestinationPath))
            {
                if (!options.Overwrite)
                    throw new IOException($"Destination already exists: {options.DestinationPath}");
                try { File.Delete(options.DestinationPath); }
                catch (Exception ex)
                {
                    throw new IOException(
                        $"Cannot overwrite destination (file is in use): {ex.Message}", ex);
                }
            }

            // Hash source if verification requested
            if (options.VerifyAfter)
            {
                Emit("[INFO] Hashing source (SHA-256)...");
                srcHash = await HashService.ComputeFileHashAsync(
                    options.SourcePath, HashAlgorithmName.SHA256, ct);
                Emit($"[INFO] Source SHA-256: {srcHash}");
            }

            using var generator = new PaddingGenerator(options.PaddingMethod, options.Pattern);

            // Open source: read-only, allow others to read.
            await using var src = new FileStream(
                options.SourcePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete,
                options.BufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            // Open destination: create-new, allow others to read.
            await using var dst = new FileStream(
                options.DestinationPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.Read,
                options.BufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            var buffer = new byte[options.BufferSize];
            var stopwatch = Stopwatch.StartNew();
            long lastReport = 0;
            long bytesSinceRate = 0;
            double rate = 0;
            var rateClock = Stopwatch.StartNew();

            Emit("[INFO] Copying original bytes...");
            int read;
            while ((read = await src.ReadAsync(buffer.AsMemory(), ct)) > 0)
            {
                await dst.WriteAsync(buffer.AsMemory(0, read), ct);
                written += read;
                bytesSinceRate += read;

                if (rateClock.ElapsedMilliseconds >= 500)
                {
                    rate = bytesSinceRate / rateClock.Elapsed.TotalSeconds;
                    bytesSinceRate = 0;
                    rateClock.Restart();
                }

                if (written - lastReport >= 512 * 1024 || written == originalSize)
                {
                    lastReport = written;
                    progress?.Report(BuildProgress(written, options.TargetSize, stopwatch, rate));
                }
            }

            Emit("[INFO] Writing padding...");
            long remaining = padding;
            while (remaining > 0)
            {
                ct.ThrowIfCancellationRequested();
                int chunk = (int)Math.Min(buffer.Length, remaining);
                var span = buffer.AsSpan(0, chunk);
                generator.Fill(span);
                await dst.WriteAsync(buffer.AsMemory(0, chunk), ct);
                written += chunk;
                remaining -= chunk;
                bytesSinceRate += chunk;

                if (rateClock.ElapsedMilliseconds >= 500)
                {
                    rate = bytesSinceRate / rateClock.Elapsed.TotalSeconds;
                    bytesSinceRate = 0;
                    rateClock.Restart();
                }

                if (written - lastReport >= 512 * 1024 || remaining == 0)
                {
                    lastReport = written;
                    progress?.Report(BuildProgress(written, options.TargetSize, stopwatch, rate));
                }
            }

            await dst.FlushAsync(ct);
            dst.Close();
            stopwatch.Stop();

            var finalSize = new FileInfo(options.DestinationPath).Length;
            Emit($"[INFO] Final size: {SizeCalculator.FormatBytes(finalSize)}");

            if (finalSize != options.TargetSize)
                throw new IOException(
                    $"Final size mismatch. Expected {options.TargetSize}, got {finalSize}.");

            if (options.VerifyAfter)
            {
                Emit("[INFO] Verifying original bytes preserved...");
                var dstHash = await ComputePrefixHashAsync(options.DestinationPath, originalSize, ct);

                if (!string.Equals(srcHash, dstHash, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Integrity check failed: original bytes modified.");

                verified = true;
                Emit("[SUCCESS] INTEGRITY: VERIFIED");
            }

            Emit($"[SUCCESS] Completed in {sw.Elapsed:hh\\:mm\\:ss}.");

            return new PumpResult(
                true, originalSize, finalSize, padding, srcHash, verified, sw.Elapsed, null);
        }
        catch (OperationCanceledException)
        {
            TryDeletePartial(options.DestinationPath);
            Emit("[WARNING] Operation cancelled.");
            return new PumpResult(false, originalSize, written, 0, srcHash, false, sw.Elapsed, "Cancelled");
        }
        catch (Exception ex)
        {
            TryDeletePartial(options.DestinationPath);
            Emit($"[ERROR] {ex.Message}");
            return new PumpResult(false, originalSize, written, 0, srcHash, false, sw.Elapsed, ex.Message);
        }
    }

    private static PumpProgress BuildProgress(long written, long total, Stopwatch sw, double rate)
    {
        var elapsed = sw.Elapsed;
        TimeSpan eta = TimeSpan.Zero;
        if (rate > 1 && total > written)
            eta = TimeSpan.FromSeconds((total - written) / rate);
        return new PumpProgress(written, total, rate, elapsed, eta);
    }

    private static async Task<string> ComputePrefixHashAsync(string path, long length, CancellationToken ct)
    {
        using var sha = SHA256.Create();
        await using var fs = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete,
            1024 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);

        var buffer = new byte[1024 * 1024];
        long remaining = length;
        while (remaining > 0)
        {
            int toRead = (int)Math.Min(buffer.Length, remaining);
            int read = await fs.ReadAsync(buffer.AsMemory(0, toRead), ct);
            if (read == 0) throw new EndOfStreamException();
            sha.TransformBlock(buffer, 0, read, null, 0);
            remaining -= read;
        }
        sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        return Convert.ToHexString(sha.Hash!).ToLowerInvariant();
    }

    private static void TryDeletePartial(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch { }
    }
}