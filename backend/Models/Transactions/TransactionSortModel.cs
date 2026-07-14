namespace Models.Transactions;

/// <summary>
/// Enum representing the different ways top-level Transactions can be sorted
/// </summary>
public enum TransactionSortModel
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
    /// Transactions are sorted by accounting period in ascending order
    /// </summary>
    AccountingPeriod,

    /// <summary>
    /// Transactions are sorted by accounting period in descending order
    /// </summary>
    AccountingPeriodDescending,

    /// <summary>
    /// Transactions are sorted by description in ascending order
    /// </summary>
    Description,

    /// <summary>
    /// Transactions are sorted by description in descending order
    /// </summary>
    DescriptionDescending,

    /// <summary>
    /// Transactions are sorted by source in ascending order
    /// </summary>
    Source,

    /// <summary>
    /// Transactions are sorted by source in descending order
    /// </summary>
    SourceDescending,

    /// <summary>
    /// Transactions are sorted by destination in ascending order
    /// </summary>
    Destination,

    /// <summary>
    /// Transactions are sorted by destination in descending order
    /// </summary>
    DestinationDescending,

    /// <summary>
    /// Transactions are sorted by amount in ascending order
    /// </summary>
    Amount,

    /// <summary>
    /// Transactions are sorted by amount in descending order
    /// </summary>
    AmountDescending,
}