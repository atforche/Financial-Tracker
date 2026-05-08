namespace Domain.Transactions;

/// <summary>
/// Enum representing the different Transaction types
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Spending Transaction
    /// </summary>
    Spending,

    /// <summary>
    /// Income Transaction
    /// </summary>
    Income,

    /// <summary>
    /// Account Transaction
    /// </summary>
    Account,

    /// <summary>
    /// Fund Transaction
    /// </summary>
    Fund,
}