using FileSurge.Infrastructure;

namespace FileSurge.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
    private readonly SettingsService _settings;

    public SettingsViewModel(SettingsService settings)
    {
        _settings = settings;
        SaveCommand = new RelayCommand(_ => Save());
        ReloadCommand = new RelayCommand(_ => Reload());
    }

    public RelayCommand SaveCommand { get; }
    public RelayCommand ReloadCommand { get; }

    public string Theme { get => _settings.Current.Theme; set { _settings.Current.Theme = value; OnPropertyChanged(); } }
    public bool AnimationsEnabled { get => _settings.Current.AnimationsEnabled; set { _settings.Current.AnimationsEnabled = value; OnPropertyChanged(); } }
    public bool CompactMode { get => _settings.Current.CompactMode; set { _settings.Current.CompactMode = value; OnPropertyChanged(); } }

    public int IoBufferSize { get => _settings.Current.IoBufferSize; set { _settings.Current.IoBufferSize = value; OnPropertyChanged(); } }
    public bool VerifyAfterProcessing { get => _settings.Current.VerifyAfterProcessing; set { _settings.Current.VerifyAfterProcessing = value; OnPropertyChanged(); } }

    public bool EnableLogs { get => _settings.Current.EnableLogs; set { _settings.Current.EnableLogs = value; OnPropertyChanged(); } }
    public string LogLevel { get => _settings.Current.LogLevel; set { _settings.Current.LogLevel = value; OnPropertyChanged(); } }

    public bool ConfirmOverwrite { get => _settings.Current.ConfirmOverwrite; set { _settings.Current.ConfirmOverwrite = value; OnPropertyChanged(); } }
    public bool ConfirmLargeFiles { get => _settings.Current.ConfirmLargeFiles; set { _settings.Current.ConfirmLargeFiles = value; OnPropertyChanged(); } }
    public bool KeepOriginalsByDefault { get => _settings.Current.KeepOriginalsByDefault; set { _settings.Current.KeepOriginalsByDefault = value; OnPropertyChanged(); } }
    public bool ShowStartupAnimation { get => _settings.Current.ShowStartupAnimation; set { _settings.Current.ShowStartupAnimation = value; OnPropertyChanged(); } }

    private void Save()
    {
        _settings.Save();
        System.Windows.MessageBox.Show("Settings saved.");
    }

    private void Reload()
    {
        _settings.Load();
        OnPropertyChanged(nameof(Theme));
        OnPropertyChanged(nameof(AnimationsEnabled));
        OnPropertyChanged(nameof(CompactMode));
        OnPropertyChanged(nameof(IoBufferSize));
        OnPropertyChanged(nameof(VerifyAfterProcessing));
        OnPropertyChanged(nameof(EnableLogs));
        OnPropertyChanged(nameof(LogLevel));
        OnPropertyChanged(nameof(ConfirmOverwrite));
        OnPropertyChanged(nameof(ConfirmLargeFiles));
        OnPropertyChanged(nameof(KeepOriginalsByDefault));
        OnPropertyChanged(nameof(ShowStartupAnimation));
    }
}