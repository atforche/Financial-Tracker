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
    /// Spending Transfer Transaction
    /// </summary>
    SpendingTransfer,

    /// <summary>
    /// Income Transaction
    /// </summary>
    Income,

    /// <summary>
    /// Income Transfer Transaction
    /// </summary>
    IncomeTransfer,

    /// <summary>
    /// Transfer Transaction
    /// </summary>
    Transfer,

    /// <summary>
    /// Refund Transaction
    /// </summary>
    Refund,
}