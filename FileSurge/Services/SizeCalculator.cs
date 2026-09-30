namespace FileSurge.Services;

public enum TargetSizeMode
{
    ExactSize,
    AddToCurrent,
    MinimumSize
}

public static class SizeCalculator
{
    public const long KB = 1024L;
    public const long MB = 1024L * 1024L;
    public const long GB = 1024L * 1024L * 1024L;
    public const long TB = 1024L * 1024L * 1024L * 1024L;

    public static long UnitMultiplier(string unit) => unit.ToUpperInvariant() switch
    {
        "B" => 1L,
        "KB" => KB,
        "MB" => MB,
        "GB" => GB,
        "TB" => TB,
        _ => throw new ArgumentException($"Unknown unit: {unit}")
    };

    public static long ComputeTargetSize(long currentSize, TargetSizeMode mode, double value, string unit)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
        if (double.IsNaN(value) || double.IsInfinity(value)) throw new ArgumentException("Invalid value");

        long bytes;
        try
        {
            checked { bytes = (long)Math.Round(value * UnitMultiplier(unit)); }
        }
        catch (OverflowException)
        {
            throw new OverflowException("Size exceeds 64-bit range.");
        }

        long target = mode switch
        {
            TargetSizeMode.ExactSize => bytes,
            TargetSizeMode.AddToCurrent => checked(currentSize + bytes),
            TargetSizeMode.MinimumSize => Math.Max(bytes, currentSize),
            _ => bytes
        };

        if (target < 0) throw new OverflowException("Target size is negative.");
        return target;
    }

    public static long ComputePadding(long currentSize, long targetSize)
        => Math.Max(0, targetSize - currentSize);

    public static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        double value = bytes;
        int i = 0;
        while (value >= 1024 && i < suffixes.Length - 1)
        {
            value /= 1024;
            i++;
        }
        return $"{value:0.##} {suffixes[i]}";
    }
}