using System.Security.Cryptography;

namespace FileSurge.Services;

public enum PaddingMethod
{
    Zero,
    Random,
    SecureRandom,
    RepeatingPattern,
    CustomPattern
}

public sealed class PaddingGenerator : IDisposable
{
    private readonly PaddingMethod _method;
    private readonly byte[]? _pattern;
    private readonly RandomNumberGenerator _rng;
    private int _patternOffset;

    public PaddingGenerator(PaddingMethod method, byte[]? pattern)
    {
        _method = method;
        _pattern = pattern;

        if (method is PaddingMethod.RepeatingPattern or PaddingMethod.CustomPattern)
        {
            if (pattern is null || pattern.Length == 0)
                throw new ArgumentException("Pattern required for pattern padding.", nameof(pattern));
        }

        _rng = RandomNumberGenerator.Create();
    }

    public void Fill(Span<byte> buffer)
    {
        switch (_method)
        {
            case PaddingMethod.Zero:
                buffer.Clear();
                break;
            case PaddingMethod.Random:
                Random.Shared.NextBytes(buffer);
                break;
            case PaddingMethod.SecureRandom:
                _rng.GetBytes(buffer);
                break;
            case PaddingMethod.RepeatingPattern:
            case PaddingMethod.CustomPattern:
                FillPattern(buffer);
                break;
        }
    }

    private void FillPattern(Span<byte> buffer)
    {
        var p = _pattern!;
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = p[_patternOffset];
            _patternOffset = (_patternOffset + 1) % p.Length;
        }
    }

    public void Dispose() => _rng.Dispose();
}