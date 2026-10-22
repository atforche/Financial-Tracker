namespace Domain.BalanceEvents;

/// <summary>
/// Posted credit and debit totals across all matching balance events.
/// </summary>
public sealed record BalanceEventTotals(decimal TotalInflow, decimal TotalOutflow)
{
    /// <summary>
    /// Builds dated posted balance checkpoints for the selected events. Only posted events, which have a date
    /// and balances, are used.
    /// </summary>
    public static (decimal? OpeningBalance, IReadOnlyCollection<(DateOnly Date, decimal Balance)> Dates) BuildDateBalances<T>(
        IEnumerable<T> events,
        Func<T, DateOnly?> getDate,
        Func<T, int?> getSequence,
        Func<T, decimal?> getPreviousBalance,
        Func<T, decimal?> getNewBalance)
    {
        var ordered = events
            .Select(item => (Date: getDate(item), Sequence: getSequence(item), Previous: getPreviousBalance(item), New: getNewBalance(item)))
            .Where(item => item.Date.HasValue && item.Previous.HasValue && item.New.HasValue)
            .OrderBy(item => item.Date).ThenBy(item => item.Sequence).ToList();
        if (ordered.Count == 0)
        {
            return (null, []);
        }
        var dates = ordered.GroupBy(item => item.Date!.Value)
            .Select(group => (group.Key, group.Last().New!.Value)).ToList();
        return (ordered[0].Previous!.Value, dates);
    }

    /// <summary>
    /// Calculates totals before event pagination is applied.
    /// </summary>
    public static BalanceEventTotals Calculate<T>(
        IEnumerable<T> events,
        Func<T, bool> isPosted,
        Func<T, BalanceEventType> getType,
        Func<T, decimal> getAmount)
    {
        decimal inflow = 0;
        decimal outflow = 0;
        foreach (T balanceEvent in events.Where(isPosted))
        {
            if (getType(balanceEvent) == BalanceEventType.Credit)
            {
                inflow += getAmount(balanceEvent);
            }
            else
            {
                outflow += getAmount(balanceEvent);
            }
        }
        return new BalanceEventTotals(inflow, outflow);
    }
}
