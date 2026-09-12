namespace Domain.AccountingPeriods;

/// <summary>Expands persisted within-period checkpoints into a daily balance series.</summary>
public static class PeriodBalanceDateSeries
{
    /// <summary>
    /// Includes every calendar day, carrying the last posted balance through quiet days. The
    /// calendar month is the minimum range; period events may extend it on either side.
    /// </summary>
    public static PeriodBalanceDateRange Build(
        AccountingPeriod period,
        decimal openingBalance,
        IEnumerable<(DateOnly Date, int Sequence, decimal Change)> checkpoints)
    {
        DateOnly first = new(period.Year, period.Month, 1);
        DateOnly last = new(period.Year, period.Month, DateTime.DaysInMonth(period.Year, period.Month));
        var ordered = checkpoints
            .OrderBy(item => item.Date).ThenBy(item => item.Sequence).ToList();
        DateOnly start = ordered.Count == 0 || ordered[0].Date >= first ? first : ordered[0].Date;
        DateOnly end = ordered.Count == 0 || ordered[^1].Date <= last ? last : ordered[^1].Date;
        decimal initialChange = ordered.Where(item => item.Date < start).Select(item => item.Change).LastOrDefault();
        int index = 0;
        decimal change = 0;
        var dates = new List<PeriodBalanceDate>();
        for (DateOnly date = start; date <= end; date = date.AddDays(1))
        {
            while (index < ordered.Count && ordered[index].Date <= date)
            {
                change = ordered[index++].Change;
            }
            dates.Add(new PeriodBalanceDate(date, openingBalance + change));
            if (date == end)
            {
                break;
            }
        }
        return new PeriodBalanceDateRange(openingBalance + initialChange, dates);
    }
}
