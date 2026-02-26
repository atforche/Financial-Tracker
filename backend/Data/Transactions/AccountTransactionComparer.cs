namespace Data.Transactions;

/// <summary>
/// Comparer class responsible for sorting two Transactions within an Account
/// </summary>
internal sealed class AccountTransactionComparer(AccountTransactionSortOrder? sortBy) : IComparer<AccountTransactionSortModel>
{
    /// <inheritdoc/>
    public int Compare(AccountTransactionSortModel? x, AccountTransactionSortModel? y)
    {
        if (x == null || y == null)
        {
            throw new ArgumentException("Cannot compare null Transactions");
        }
        if (sortBy is AccountTransactionSortOrder.Location or AccountTransactionSortOrder.LocationDescending &&
            TryCompareByLocation(x, y, out int locationResult))
        {
            return sortBy == AccountTransactionSortOrder.Location ? locationResult : -locationResult;
        }
        if (sortBy is AccountTransactionSortOrder.Type or AccountTransactionSortOrder.TypeDescending &&
            TryCompareByType(x, y, out int typeResult))
        {
            return sortBy == AccountTransactionSortOrder.Type ? typeResult : -typeResult;
        }
        if (sortBy is AccountTransactionSortOrder.Amount or AccountTransactionSortOrder.AmountDescending &&
            TryCompareByAmount(x, y, out int amountResult))
        {
            return sortBy == AccountTransactionSortOrder.Amount ? amountResult : -amountResult;
        }
        if (TryCompareByAccountPostedDate(x, y, out int accountPostedDateResult))
        {
            return sortBy == AccountTransactionSortOrder.DateDescending ? -accountPostedDateResult : accountPostedDateResult;
        }
        int dateResult = CompareByDate(x, y);
        return sortBy == AccountTransactionSortOrder.DateDescending ? -dateResult : dateResult;
    }

    /// <summary>
    /// Attempts to compare two Transactions by their location
    /// </summary>
    private static bool TryCompareByLocation(AccountTransactionSortModel x, AccountTransactionSortModel y, out int result)
    {
        result = string.Compare(x.Transaction.Location, y.Transaction.Location, StringComparison.Ordinal);
        return result != 0;
    }

    /// <summary>
    /// Attempts to compare two Transactions by their type
    /// </summary>
    private static bool TryCompareByType(AccountTransactionSortModel x, AccountTransactionSortModel y, out int result)
    {
        result = string.Compare(x.TransactionType.ToString(), y.TransactionType.ToString(), StringComparison.Ordinal);
        return result != 0;
    }

    /// <summary>
    /// Attempts to compare two Transactions by their amount
    /// </summary>
    private static bool TryCompareByAmount(AccountTransactionSortModel x, AccountTransactionSortModel y, out int result)
    {
        result = x.Transaction.Amount.CompareTo(y.Transaction.Amount);
        return result != 0;
    }

    /// <summary>
    /// Attempts to compare two Transactions by their account posted date
    /// </summary>
    private static bool TryCompareByAccountPostedDate(AccountTransactionSortModel x, AccountTransactionSortModel y, out int result)
    {
        if (x.AccountPostedDate == null && y.AccountPostedDate == null)
        {
            result = 0;
            return false;
        }
        if (x.AccountPostedDate == null)
        {
            result = -1;
            return true;
        }
        if (y.AccountPostedDate == null)
        {
            result = 1;
            return true;
        }
        result = x.AccountPostedDate.Value.CompareTo(y.AccountPostedDate.Value);
        if (result == 0)
        {
            result = (x.AccountPostedSequence ?? -1).CompareTo(y.AccountPostedSequence ?? -1);
        }
        return result != 0;
    }

    /// <summary>
    /// Compares two Transactions by their date
    /// </summary>
    private static int CompareByDate(AccountTransactionSortModel x, AccountTransactionSortModel y)
    {
        int result = x.Transaction.Date.CompareTo(y.Transaction.Date);
        if (result == 0)
        {
            result = x.Transaction.Sequence.CompareTo(y.Transaction.Sequence);
        }
        return result;
    }
}