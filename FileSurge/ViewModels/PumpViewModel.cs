using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using FileSurge.Services;
using Microsoft.Win32;

namespace FileSurge.ViewModels;

public sealed class PumpViewModel : ViewModelBase
{
    private readonly PumpEngine _engine = new();

    private string? _sourcePath;
    private long _sourceSize;
    private string _fileName = "—";
    private string _fileType = "—";
    private string _formattedSize = "—";
    private string _sha256 = "—";
    private DateTime _created;
    private DateTime _modified;

    private double _sizeValue = 100;
    private string _selectedUnit = "MB";
    private TargetSizeMode _sizeMode = TargetSizeMode.ExactSize;

    private PaddingMethod _paddingMethod = PaddingMethod.SecureRandom;
    private string _patternHex = "AA BB CC DD";

    private bool _keepOriginal = true;
    private bool _overwrite;
    private bool _addSuffix = true;
    private string _suffix = "_pumped";

    private bool _isRunning;
    private double _progressPercent;
    private string _progressText = "0%";
    private string _bytesText = "0 B / 0 B";
    private string _speedText = "0 B/s";
    private string _etaText = "—";
    private string _statusText = "IDLE";

    private CancellationTokenSource? _cts;

    public ObservableCollection<LogEntry> Logs { get; } = new();
    public string[] Units { get; } = { "B", "KB", "MB", "GB", "TB" };

    // File info
    public string? SourcePath { get => _sourcePath; private set => SetProperty(ref _sourcePath, value); }
    public string FileName { get => _fileName; private set => SetProperty(ref _fileName, value); }
    public string FileType { get => _fileType; private set => SetProperty(ref _fileType, value); }
    public string FormattedSize { get => _formattedSize; private set => SetProperty(ref _formattedSize, value); }
    public string Sha256Short => _sha256 == "—" ? "—" : _sha256[..16] + "...";
    public string CreatedText => _created == default ? "—" : _created.ToString("yyyy-MM-dd HH:mm");
    public string ModifiedText => _modified == default ? "—" : _modified.ToString("yyyy-MM-dd HH:mm");

    public string FormattedTarget =>
        SizeCalculator.FormatBytes(SizeCalculator.ComputeTargetSize(_sourceSize, _sizeMode, _sizeValue, _selectedUnit));
    public string FormattedPadding =>
        SizeCalculator.FormatBytes(SizeCalculator.ComputePadding(
            _sourceSize,
            SizeCalculator.ComputeTargetSize(_sourceSize, _sizeMode, _sizeValue, _selectedUnit)));

    // Options
    public double SizeValue { get => _sizeValue; set { if (SetProperty(ref _sizeValue, value)) RefreshComputed(); } }
    public string SelectedUnit { get => _selectedUnit; set { if (SetProperty(ref _selectedUnit, value)) RefreshComputed(); } }
    public bool IsExactMode { get => _sizeMode == TargetSizeMode.ExactSize; set { if (value) { _sizeMode = TargetSizeMode.ExactSize; OnPropertyChanged(); RefreshComputed(); } } }
    public bool IsAddMode { get => _sizeMode == TargetSizeMode.AddToCurrent; set { if (value) { _sizeMode = TargetSizeMode.AddToCurrent; OnPropertyChanged(); RefreshComputed(); } } }
    public bool IsMinMode { get => _sizeMode == TargetSizeMode.MinimumSize; set { if (value) { _sizeMode = TargetSizeMode.MinimumSize; OnPropertyChanged(); RefreshComputed(); } } }

    public bool IsZeroPad { get => _paddingMethod == PaddingMethod.Zero; set { if (value) { _paddingMethod = PaddingMethod.Zero; OnPropertyChanged(); } } }
    public bool IsRandomPad { get => _paddingMethod == PaddingMethod.Random; set { if (value) { _paddingMethod = PaddingMethod.Random; OnPropertyChanged(); } } }
    public bool IsSecurePad { get => _paddingMethod == PaddingMethod.SecureRandom; set { if (value) { _paddingMethod = PaddingMethod.SecureRandom; OnPropertyChanged(); } } }
    public bool IsRepeatingPad { get => _paddingMethod == PaddingMethod.RepeatingPattern; set { if (value) { _paddingMethod = PaddingMethod.RepeatingPattern; OnPropertyChanged(); } } }
    public bool IsCustomPad { get => _paddingMethod == PaddingMethod.CustomPattern; set { if (value) { _paddingMethod = PaddingMethod.CustomPattern; OnPropertyChanged(); } } }

