using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using FileSurge.Services;
using Microsoft.Win32;

namespace FileSurge.ViewModels;

public sealed class HexViewModel : ViewModelBase
{
    private string? _filePath;
    private long _fileLength;
    private long _offset;
    private string _offsetText = "0";
    private string _gotoText = "0";
    private string _searchText = "";

    public ObservableCollection<HexLine> Lines { get; } = new();

    public string FilePathDisplay => _filePath ?? "No file loaded";
    public long FileLength { get => _fileLength; private set { if (SetProperty(ref _fileLength, value)) OnPropertyChanged(nameof(FileLengthText)); } }
    public string FileLengthText => SizeCalculator.FormatBytes(_fileLength);
    public string OffsetText { get => _offsetText; set => SetProperty(ref _offsetText, value); }
    public string GotoText { get => _gotoText; set => SetProperty(ref _gotoText, value); }
    public string SearchText { get => _searchText; set => SetProperty(ref _searchText, value); }

    public AsyncRelayCommand OpenCommand { get; }
    public AsyncRelayCommand NextCommand { get; }
    public AsyncRelayCommand PrevCommand { get; }
    public AsyncRelayCommand GotoCommand { get; }
    public AsyncRelayCommand SearchCommand { get; }

    private const int LinesPerPage = 32;

    public HexViewModel()
    {
        OpenCommand = new AsyncRelayCommand(async _ => await OpenAsync());
        NextCommand = new AsyncRelayCommand(async _ => await LoadOffsetAsync(_offset + LinesPerPage * HexReader.BytesPerLine));
        PrevCommand = new AsyncRelayCommand(async _ => await LoadOffsetAsync(Math.Max(0, _offset - LinesPerPage * HexReader.BytesPerLine)));
        GotoCommand = new AsyncRelayCommand(async _ => await GotoAsync());
        SearchCommand = new AsyncRelayCommand(async _ => await SearchAsync());
    }

    private async Task OpenAsync()
    {
        var dlg = new OpenFileDialog();
        if (dlg.ShowDialog() != true) return;
        _filePath = dlg.FileName;
        FileLength = new FileInfo(_filePath).Length;
        OnPropertyChanged(nameof(FilePathDisplay));
        await LoadOffsetAsync(0);
    }

    private async Task GotoAsync()
    {
        if (_filePath is null) return;
        if (long.TryParse(GotoText, out var off) && off >= 0)
            await LoadOffsetAsync(off);
    }

    private async Task SearchAsync()
    {
        if (_filePath is null || string.IsNullOrWhiteSpace(SearchText)) return;
        var needle = ParseHex(SearchText);
        if (needle.Length == 0) return;

        await using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, true);
        var buf = new byte[1 << 20];
        long pos = _offset;
        fs.Seek(pos, SeekOrigin.Begin);

        var tail = new List<byte>();
        int read;
        while ((read = await fs.ReadAsync(buf)) > 0)
        {
            var window = new byte[tail.Count + read];
            tail.CopyTo(window, 0);
            Array.Copy(buf, 0, window, tail.Count, read);

            int idx = IndexOfSequence(window, needle);
            if (idx >= 0)
            {
                await LoadOffsetAsync(pos + idx);
                return;
            }

            int keep = Math.Min(needle.Length - 1, window.Length);
            tail.Clear();
            for (int i = window.Length - keep; i < window.Length; i++)
                tail.Add(window[i]);

            pos += read;
        }
        MessageBox.Show("Pattern not found.");
    }

    private static int IndexOfSequence(byte[] haystack, byte[] needle)
    {
        if (needle.Length == 0) return -1;
        for (int i = 0; i <= haystack.Length - needle.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < needle.Length; j++)
                if (haystack[i + j] != needle[j]) { match = false; break; }
            if (match) return i;
        }
        return -1;
    }

    private static byte[] ParseHex(string s)
    {
        var clean = new string(s.Where(c => Uri.IsHexDigit(c)).ToArray());
        if (clean.Length % 2 != 0) return Array.Empty<byte>();
        var result = new byte[clean.Length / 2];
        for (int i = 0; i < result.Length; i++)
            result[i] = Convert.ToByte(clean.Substring(i * 2, 2), 16);
        return result;
    }

    private async Task LoadOffsetAsync(long offset)
    {
        if (_filePath is null) return;
        _offset = Math.Max(0, offset);
        OffsetText = _offset.ToString("X8");

        var lines = await HexReader.ReadLinesAsync(_filePath, _offset, LinesPerPage);
        Lines.Clear();
        foreach (var l in lines) Lines.Add(l);
    }
}