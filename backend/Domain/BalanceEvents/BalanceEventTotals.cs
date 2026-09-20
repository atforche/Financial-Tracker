namespace Domain.BalanceEvents;

/// <summary>
/// Posted credit and debit totals across all matching balance events.
/// </summary>
public sealed record BalanceEventTotals(decimal TotalInflow, decimal TotalOutflow)
{
    /// <summary>
    /// Builds dated posted balance checkpoints for the selected events.
    /// </summary>
    public static (decimal? OpeningBalance, IReadOnlyCollection<(DateOnly Date, decimal Balance)> Dates) BuildDateBalances<T>(
        IEnumerable<T> events,
        Func<T, DateOnly?> getDate,
        Func<T, int?> getSequence,
        Func<T, decimal> getPreviousBalance,
        Func<T, decimal> getNewBalance)
    {
        var ordered = events.Where(item => getDate(item).HasValue)
            .OrderBy(item => getDate(item)).ThenBy(item => getSequence(item)).ToList();
        if (ordered.Count == 0)
        {
            return (null, []);
        }
        var dates = ordered.GroupBy(item => getDate(item)!.Value)
            .Select(group => (group.Key, getNewBalance(group.Last()))).ToList();
        return (getPreviousBalance(ordered[0]), dates);
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
