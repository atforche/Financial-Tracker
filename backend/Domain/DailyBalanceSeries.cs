namespace Domain;

/// <summary>
/// Builds daily balance summaries from opening balances and posted balance history.
/// </summary>
public static class DailyBalanceSeries
{
    /// <summary>
    /// Summarizes balances for each date from the start date to the end date, inclusive, by advancing through the
    /// history once. The summarize function receives the latest balance for each ID as of the end of that date.
    /// </summary>
    public static List<TSummary> Summarize<TId, TSummary>(
        DateOnly start,
        DateOnly end,
        IReadOnlyDictionary<TId, decimal> openingBalances,
        IEnumerable<(TId Id, DateOnly Date, int Sequence, decimal Balance)> history,
        Func<DateOnly, IReadOnlyDictionary<TId, decimal>, TSummary> summarize)
        where TId : notnull
    {
        var currentBalances = new Dictionary<TId, decimal>(openingBalances);
        var orderedHistory = history
            .OrderBy(item => item.Date)
            .ThenBy(item => item.Sequence)
            .ToList();
        int historyIndex = 0;
        var summaries = new List<TSummary>();
        for (DateOnly date = start; date <= end; date = date.AddDays(1))
        {
            while (historyIndex < orderedHistory.Count && orderedHistory[historyIndex].Date <= date)
            {
                (TId id, _, _, decimal balance) = orderedHistory[historyIndex++];
                currentBalances[id] = balance;
            }
            summaries.Add(summarize(date, currentBalances));
            if (date == end)
            {
                break;
            }
        }
        return summaries;
    }
}