    public string PatternHex { get => _patternHex; set => SetProperty(ref _patternHex, value); }
    public bool KeepOriginal { get => _keepOriginal; set => SetProperty(ref _keepOriginal, value); }
    public bool Overwrite { get => _overwrite; set => SetProperty(ref _overwrite, value); }
    public bool AddSuffix { get => _addSuffix; set => SetProperty(ref _addSuffix, value); }
    public string Suffix { get => _suffix; set => SetProperty(ref _suffix, value); }

    public bool IsRunning { get => _isRunning; private set { if (SetProperty(ref _isRunning, value)) { OnPropertyChanged(nameof(IsNotRunning)); StartCommand.RaiseCanExecuteChanged(); CancelCommand.RaiseCanExecuteChanged(); } } }
    public bool IsNotRunning => !_isRunning;

    public double ProgressPercent { get => _progressPercent; private set => SetProperty(ref _progressPercent, value); }
    public string ProgressText { get => _progressText; private set => SetProperty(ref _progressText, value); }
    public string BytesText { get => _bytesText; private set => SetProperty(ref _bytesText, value); }
    public string SpeedText { get => _speedText; private set => SetProperty(ref _speedText, value); }
    public string EtaText { get => _etaText; private set => SetProperty(ref _etaText, value); }
    public string StatusText { get => _statusText; private set => SetProperty(ref _statusText, value); }

    public RelayCommand BrowseCommand { get; }
    public RelayCommand ClearFileCommand { get; }
    public RelayCommand CopyHashCommand { get; }
    public AsyncRelayCommand StartCommand { get; }
    public RelayCommand CancelCommand { get; }

    public PumpViewModel()
    {
        _engine.Log += msg => Log(msg);

        BrowseCommand = new RelayCommand(_ => Browse());
        ClearFileCommand = new RelayCommand(_ => ClearFile());
        CopyHashCommand = new RelayCommand(_ =>
        {
            if (_sha256 != "—") Clipboard.SetText(_sha256);
        });
        StartCommand = new AsyncRelayCommand(async _ => await StartAsync(), _ => !IsRunning && SourcePath != null);
        CancelCommand = new RelayCommand(_ => _cts?.Cancel(), _ => IsRunning);
    }

    public void SetFile(string path)
    {
        if (!File.Exists(path)) { Log("[ERROR] File not found.", LogLevel.Error); return; }
        var fi = new FileInfo(path);
        SourcePath = path;
        _sourceSize = fi.Length;
        FileName = fi.Name;
        FileType = DetectType(fi.Extension);
        FormattedSize = SizeCalculator.FormatBytes(fi.Length);
        _created = fi.CreationTime;
        _modified = fi.LastWriteTime;
        OnPropertyChanged(nameof(CreatedText));
        OnPropertyChanged(nameof(ModifiedText));
        RefreshComputed();

        Log($"[INFO] File selected: {fi.Name} ({FormattedSize})");

        // Hash async
        _ = Task.Run(async () =>
        {
            try
            {
                _sha256 = await HashService.ComputeFileHashAsync(path, System.Security.Cryptography.HashAlgorithmName.SHA256);
                App.Current.Dispatcher.Invoke(() => OnPropertyChanged(nameof(Sha256Short)));
            }
            catch { }
        });
    }

    private void Browse()
    {
        var dlg = new OpenFileDialog { Title = "Select file to pump" };
        if (dlg.ShowDialog() == true) SetFile(dlg.FileName);
    }

    private void ClearFile()
    {
        SourcePath = null;
        FileName = "—"; FileType = "—"; FormattedSize = "—"; _sha256 = "—";
        _sourceSize = 0; _created = default; _modified = default;
        OnPropertyChanged(nameof(Sha256Short));
        OnPropertyChanged(nameof(CreatedText));
        OnPropertyChanged(nameof(ModifiedText));
        RefreshComputed();
    }

