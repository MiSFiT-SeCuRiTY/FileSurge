using FileSurge.Services;
using Xunit;

namespace FileSurge.Tests;

public class SizeCalculatorTests
{
    [Fact]
    public void UnitMultiplier_ReturnsCorrectValues()
    {
        Assert.Equal(1L, SizeCalculator.UnitMultiplier("B"));
        Assert.Equal(1024L, SizeCalculator.UnitMultiplier("KB"));
        Assert.Equal(1024L * 1024, SizeCalculator.UnitMultiplier("MB"));
        Assert.Equal(1024L * 1024 * 1024, SizeCalculator.UnitMultiplier("GB"));
        Assert.Equal(1024L * 1024 * 1024 * 1024, SizeCalculator.UnitMultiplier("TB"));
    }

    [Fact]
    public void ComputeTargetSize_ExactMode_ReturnsBytes()
    {
        long result = SizeCalculator.ComputeTargetSize(0, TargetSizeMode.ExactSize, 500, "MB");
        Assert.Equal(500L * 1024 * 1024, result);
    }

    [Fact]
    public void ComputeTargetSize_AddMode_AddsToCurrent()
    {
        long result = SizeCalculator.ComputeTargetSize(100L * 1024 * 1024, TargetSizeMode.AddToCurrent, 50, "MB");
        Assert.Equal(150L * 1024 * 1024, result);
    }

    [Fact]
    public void ComputeTargetSize_MinMode_ReturnsLargerOfTwo()
    {
        long current = 200L * 1024 * 1024;
        long result1 = SizeCalculator.ComputeTargetSize(current, TargetSizeMode.MinimumSize, 100, "MB");
        Assert.Equal(current, result1);

        long result2 = SizeCalculator.ComputeTargetSize(current, TargetSizeMode.MinimumSize, 500, "MB");
        Assert.Equal(500L * 1024 * 1024, result2);
    }

    [Fact]
    public void ComputePadding_ReturnsDifference()
    {
        long padding = SizeCalculator.ComputePadding(100L * 1024 * 1024, 500L * 1024 * 1024);
        Assert.Equal(400L * 1024 * 1024, padding);
    }

    [Fact]
    public void ComputePadding_TargetSmallerThanCurrent_ReturnsZero()
    {
        long padding = SizeCalculator.ComputePadding(500L * 1024 * 1024, 100L * 1024 * 1024);
        Assert.Equal(0, padding);
    }

    [Fact]
    public void FormatBytes_FormatsCorrectly()
    {
        Assert.Equal("0 B", SizeCalculator.FormatBytes(0));
        Assert.Equal("1 KB", SizeCalculator.FormatBytes(1024));
        Assert.Equal("1 MB", SizeCalculator.FormatBytes(1024L * 1024));
        Assert.Equal("1 GB", SizeCalculator.FormatBytes(1024L * 1024 * 1024));
    }

    [Fact]
    public void ComputeTargetSize_NegativeValue_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SizeCalculator.ComputeTargetSize(0, TargetSizeMode.ExactSize, -1, "MB"));
    }

    [Fact]
    public void ComputeTargetSize_UnknownUnit_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            SizeCalculator.ComputeTargetSize(0, TargetSizeMode.ExactSize, 1, "XX"));
    }
}