using System.IO;
using System.Security.Cryptography;
using FileSurge.Services;
using Microsoft.Win32;

namespace FileSurge.ViewModels;

public sealed class ToolsViewModel : ViewModelBase
{
    private double _currentValue = 1;
    private string _currentUnit = "MB";
    private double _targetValue = 100;
    private string _targetUnit = "MB";

    public string[] Units { get; } = { "B", "KB", "MB", "GB", "TB" };
    public double CurrentValue { get => _currentValue; set { if (SetProperty(ref _currentValue, value)) RefreshCalc(); } }
    public string CurrentUnit { get => _currentUnit; set { if (SetProperty(ref _currentUnit, value)) RefreshCalc(); } }
    public double TargetValue { get => _targetValue; set { if (SetProperty(ref _targetValue, value)) RefreshCalc(); } }
    public string TargetUnit { get => _targetUnit; set { if (SetProperty(ref _targetUnit, value)) RefreshCalc(); } }

    private string _calcResult = "";
    public string CalcResult { get => _calcResult; set => SetProperty(ref _calcResult, value); }

    private string _hashAlgorithm = "SHA-256";
    private string _hashResult = "";
    private string _hashFilePath = "";
    public string[] HashAlgorithms { get; } = { "MD5", "SHA-1", "SHA-256", "SHA-384", "SHA-512" };
    public string HashAlgorithm { get => _hashAlgorithm; set => SetProperty(ref _hashAlgorithm, value); }
    public string HashResult { get => _hashResult; set => SetProperty(ref _hashResult, value); }
    public string HashFilePath { get => _hashFilePath; set => SetProperty(ref _hashFilePath, value); }

    private string _diskInfo = "";
    public string DiskInfo { get => _diskInfo; set => SetProperty(ref _diskInfo, value); }

    public RelayCommand CalculateCommand { get; }
    public RelayCommand BrowseHashCommand { get; }
    public AsyncRelayCommand ComputeHashCommand { get; }
    public RelayCommand RefreshDiskCommand { get; }

    public ToolsViewModel()
    {
        CalculateCommand = new RelayCommand(_ => RefreshCalc());
        BrowseHashCommand = new RelayCommand(_ => BrowseHash());
        ComputeHashCommand = new AsyncRelayCommand(async _ => await ComputeHashAsync());
        RefreshDiskCommand = new RelayCommand(_ => RefreshDisk());
        RefreshCalc();
        RefreshDisk();
    }

    private void RefreshCalc()
    {
        try
        {
            long curBytes = (long)(CurrentValue * SizeCalculator.UnitMultiplier(CurrentUnit));
            long tgtBytes = (long)(TargetValue * SizeCalculator.UnitMultiplier(TargetUnit));
            long pad = SizeCalculator.ComputePadding(curBytes, tgtBytes);
            CalcResult = $"Current:   {SizeCalculator.FormatBytes(curBytes)}\n" +
                         $"Target:    {SizeCalculator.FormatBytes(tgtBytes)}\n" +
                         $"Padding:   {SizeCalculator.FormatBytes(pad)}";
        }
        catch (Exception ex) { CalcResult = $"Error: {ex.Message}"; }
    }

    private void BrowseHash()
    {
        var dlg = new OpenFileDialog();
        if (dlg.ShowDialog() == true) HashFilePath = dlg.FileName;
    }

    private async Task ComputeHashAsync()
    {
        if (!File.Exists(HashFilePath)) { HashResult = "File not found."; return; }
        try
        {
            var algo = HashAlgorithm switch
            {
                "MD5" => HashAlgorithmName.MD5,
                "SHA-1" => HashAlgorithmName.SHA1,
                "SHA-256" => HashAlgorithmName.SHA256,
                "SHA-384" => HashAlgorithmName.SHA384,
                "SHA-512" => HashAlgorithmName.SHA512,
                _ => HashAlgorithmName.SHA256
            };
            HashResult = "Computing...";
            var hash = await HashService.ComputeFileHashAsync(HashFilePath, algo);
            HashResult = hash;
        }
        catch (Exception ex) { HashResult = $"Error: {ex.Message}"; }
    }

    private void RefreshDisk()
    {
        try
        {
            var sb = new System.Text.StringBuilder();
            foreach (var d in DriveInfo.GetDrives())
            {
                if (!d.IsReady) continue;
                sb.AppendLine($"{d.Name,-5} {SizeCalculator.FormatBytes(d.AvailableFreeSpace),-12} free / {SizeCalculator.FormatBytes(d.TotalSize)}");
            }
            DiskInfo = sb.ToString();
        }
        catch (Exception ex) { DiskInfo = $"Error: {ex.Message}"; }
    }
}