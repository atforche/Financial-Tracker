namespace Data.Funds;

/// <summary>
/// Comparer class responsible for sorting two Funds
/// </summary>
internal sealed class FundComparer(FundSortOrder? sortBy) : IComparer<FundSortModel>
{
    /// <inheritdoc/>
    public int Compare(FundSortModel? x, FundSortModel? y)
    {
        if (x == null || y == null)
        {
            throw new ArgumentException("Cannot compare null Funds");
        }
        if (sortBy is FundSortOrder.Balance or FundSortOrder.BalanceDescending &&
            TryCompareByBalance(x, y, out int balanceResult))
        {
            return sortBy == FundSortOrder.Balance ? balanceResult : -balanceResult;
        }
        if (sortBy is FundSortOrder.Description or FundSortOrder.DescriptionDescending &&
            TryCompareByDescription(x, y, out int descriptionResult))
        {
            return sortBy == FundSortOrder.Description ? descriptionResult : -descriptionResult;
        }
        int nameResult = string.Compare(x.Fund.Name, y.Fund.Name, StringComparison.Ordinal);
        return sortBy == FundSortOrder.NameDescending ? -nameResult : nameResult;
    }

    /// <summary>
    /// Attempts to compare two Funds by their balance
    /// </summary>
    private static bool TryCompareByBalance(FundSortModel x, FundSortModel y, out int result)
    {
        result = x.PostedBalance.CompareTo(y.PostedBalance);
        return result != 0;
    }

    /// <summary>
    /// Attempts to compare two Funds by their description
    /// </summary>
    private static bool TryCompareByDescription(FundSortModel x, FundSortModel y, out int result)
    {
        result = string.Compare(x.Fund.Description, y.Fund.Description, StringComparison.Ordinal);
        return result != 0;
    }
}