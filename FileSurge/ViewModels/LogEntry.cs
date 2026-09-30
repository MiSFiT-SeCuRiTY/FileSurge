namespace FileSurge.ViewModels;

public enum LogLevel { Info, Warning, Error, Success }

public sealed class LogEntry
{
    public DateTime Timestamp { get; } = DateTime.Now;
    public LogLevel Level { get; init; }
    public string Message { get; init; } = "";
    public string Formatted => $"[{Timestamp:HH:mm:ss}] [{Level.ToString().ToUpperInvariant()}] {Message}";

    public System.Windows.Media.Brush Color => Level switch
    {
        LogLevel.Warning => System.Windows.Media.Brushes.Orange,
        LogLevel.Error => System.Windows.Media.Brushes.IndianRed,
        LogLevel.Success => System.Windows.Media.Brushes.LightGreen,
        _ => System.Windows.Media.Brushes.Gainsboro
    };
}