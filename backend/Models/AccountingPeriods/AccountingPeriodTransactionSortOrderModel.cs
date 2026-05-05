namespace Models.AccountingPeriods;

/// <summary>
/// Model representing the different ways Transactions within an Accounting Period can be sorted
/// </summary>
public enum AccountingPeriodTransactionSortOrderModel
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
    /// Transactions are sorted by debit from in ascending order
    /// </summary>
    DebitFrom,

    /// <summary>
    /// Transactions are sorted by debit from in descending order
    /// </summary>
    DebitFromDescending,

    /// <summary>
    /// Transactions are sorted by credit to in ascending order
    /// </summary>
    CreditTo,

    /// <summary>
    /// Transactions are sorted by credit to in descending order
    /// </summary>
    CreditToDescending,

    /// <summary>
    /// Transactions are sorted by amount in ascending order
    /// </summary>
    Amount,

    /// <summary>
    /// Transactions are sorted by amount in descending order
    /// </summary>
    AmountDescending,
}