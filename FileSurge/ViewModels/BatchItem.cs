namespace FileSurge.ViewModels;

public enum BatchItemState { Waiting, Processing, Complete, Failed, Skipped }

public sealed class BatchItem : ViewModelBase
{
    private BatchItemState _state = BatchItemState.Waiting;
    private double _progress;
    private string _message = "";

    public string SourcePath { get; init; } = "";
    public string FileName { get; init; } = "";
    public long OriginalSize { get; init; }
    public long TargetSize { get; set; }
    public string? ErrorMessage { get; set; }

    public BatchItemState State
    {
        get => _state;
        set { if (SetProperty(ref _state, value)) OnPropertyChanged(nameof(StateIcon)); }
    }

    public double Progress { get => _progress; set => SetProperty(ref _progress, value); }
    public string Message { get => _message; set => SetProperty(ref _message, value); }

    public string StateIcon => State switch
    {
        BatchItemState.Waiting => "○",
        BatchItemState.Processing => "⚡",
        BatchItemState.Complete => "✓",
        BatchItemState.Failed => "✕",
        BatchItemState.Skipped => "—",
        _ => "?"
    };

    public string SizeText => $"{Services.SizeCalculator.FormatBytes(OriginalSize)} → {Services.SizeCalculator.FormatBytes(TargetSize)}";
}