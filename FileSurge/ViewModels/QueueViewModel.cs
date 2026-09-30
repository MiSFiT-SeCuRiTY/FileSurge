using System.Collections.ObjectModel;
using System.IO;
using FileSurge.Services;
using Microsoft.Win32;

namespace FileSurge.ViewModels;

public sealed class QueueViewModel : ViewModelBase
{
    private readonly PumpEngine _engine = new();
    private long _targetBytes = 100L * 1024 * 1024;
    private bool _isRunning;
    private CancellationTokenSource? _cts;

    public ObservableCollection<BatchItem> Items { get; } = new();

    public long TargetBytes { get => _targetBytes; set { if (SetProperty(ref _targetBytes, value)) RefreshTargets(); } }
    public string TargetText => SizeCalculator.FormatBytes(_targetBytes);

    public bool IsRunning { get => _isRunning; private set { if (SetProperty(ref _isRunning, value)) { OnPropertyChanged(nameof(IsNotRunning)); RaiseCommands(); } } }
    public bool IsNotRunning => !_isRunning;

    public RelayCommand AddCommand { get; }
    public RelayCommand ClearCommand { get; }
    public RelayCommand MoveUpCommand { get; }
    public RelayCommand MoveDownCommand { get; }
    public RelayCommand RemoveCommand { get; }
    public AsyncRelayCommand StartAllCommand { get; }
    public RelayCommand CancelCommand { get; }

    public QueueViewModel()
    {
        AddCommand = new RelayCommand(_ => AddFiles());
        ClearCommand = new RelayCommand(_ => { if (!IsRunning) Items.Clear(); });
        MoveUpCommand = new RelayCommand(p => Move(p as BatchItem, -1));
        MoveDownCommand = new RelayCommand(p => Move(p as BatchItem, +1));
        RemoveCommand = new RelayCommand(p => { if (!IsRunning && p is BatchItem it) Items.Remove(it); });
        StartAllCommand = new AsyncRelayCommand(async _ => await RunAsync(Items.ToList()), _ => !IsRunning && Items.Count > 0);
        CancelCommand = new RelayCommand(_ => _cts?.Cancel(), _ => IsRunning);
    }

    private void AddFiles()
    {
        var dlg = new OpenFileDialog { Multiselect = true };
        if (dlg.ShowDialog() != true) return;
        foreach (var f in dlg.FileNames)
        {
            var fi = new FileInfo(f);
            Items.Add(new BatchItem
            {
                SourcePath = f,
                FileName = fi.Name,
                OriginalSize = fi.Length,
                TargetSize = Math.Max(_targetBytes, fi.Length + 1)
            });
        }
        RefreshTargets();
        RaiseCommands();
    }

    private void Move(BatchItem? item, int delta)
    {
        if (item is null || IsRunning) return;
        int idx = Items.IndexOf(item);
        int newIdx = idx + delta;
        if (newIdx < 0 || newIdx >= Items.Count) return;
        Items.Move(idx, newIdx);
    }

    private void RefreshTargets()
    {
        foreach (var item in Items)
            item.TargetSize = Math.Max(_targetBytes, item.OriginalSize + 1);
        OnPropertyChanged(nameof(TargetText));
    }

    private void RaiseCommands()
    {
        StartAllCommand.RaiseCanExecuteChanged();
        CancelCommand.RaiseCanExecuteChanged();
    }

    private async Task RunAsync(List<BatchItem> items)
    {
        IsRunning = true;
        _cts = new CancellationTokenSource();

        try
        {
            foreach (var item in items)
            {
                if (_cts.IsCancellationRequested) break;
                if (item.State == BatchItemState.Complete) continue;

                item.State = BatchItemState.Processing;
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
                        PaddingMethod = PaddingMethod.SecureRandom
                    };

                    var progress = new Progress<PumpProgress>(p =>
                    {
                        item.Progress = p.Percent;
                        item.Message = $"{p.Percent:0}%";
                    });

                    var result = await _engine.ExecuteAsync(opts, progress, _cts.Token);
                    item.State = result.Success ? BatchItemState.Complete : BatchItemState.Failed;
                    item.Message = result.Success ? "Complete" : (result.Error ?? "Failed");
                }
                catch (Exception ex)
                {
                    item.State = BatchItemState.Failed;
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