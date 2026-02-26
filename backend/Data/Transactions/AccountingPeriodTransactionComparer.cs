namespace Data.Transactions;

/// <summary>
/// Comparer class responsible for sorting two Transactions within an Accounting Period
/// </summary>
internal sealed class AccountingPeriodTransactionComparer(AccountingPeriodTransactionSortOrder? sortBy) : IComparer<AccountingPeriodTransactionSortModel>
{
    /// <inheritdoc/>
    public int Compare(AccountingPeriodTransactionSortModel? x, AccountingPeriodTransactionSortModel? y)
    {
        if (x == null || y == null)
        {
            throw new ArgumentException("Cannot compare null Transactions");
        }
        if (sortBy is AccountingPeriodTransactionSortOrder.Location or AccountingPeriodTransactionSortOrder.LocationDescending &&
            TryCompareByLocation(x, y, out int locationResult))
        {
            return sortBy == AccountingPeriodTransactionSortOrder.Location ? locationResult : -locationResult;
        }
        if (sortBy is AccountingPeriodTransactionSortOrder.DebitAccount or AccountingPeriodTransactionSortOrder.DebitAccountDescending &&
            TryCompareByDebitAccount(x, y, out int debitAccountResult))
        {
            return sortBy == AccountingPeriodTransactionSortOrder.DebitAccount ? debitAccountResult : -debitAccountResult;
        }
        if (sortBy is AccountingPeriodTransactionSortOrder.CreditAccount or AccountingPeriodTransactionSortOrder.CreditAccountDescending &&
            TryCompareByCreditAccount(x, y, out int creditAccountResult))
        {
            return sortBy == AccountingPeriodTransactionSortOrder.CreditAccount ? creditAccountResult : -creditAccountResult;
        }
        if (sortBy is AccountingPeriodTransactionSortOrder.Amount or AccountingPeriodTransactionSortOrder.AmountDescending &&
            TryCompareByAmount(x, y, out int amountResult))
        {
            return sortBy == AccountingPeriodTransactionSortOrder.Amount ? amountResult : -amountResult;
        }
        int dateResult = x.Transaction.Date.CompareTo(y.Transaction.Date);
        return sortBy == AccountingPeriodTransactionSortOrder.DateDescending ? -dateResult : dateResult;
    }

    /// <summary>
    /// Attempts to compare two Transactions by their location
    /// </summary>
    private static bool TryCompareByLocation(AccountingPeriodTransactionSortModel x, AccountingPeriodTransactionSortModel y, out int result)
    {
        result = string.Compare(x.Transaction.Location, y.Transaction.Location, StringComparison.Ordinal);
        return result != 0;
    }

    /// <summary>
    /// Attempts to compare two Transactions by their debit account name
    /// </summary>
    private static bool TryCompareByDebitAccount(AccountingPeriodTransactionSortModel x, AccountingPeriodTransactionSortModel y, out int result)
    {
        result = string.Compare(x.DebitAccountName, y.DebitAccountName, StringComparison.Ordinal);
        return result != 0;
    }

    /// <summary>
    /// Attempts to compare two Transactions by their credit account name
    /// </summary>
    private static bool TryCompareByCreditAccount(AccountingPeriodTransactionSortModel x, AccountingPeriodTransactionSortModel y, out int result)
    {
        result = string.Compare(x.CreditAccountName, y.CreditAccountName, StringComparison.Ordinal);
        return result != 0;
    }

    /// <summary>
    /// Attempts to compare two Transactions by their amount
    /// </summary>
    private static bool TryCompareByAmount(AccountingPeriodTransactionSortModel x, AccountingPeriodTransactionSortModel y, out int result)
    {
        result = x.Transaction.Amount.CompareTo(y.Transaction.Amount);
        return result != 0;
    }
}