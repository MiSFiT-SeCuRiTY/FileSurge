namespace FileSurge.Services;

public sealed class PumpOptions
{
    public required string SourcePath { get; init; }
    public required string DestinationPath { get; init; }
    public required long TargetSize { get; init; }
    public PaddingMethod PaddingMethod { get; init; } = PaddingMethod.SecureRandom;
    public byte[]? Pattern { get; init; }
    public bool VerifyAfter { get; init; } = true;
    public int BufferSize { get; init; } = 1024 * 1024;
    public bool Overwrite { get; init; }
}

public sealed record PumpProgress(
    long BytesProcessed,
    long TotalBytes,
    double BytesPerSecond,
    TimeSpan Elapsed,
    TimeSpan Eta)
{
    public double Percent => TotalBytes <= 0 ? 0 : (double)BytesProcessed / TotalBytes * 100.0;
}

public sealed record PumpResult(
    bool Success,
    long OriginalSize,
    long FinalSize,
    long PaddingBytes,
    string? SourceHash,
    bool IntegrityVerified,
    TimeSpan Duration,
    string? Error);