using System.IO;
using System.Windows.Threading;
using FileSurge.Infrastructure;
using FileSurge.Views;

namespace FileSurge.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly SettingsService _settings;
    private readonly DispatcherTimer _statusTimer;

    private object? _currentView;
    private string _statusText = "IDLE";
    private string _cpuText = "0%";
    private string _ramText = "0%";
    private string _diskText = "—";

    public object? CurrentView { get => _currentView; private set => SetProperty(ref _currentView, value); }
    public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }
    public string CpuText { get => _cpuText; set => SetProperty(ref _cpuText, value); }
    public string RamText { get => _ramText; set => SetProperty(ref _ramText, value); }
    public string DiskText { get => _diskText; set => SetProperty(ref _diskText, value); }

    public MainViewModel(SettingsService settings)
    {
        _settings = settings;
        Navigate("Pump");

        _statusTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _statusTimer.Tick += (_, _) => UpdateStats();
        _statusTimer.Start();
        UpdateStats();
    }

    public void Navigate(string key)
    {
        CurrentView = key switch
        {
            "Pump" => new PumpView(),
            "Batch" => new BatchView { DataContext = new BatchViewModel() },
            "Queue" => new QueueView { DataContext = new QueueViewModel() },
            "Analysis" => new AnalysisView { DataContext = new AnalysisViewModel() },
            "Hex" => new HexView { DataContext = new HexViewModel() },
            "Tools" => new ToolsView { DataContext = new ToolsViewModel() },
            "Settings" => new SettingsView { DataContext = new SettingsViewModel(_settings) },
            _ => CurrentView
        };
    }

    private void UpdateStats()
    {
        try
        {
            var proc = System.Diagnostics.Process.GetCurrentProcess();
            proc.Refresh();

            // Simple RAM % of total
            var totalMem = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
            if (totalMem > 0)
                RamText = $"{(proc.WorkingSet64 * 100.0 / totalMem):0}%";

            // CPU via Process.TotalProcessorTime delta
            // Skipped for simplicity — shows steady percentage
            CpuText = "—";

            // Disk free for C:
            try
            {
                var drive = new DriveInfo("C");
                if (drive.IsReady)
                    DiskText = $"{drive.AvailableFreeSpace / (1024L * 1024 * 1024)} GB FREE";
            }
            catch { DiskText = "—"; }
        }
        catch { }
    }
}