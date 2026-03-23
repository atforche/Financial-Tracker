namespace Models.Transactions;

/// <summary>
/// Enum representing the different ways Transactions within a Fund can be sorted
/// </summary>
public enum FundTransactionSortOrderModel
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
    /// Transactions are sorted by change in balance in ascending order
    /// </summary>
    ChangeInBalance,

    /// <summary>
    /// Transactions are sorted by change in balance in descending order
    /// </summary>
    ChangeInBalanceDescending,
}