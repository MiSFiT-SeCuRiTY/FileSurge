using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using FileSurge.Services;
using Microsoft.Win32;

namespace FileSurge.ViewModels;

public sealed class BatchViewModel : ViewModelBase
{
    private readonly PumpEngine _engine = new();
    private long _targetBytes = 100L * 1024 * 1024;
    private bool _isRunning;
    private CancellationTokenSource? _cts;

    public ObservableCollection<BatchItem> Items { get; } = new();

    public long TargetBytes { get => _targetBytes; set { if (SetProperty(ref _targetBytes, value)) RefreshTargets(); } }
    public string TargetText => SizeCalculator.FormatBytes(_targetBytes);

    public bool IsRunning { get => _isRunning; private set { if (SetProperty(ref _isRunning, value)) { OnPropertyChanged(nameof(IsNotRunning)); StartCommand.RaiseCanExecuteChanged(); CancelCommand.RaiseCanExecuteChanged(); } } }
    public bool IsNotRunning => !_isRunning;

    public RelayCommand AddFilesCommand { get; }
    public RelayCommand AddFolderCommand { get; }
    public RelayCommand ClearCommand { get; }
    public RelayCommand RemoveItemCommand { get; }
    public AsyncRelayCommand StartCommand { get; }
    public RelayCommand CancelCommand { get; }

    public BatchViewModel()
    {
        AddFilesCommand = new RelayCommand(_ => AddFiles());
        AddFolderCommand = new RelayCommand(_ => AddFolder());
        ClearCommand = new RelayCommand(_ => { if (!IsRunning) Items.Clear(); });
        RemoveItemCommand = new RelayCommand(p => { if (!IsRunning && p is BatchItem it) Items.Remove(it); });
        StartCommand = new AsyncRelayCommand(async _ => await StartAsync(), _ => !IsRunning && Items.Count > 0);
        CancelCommand = new RelayCommand(_ => _cts?.Cancel(), _ => IsRunning);
    }

    private void AddFiles()
    {
        var dlg = new OpenFileDialog { Multiselect = true, Title = "Add files to batch" };
        if (dlg.ShowDialog() != true) return;
        foreach (var f in dlg.FileNames) AddFile(f);
        RefreshTargets();
        StartCommand.RaiseCanExecuteChanged();
    }

    private void AddFolder()
    {
        var dlg = new OpenFolderDialog { Title = "Add folder (recursive)" };
        if (dlg.ShowDialog() != true) return;
        try
        {
            foreach (var f in Directory.EnumerateFiles(dlg.FolderName, "*", SearchOption.AllDirectories))
                AddFile(f);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error reading folder: {ex.Message}");
        }
        RefreshTargets();
        StartCommand.RaiseCanExecuteChanged();
    }

    private void AddFile(string path)
    {
        try
        {
            var fi = new FileInfo(path);
            Items.Add(new BatchItem
            {
                SourcePath = path,
                FileName = fi.Name,
                OriginalSize = fi.Length,
                TargetSize = Math.Max(_targetBytes, fi.Length + 1)
            });
        }
        catch { }
    }

    private void RefreshTargets()
    {
        foreach (var item in Items)
            item.TargetSize = Math.Max(_targetBytes, item.OriginalSize + 1);
        OnPropertyChanged(nameof(TargetText));
    }

    private async Task StartAsync()
    {
        if (Items.Count == 0) return;
        IsRunning = true;
        _cts = new CancellationTokenSource();

        try
        {
            foreach (var item in Items)
            {
                if (_cts.IsCancellationRequested) break;
                if (item.State == BatchItemState.Complete) continue;

                item.State = BatchItemState.Processing;
                item.Message = "Processing...";

                try
                {
                    var dir = Path.GetDirectoryName(item.SourcePath)!;
                    var name = Path.GetFileNameWithoutExtension(item.SourcePath);
                    var ext = Path.GetExtension(item.SourcePath);
                    var dest = Path.Combine(dir, $"{name}_pumped{ext}");

                    var opts = new PumpOptions
                    {
                        SourcePath = item.SourcePath,
                        DestinationPath = dest,
                        TargetSize = item.TargetSize,
                        PaddingMethod = PaddingMethod.SecureRandom,
                        Overwrite = false
                    };

                    var progress = new Progress<PumpProgress>(p =>
                    {
                        item.Progress = p.Percent;
                        item.Message = $"{p.Percent:0}% • {SizeCalculator.FormatBytes((long)p.BytesPerSecond)}/s";
                    });

                    var result = await _engine.ExecuteAsync(opts, progress, _cts.Token);

                    if (result.Success)
                    {
                        item.State = BatchItemState.Complete;
                        item.Progress = 100;
                        item.Message = "Complete";
                    }
                    else
                    {
                        item.State = BatchItemState.Failed;
                        item.ErrorMessage = result.Error;
                        item.Message = result.Error ?? "Failed";
                    }
                }
                catch (Exception ex)
                {
                    item.State = BatchItemState.Failed;
                    item.ErrorMessage = ex.Message;
                    item.Message = ex.Message;
                }
            }
        }
        finally
        {
            IsRunning = false;
            _cts?.Dispose();
            _cts = null;
        }
    }
}