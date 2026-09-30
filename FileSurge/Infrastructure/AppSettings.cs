namespace FileSurge.Infrastructure;

public sealed class AppSettings
{
    public string Theme { get; set; } = "Dark";
    public string AccentColor { get; set; } = "#00FF9C";
    public bool CompactMode { get; set; }
    public bool AnimationsEnabled { get; set; } = true;

    public int IoBufferSize { get; set; } = 1024 * 1024;
    public bool VerifyAfterProcessing { get; set; } = true;
    public string DefaultPaddingMethod { get; set; } = "SecureRandom";
    public string DefaultOutputDirectory { get; set; } = "SameAsSource";

    public int MaxConcurrentBatch { get; set; } = 1;
    public bool MonitorDiskUsage { get; set; } = true;

    public bool EnableLogs { get; set; } = true;
    public string LogLevel { get; set; } = "Info";
    public long MaxLogSizeBytes { get; set; } = 5 * 1024 * 1024;

    public bool ConfirmOverwrite { get; set; } = true;
    public bool ConfirmLargeFiles { get; set; } = true;
    public bool KeepOriginalsByDefault { get; set; } = true;

    public bool ShowStartupAnimation { get; set; } = true;
}