using Domain;

namespace Tests.Domain;

/// <summary>
/// Covers how balance series choose their spacing and the dates they summarize.
/// </summary>
public sealed class BalanceSeriesTests
{
    /// <summary>
    /// Switches from daily to weekly after 180 days and from weekly to monthly after three years.
    /// </summary>
    [Fact]
    public void GranularityWidensWithRangeLength()
    {
        DateOnly start = new(2026, 1, 1);

        Assert.Equal(BalanceGranularity.Day, BalanceSeries.GetGranularity(start, new DateOnly(2026, 6, 29)));
        Assert.Equal(BalanceGranularity.Week, BalanceSeries.GetGranularity(start, new DateOnly(2026, 6, 30)));
        Assert.Equal(BalanceGranularity.Week, BalanceSeries.GetGranularity(start, new DateOnly(2029, 1, 1)));
        Assert.Equal(BalanceGranularity.Month, BalanceSeries.GetGranularity(start, new DateOnly(2029, 1, 2)));
        Assert.True(BalanceSeries.IsWithinMaximumRange(start, new DateOnly(2036, 1, 1)));
        Assert.False(BalanceSeries.IsWithinMaximumRange(start, new DateOnly(2036, 1, 2)));
        Assert.True(BalanceSeries.IsWithinMaximumRange(new DateOnly(2018, 2, 28), new DateOnly(2028, 2, 29)));
    }

    /// <summary>
    /// Samples the day before the range, then the end of each week or month, cutting the last one short at the end date.
    /// </summary>
    [Fact]
    public void SampleDatesEndEachBucketAndTheRange()
    {
        Assert.Equal(
            [new DateOnly(2026, 6, 30), new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 2)],
            BalanceSeries.GetSampleDates(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 2), BalanceGranularity.Day));
        Assert.Equal(
            [new DateOnly(2026, 6, 30), new DateOnly(2026, 7, 4), new DateOnly(2026, 7, 11), new DateOnly(2026, 7, 14)],
            BalanceSeries.GetSampleDates(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 14), BalanceGranularity.Week));
        Assert.Equal(
            [new DateOnly(2026, 6, 30), new DateOnly(2026, 7, 4), new DateOnly(2026, 7, 11)],
            BalanceSeries.GetSampleDates(new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 11), BalanceGranularity.Week));
        Assert.Equal(
            [new DateOnly(2026, 7, 3), new DateOnly(2026, 7, 4), new DateOnly(2026, 7, 11)],
            BalanceSeries.GetSampleDates(new DateOnly(2026, 7, 4), new DateOnly(2026, 7, 11), BalanceGranularity.Week));
        Assert.Equal(
            [new DateOnly(2026, 7, 14), new DateOnly(2026, 7, 31), new DateOnly(2026, 8, 31), new DateOnly(2026, 9, 10)],
            BalanceSeries.GetSampleDates(new DateOnly(2026, 7, 15), new DateOnly(2026, 9, 10), BalanceGranularity.Month));
    }

    /// <summary>
    /// Reports the latest balance as of each sample date, carrying balances through dates without history.
    /// </summary>
    [Fact]
    public void SummarizeCarriesTheLatestBalanceForward()
    {
        List<decimal> totals = BalanceSeries.Summarize(
            [new DateOnly(2026, 6, 30), new DateOnly(2026, 7, 4), new DateOnly(2026, 7, 11)],
            new Dictionary<string, decimal> { ["cash"] = 100m },
            [("cash", new DateOnly(2026, 7, 2), 1, 120m), ("cash", new DateOnly(2026, 7, 2), 2, 110m)],
            (_, balances) => balances["cash"]);

        Assert.Equal([100m, 110m, 110m], totals);
    }
}
