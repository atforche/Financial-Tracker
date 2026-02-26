namespace Data.Accounts;

/// <summary>
/// Comparer class responsible for sorting two Accounts
/// </summary>
internal sealed class AccountComparer(AccountSortOrder? sortBy) : IComparer<AccountSortModel>
{
    /// <inheritdoc/>
    public int Compare(AccountSortModel? x, AccountSortModel? y)
    {
        if (x == null || y == null)
        {
            throw new ArgumentException("Cannot compare null Accounts");
        }
        if (sortBy is AccountSortOrder.PostedBalance or AccountSortOrder.PostedBalanceDescending &&
            TryCompareByPostedBalance(x, y, out int postedBalanceResult))
        {
            return sortBy == AccountSortOrder.PostedBalance ? postedBalanceResult : -postedBalanceResult;
        }
        if (sortBy is AccountSortOrder.AvailableToSpend or AccountSortOrder.AvailableToSpendDescending &&
            TryCompareByAvailableToSpend(x, y, out int availableToSpendResult))
        {
            return sortBy == AccountSortOrder.AvailableToSpend ? availableToSpendResult : -availableToSpendResult;
        }
        if (sortBy is AccountSortOrder.Type or AccountSortOrder.TypeDescending &&
            TryCompareByType(x, y, out int typeResult))
        {
            return sortBy == AccountSortOrder.Type ? typeResult : -typeResult;
        }
        int nameResult = string.Compare(x.Account.Name, y.Account.Name, StringComparison.Ordinal);
        return sortBy == AccountSortOrder.NameDescending ? -nameResult : nameResult;
    }

    /// <summary>
    /// Attempts to compare two Accounts by their posted balance
    /// </summary>
    private static bool TryCompareByPostedBalance(AccountSortModel x, AccountSortModel y, out int result)
    {
        result = x.PostedBalance.CompareTo(y.PostedBalance);
        return result != 0;
    }

    /// <summary>
    /// Attempts to compare two Accounts by their available to spend balance
    /// </summary>
    private static bool TryCompareByAvailableToSpend(AccountSortModel x, AccountSortModel y, out int result)
    {
        result = x.AvailableToSpend.CompareTo(y.AvailableToSpend);
        return result != 0;
    }

    /// <summary>
    /// Attempts to compare two Accounts by their type
    /// </summary>
    private static bool TryCompareByType(AccountSortModel x, AccountSortModel y, out int result)
    {
        result = string.Compare(x.Account.Type.ToString(), y.Account.Type.ToString(), StringComparison.Ordinal);
        return result != 0;
    }
}