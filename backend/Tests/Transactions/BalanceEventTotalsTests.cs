using Domain.BalanceEvents;

namespace Tests.Transactions;

/// <summary>Checks posted credit and debit aggregation independent of daily net changes.</summary>
public sealed class BalanceEventTotalsTests
{
    /// <summary>Same-day opposite movements remain separate, and pending events are excluded.</summary>
    [Fact]
    public void CountsPostedCreditsAndDebitsSeparately()
    {
        TestEvent[] events =
        {
            new(true, BalanceEventType.Credit, 200m),
            new(true, BalanceEventType.Debit, 22.94m),
            new(false, BalanceEventType.Credit, 50m),
        };

        var totals = BalanceEventTotals.Calculate(events,
            item => item.IsPosted, item => item.Type, item => item.Amount);

        Assert.Equal(200m, totals.TotalInflow);
        Assert.Equal(22.94m, totals.TotalOutflow);
    }

    /// <summary>Orders posted event checkpoints and excludes pending events.</summary>
    [Fact]
    public void BuildsChartFromPostedEventsInTransactionSelection()
    {
        TestChartEvent[] events =
        {
            new(new DateOnly(2026, 8, 2), 2, 120m, 150m),
            new(null, null, 150m, 150m),
            new(new DateOnly(2026, 7, 30), 1, 100m, 120m),
            new(new DateOnly(2026, 8, 2), 1, 120m, 130m),
        };

        (decimal? openingBalance, IReadOnlyCollection<(DateOnly Date, decimal Balance)> dates) = BalanceEventTotals.BuildDateBalances(events,
            item => item.Date, item => item.Sequence,
            item => item.PreviousBalance, item => item.NewBalance);

        Assert.Equal(100m, openingBalance);
        Assert.Equal([(new DateOnly(2026, 7, 30), 120m), (new DateOnly(2026, 8, 2), 150m)], dates);
    }

    private sealed record TestEvent(bool IsPosted, BalanceEventType Type, decimal Amount);
    private sealed record TestChartEvent(DateOnly? Date, int? Sequence, decimal PreviousBalance, decimal NewBalance);
}
