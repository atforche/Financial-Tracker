namespace Data.Transactions;

/// <summary>
/// Comparer class responsible for sorting two Transactions within a Fund
/// </summary>
internal sealed class FundTransactionComparer(FundTransactionSortOrder? sortBy) : IComparer<FundTransactionSortModel>
{
    /// <inheritdoc/>
    public int Compare(FundTransactionSortModel? x, FundTransactionSortModel? y)
    {
        if (x == null || y == null)
        {
            throw new ArgumentException("Cannot compare null Transactions");
        }
        if (sortBy is FundTransactionSortOrder.Location or FundTransactionSortOrder.LocationDescending &&
            TryCompareByLocation(x, y, out int locationResult))
        {
            return sortBy == FundTransactionSortOrder.Location ? locationResult : -locationResult;
        }
        if (sortBy is FundTransactionSortOrder.Type or FundTransactionSortOrder.TypeDescending &&
            TryCompareByType(x, y, out int typeResult))
        {
            return sortBy == FundTransactionSortOrder.Type ? typeResult : -typeResult;
        }
        if (sortBy is FundTransactionSortOrder.Amount or FundTransactionSortOrder.AmountDescending &&
            TryCompareByAmount(x, y, out int amountResult))
        {
            return sortBy == FundTransactionSortOrder.Amount ? amountResult : -amountResult;
        }
        int dateResult = CompareByDate(x, y);
        return sortBy == FundTransactionSortOrder.DateDescending ? -dateResult : dateResult;
    }

    /// <summary>
    /// Attempts to compare two Transactions by their date
    /// </summary>
    private static bool TryCompareByLocation(FundTransactionSortModel x, FundTransactionSortModel y, out int result)
    {
        result = string.Compare(x.Transaction.Location, y.Transaction.Location, StringComparison.Ordinal);
        return result != 0;
    }

    /// <summary>
    /// Attempts to compare two Transactions by their type
    /// </summary>
    private static bool TryCompareByType(FundTransactionSortModel x, FundTransactionSortModel y, out int result)
    {
        result = string.Compare(x.TransactionType.ToString(), y.TransactionType.ToString(), StringComparison.Ordinal);
        return result != 0;
    }

    /// <summary>
    /// Attempts to compare two Transactions by their amount
    /// </summary>
    private static bool TryCompareByAmount(FundTransactionSortModel x, FundTransactionSortModel y, out int result)
    {
        result = x.Transaction.Amount.CompareTo(y.Transaction.Amount);
        return result != 0;
    }

    /// <summary>
    /// Compares two Transactions by their date
    /// </summary>
    private static int CompareByDate(FundTransactionSortModel x, FundTransactionSortModel y)
    {
        int result = x.Transaction.Date.CompareTo(y.Transaction.Date);
        if (result == 0)
        {
            result = x.Transaction.Sequence.CompareTo(y.Transaction.Sequence);
        }
        return result;
    }
}