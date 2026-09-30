using System.IO;

namespace FileSurge.Services;

public sealed class HexLine
{
    public long Offset { get; init; }
    public byte[] Bytes { get; init; } = Array.Empty<byte>();
    public string Ascii { get; init; } = "";
    public string HexDisplay => string.Join(" ", Bytes.Select(b => b.ToString("X2")));

    public HexLine(long offset, byte[] bytes, string ascii)
    {
        Offset = offset;
        Bytes = bytes;
        Ascii = ascii;
    }
}

public static class HexReader
{
    public const int BytesPerLine = 16;

    public static async Task<IReadOnlyList<HexLine>> ReadLinesAsync(
        string path, long startOffset, int lineCount, CancellationToken ct = default)
    {
        var lines = new List<HexLine>(lineCount);
        await using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, true);

        if (startOffset >= fs.Length) return lines;
        fs.Seek(startOffset, SeekOrigin.Begin);

        var buffer = new byte[BytesPerLine];
        for (int i = 0; i < lineCount; i++)
        {
            int read = await fs.ReadAsync(buffer.AsMemory(), ct);
            if (read == 0) break;

            var bytes = new byte[read];
            Array.Copy(buffer, bytes, read);

            var ascii = new char[read];
            for (int j = 0; j < read; j++)
            {
                byte b = bytes[j];
                ascii[j] = b >= 32 && b < 127 ? (char)b : '.';
            }
            lines.Add(new HexLine(startOffset + i * BytesPerLine, bytes, new string(ascii)));
        }
        return lines;
    }
}