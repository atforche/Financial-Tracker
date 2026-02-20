namespace Models.Transactions;

/// <summary>
/// Enum representing the different ways Transactions within an Account can be sorted
/// </summary>
public enum AccountTransactionSortOrderModel
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
    /// Transactions are sorted by type in ascending order
    /// </summary>
    Type,

    /// <summary>
    /// Transactions are sorted by type in descending order
    /// </summary>
    TypeDescending,

    /// <summary>
    /// Transactions are sorted by amount in ascending order
    /// </summary>
    Amount,

    /// <summary>
    /// Transactions are sorted by amount in descending order
    /// </summary>
    AmountDescending,
}