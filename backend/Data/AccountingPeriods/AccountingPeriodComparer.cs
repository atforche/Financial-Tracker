namespace Data.AccountingPeriods;

/// <summary>
/// Comparer class responsible for sorting two Accounting Periods
/// </summary>
internal sealed class AccountingPeriodComparer(AccountingPeriodSortOrder? sortBy) : IComparer<AccountingPeriodSortModel>
{
    /// <inheritdoc/>
    public int Compare(AccountingPeriodSortModel? x, AccountingPeriodSortModel? y)
    {
        if (x == null || y == null)
        {
            throw new ArgumentException("Cannot compare null Accounting Periods");
        }
        if (sortBy is AccountingPeriodSortOrder.IsOpen or AccountingPeriodSortOrder.IsOpenDescending &&
            TryCompareByIsOpen(x, y, out int isOpenResult))
        {
            return sortBy == AccountingPeriodSortOrder.IsOpen ? isOpenResult : -isOpenResult;
        }
        int dateResult = CompareByDate(x, y);
        return sortBy == AccountingPeriodSortOrder.Date ? dateResult : -dateResult;
    }

    /// <summary>
    /// Attempts to compare two Accounting Periods by whether they are open
    /// </summary>
    private static bool TryCompareByIsOpen(AccountingPeriodSortModel x, AccountingPeriodSortModel y, out int result)
    {
        result = x.AccountingPeriod.IsOpen.CompareTo(y.AccountingPeriod.IsOpen);
        return result != 0;
    }

    /// <summary>
    /// Compares two Accounting Periods by date
    /// </summary>
    private static int CompareByDate(AccountingPeriodSortModel x, AccountingPeriodSortModel y)
    {
        int yearResult = x.AccountingPeriod.Year.CompareTo(y.AccountingPeriod.Year);
        if (yearResult != 0)
        {
            return yearResult;
        }
        return x.AccountingPeriod.Month.CompareTo(y.AccountingPeriod.Month);
    }
}