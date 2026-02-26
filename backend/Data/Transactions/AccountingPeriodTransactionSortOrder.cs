namespace Data.Transactions;

/// <summary>
/// Enum representing the different ways Transactions within an Accounting Period can be sorted
/// </summary>
public enum AccountingPeriodTransactionSortOrder
{
    /// <summary>
    /// Transactions are sorted by date in ascending order
    /// </summary>
    Date,

    /// <summary>
    /// Transactions are sorted by date in descending order
    /// </summary>
    DateDescending,

    /// <summary>
    /// Transactions are sorted by location in ascending order
    /// </summary>
    Location,

    /// <summary>
    /// Transactions are sorted by location in descending order
    /// </summary>
    LocationDescending,

    /// <summary>
    /// Transactions are sorted by debit account in ascending order
    /// </summary>
    DebitAccount,

    /// <summary>
    /// Transactions are sorted by debit account in descending order
    /// </summary>
    DebitAccountDescending,

    /// <summary>
    /// Transactions are sorted by credit account in ascending order
    /// </summary>
    CreditAccount,

    /// <summary>
    /// Transactions are sorted by credit account in descending order
    /// </summary>
    CreditAccountDescending,

    /// <summary>
    /// Transactions are sorted by amount in ascending order
    /// </summary>
    Amount,

    /// <summary>
    /// Transactions are sorted by amount in descending order
    /// </summary>
    AmountDescending,
}