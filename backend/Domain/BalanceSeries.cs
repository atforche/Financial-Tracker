namespace Domain;

/// <summary>
/// Builds balance summaries over a date range from opening balances and posted balance history.
/// </summary>
public static class BalanceSeries
{
    /// <summary>
    /// Longest date range, in years, that a balance series may cover.
    /// </summary>
    public const int MaximumRangeYears = 10;

    /// <summary>
    /// Longest date range, in days, that is summarized one day at a time.
    /// </summary>
    private const int MaximumDailyRangeDays = 180;

    /// <summary>
    /// Longest date range, in years, that is summarized one week at a time.
    /// </summary>
    private const int MaximumWeeklyRangeYears = 3;

    /// <summary>
    /// Determines whether a date range is short enough to summarize. The limit counts back from the end date, matching
    /// how the frontend shortens long ranges, so a leap-day end date allows the same start date on both sides.
    /// </summary>
    public static bool IsWithinMaximumRange(DateOnly start, DateOnly end) => start >= end.AddYears(-MaximumRangeYears);

    /// <summary>
    /// Chooses the spacing that keeps a date range's series to a readable number of points.
    /// </summary>
    public static BalanceGranularity GetGranularity(DateOnly start, DateOnly end) =>
        end.DayNumber - start.DayNumber < MaximumDailyRangeDays ? BalanceGranularity.Day
        : end <= start.AddYears(MaximumWeeklyRangeYears) ? BalanceGranularity.Week
        : BalanceGranularity.Month;

    /// <summary>
    /// Gets the dates to summarize for a date range. The first date is the day before the range, whose balances are
    /// the opening balances. The rest are the last date of each day, week, or month in the range, with the final one
    /// cut short at the end date.
    /// </summary>
    public static List<DateOnly> GetSampleDates(DateOnly start, DateOnly end, BalanceGranularity granularity)
    {
        DateOnly openingDate = start.AddDays(-1);
        var dates = new List<DateOnly> { openingDate };
        for (DateOnly date = GetNextSampleDate(openingDate, granularity); date < end; date = GetNextSampleDate(date, granularity))
        {
            dates.Add(date);
        }
        dates.Add(end);
        return dates;
    }

    /// <summary>
    /// Summarizes balances as of the end of each sample date by advancing through the history once. The summarize
    /// function receives the latest balance for each ID as of the end of that date.
    /// </summary>
    public static List<TSummary> Summarize<TId, TSummary>(
        IReadOnlyList<DateOnly> sampleDates,
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
        foreach (DateOnly date in sampleDates)
        {
            while (historyIndex < orderedHistory.Count && orderedHistory[historyIndex].Date <= date)
            {
                (TId id, _, _, decimal balance) = orderedHistory[historyIndex++];
                currentBalances[id] = balance;
            }
            summaries.Add(summarize(date, currentBalances));
        }
        return summaries;
    }

    /// <summary>
    /// Gets the first day, Saturday, or month end after the provided date.
    /// </summary>
    private static DateOnly GetNextSampleDate(DateOnly date, BalanceGranularity granularity) => granularity switch
    {
        BalanceGranularity.Day => date.AddDays(1),
        BalanceGranularity.Week => date.AddDays(7 - (((int)date.DayOfWeek + 1) % 7)),
        BalanceGranularity.Month => GetEndOfMonth(date.AddDays(1)),
        _ => throw new ArgumentOutOfRangeException(nameof(granularity)),
    };

    /// <summary>
    /// Gets the last day of the month containing the provided date.
    /// </summary>
    private static DateOnly GetEndOfMonth(DateOnly date) =>
        new(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
}
