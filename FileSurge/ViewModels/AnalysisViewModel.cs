using System.IO;
using System.Windows;
using FileSurge.Services;
using Microsoft.Win32;

namespace FileSurge.ViewModels;

public sealed class AnalysisViewModel : ViewModelBase
{
    private string _fileName = "—";
    private string _fileType = "—";
    private string _formattedSize = "—";
    private string _created = "—";
    private string _modified = "—";
    private string _sha256 = "—";
    private string _sha512 = "—";
    private string _entropy = "—";
    private string _permissions = "—";
    private string _attributes = "—";
    private bool _isBusy;

    public string FileName { get => _fileName; set => SetProperty(ref _fileName, value); }
    public string FileType { get => _fileType; set => SetProperty(ref _fileType, value); }
    public string FormattedSize { get => _formattedSize; set => SetProperty(ref _formattedSize, value); }
    public string Created { get => _created; set => SetProperty(ref _created, value); }
    public string Modified { get => _modified; set => SetProperty(ref _modified, value); }
    public string Sha256 { get => _sha256; set => SetProperty(ref _sha256, value); }
    public string Sha512 { get => _sha512; set => SetProperty(ref _sha512, value); }
    public string Entropy { get => _entropy; set => SetProperty(ref _entropy, value); }
    public string Permissions { get => _permissions; set => SetProperty(ref _permissions, value); }
    public string Attributes { get => _attributes; set => SetProperty(ref _attributes, value); }
    public bool IsBusy { get => _isBusy; set { if (SetProperty(ref _isBusy, value)) AnalyzeCommand.RaiseCanExecuteChanged(); } }

    public RelayCommand BrowseCommand { get; }
    public AsyncRelayCommand AnalyzeCommand { get; }

    public AnalysisViewModel()
    {
        BrowseCommand = new RelayCommand(_ => Browse());
        AnalyzeCommand = new AsyncRelayCommand(async _ => await AnalyzeAsync(), _ => !IsBusy);
    }

    private void Browse()
    {
        var dlg = new OpenFileDialog();
        if (dlg.ShowDialog() == true)
            _ = AnalyzePathAsync(dlg.FileName);
    }

    private async Task AnalyzeAsync()
    {
        await Task.CompletedTask;
    }

    private async Task AnalyzePathAsync(string path)
    {
        IsBusy = true;
        try
        {
            var r = await FileAnalyzer.AnalyzeAsync(path);
            FileName = r.Name;
            FileType = r.Type;
            FormattedSize = SizeCalculator.FormatBytes(r.Size);
            Created = r.Created.ToString("yyyy-MM-dd HH:mm:ss");
            Modified = r.Modified.ToString("yyyy-MM-dd HH:mm:ss");
            Sha256 = r.Sha256 ?? "—";
            Sha512 = r.Sha512 ?? "—";
            Entropy = r.Entropy.HasValue ? $"{r.Entropy.Value:0.0000} bits/byte" : "—";
            Permissions = $"{(r.CanRead ? "R" : "-")}{(r.CanWrite ? "W" : "-")}";
            Attributes = r.Attributes.ToString();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Analysis failed: {ex.Message}");
        }
        finally { IsBusy = false; }
    }
}