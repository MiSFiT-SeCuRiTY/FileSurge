using System.IO;
using System.Security.Cryptography;

namespace FileSurge.Services;

public static class HashService
{
    public static async Task<string> ComputeFileHashAsync(
        string path,
        HashAlgorithmName algorithm,
        CancellationToken ct = default)
    {
        await using var fs = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 1024 * 1024, useAsync: true);

        using var hasher = CreateHasher(algorithm);
        var buffer = new byte[1024 * 1024];
        int read;
        while ((read = await fs.ReadAsync(buffer.AsMemory(), ct)) > 0)
        {
            hasher.TransformBlock(buffer, 0, read, null, 0);
        }
        hasher.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        return Convert.ToHexString(hasher.Hash!).ToLowerInvariant();
    }

    private static HashAlgorithm CreateHasher(HashAlgorithmName name)
    {
        if (name == HashAlgorithmName.MD5) return MD5.Create();
        if (name == HashAlgorithmName.SHA1) return SHA1.Create();
        if (name == HashAlgorithmName.SHA256) return SHA256.Create();
        if (name == HashAlgorithmName.SHA384) return SHA384.Create();
        if (name == HashAlgorithmName.SHA512) return SHA512.Create();
        throw new NotSupportedException($"Unsupported hash: {name.Name}");
    }
}