    private async Task StartAsync()
    {
        if (SourcePath is null) return;

        long target;
        try
        {
            target = SizeCalculator.ComputeTargetSize(_sourceSize, _sizeMode, _sizeValue, _selectedUnit);
        }
        catch (Exception ex)
        {
            Log($"[ERROR] Invalid target size: {ex.Message}", LogLevel.Error);
            return;
        }

        if (target <= _sourceSize)
        {
            Log("[ERROR] Target size must be larger than source size.", LogLevel.Error);
            return;
        }

        byte[]? pattern = null;
        if (_paddingMethod is PaddingMethod.RepeatingPattern or PaddingMethod.CustomPattern)
        {
            try { pattern = ParseHexPattern(_patternHex); }
            catch (Exception ex)
            {
                Log($"[ERROR] Invalid pattern: {ex.Message}", LogLevel.Error);
                return;
            }
        }

        var dir = Path.GetDirectoryName(SourcePath)!;
        var name = Path.GetFileNameWithoutExtension(SourcePath);
        var ext = Path.GetExtension(SourcePath);
        var suffix = AddSuffix ? Suffix : "";
        var dest = Path.Combine(dir, $"{name}{suffix}{ext}");

        if (File.Exists(dest) && !Overwrite)
        {
            Log($"[ERROR] Destination exists: {dest}", LogLevel.Error);
            return;
        }

        IsRunning = true;
        StatusText = "PROCESSING";
        Logs.Clear();
        _cts = new CancellationTokenSource();

        var options = new PumpOptions
        {
            SourcePath = SourcePath,
            DestinationPath = dest,
            TargetSize = target,
            PaddingMethod = _paddingMethod,
            Pattern = pattern,
            Overwrite = Overwrite
        };

        var progress = new Progress<PumpProgress>(p =>
        {
            ProgressPercent = p.Percent;
            ProgressText = $"{p.Percent:0}%";
            BytesText = $"{SizeCalculator.FormatBytes(p.BytesProcessed)} / {SizeCalculator.FormatBytes(p.TotalBytes)}";
            SpeedText = $"{SizeCalculator.FormatBytes((long)p.BytesPerSecond)}/s";
            EtaText = p.Eta > TimeSpan.Zero ? $"ETA {p.Eta:hh\\:mm\\:ss}" : "—";
        });

        try
        {
            var result = await _engine.ExecuteAsync(options, progress, _cts.Token);
            if (result.Success)
            {
                StatusText = "COMPLETE";
                Log("[SUCCESS] Pump completed successfully.", LogLevel.Success);
                if (KeepOriginal == false && SourcePath != dest)
                {
                    try { File.Delete(SourcePath); Log("[INFO] Original deleted."); }
                    catch (Exception ex) { Log($"[WARNING] Could not delete original: {ex.Message}", LogLevel.Warning); }
                }
            }
            else
            {
                StatusText = result.Error == "Cancelled" ? "CANCELLED" : "FAILED";
            }
        }
        finally
        {
            IsRunning = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private static byte[] ParseHexPattern(string hex)
    {
        var clean = new string(hex.Where(c => !char.IsWhiteSpace(c)).ToArray());
        if (clean.Length == 0) throw new FormatException("Pattern empty.");
        if (clean.Length % 2 != 0) throw new FormatException("Hex must have even length.");
        var bytes = new byte[clean.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] = Convert.ToByte(clean.Substring(i * 2, 2), 16);
        return bytes;
    }

    private static string DetectType(string ext) => ext.ToLowerInvariant() switch
    {
        ".exe" or ".dll" => "PE Executable",
        ".zip" => "ZIP Archive",
        ".png" => "PNG Image",
        ".jpg" or ".jpeg" => "JPEG Image",
        ".pdf" => "PDF Document",
        ".txt" => "Text File",
        "" => "Unknown",
        _ => $"{ext.TrimStart('.').ToUpperInvariant()} File"
    };

    private void RefreshComputed()
    {
        OnPropertyChanged(nameof(FormattedTarget));
        OnPropertyChanged(nameof(FormattedPadding));
        StartCommand.RaiseCanExecuteChanged();
    }

    private void Log(string msg, LogLevel level = LogLevel.Info)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            Logs.Add(new LogEntry { Level = level, Message = msg });
            if (Logs.Count > 500) Logs.RemoveAt(0);
        });
    }
}