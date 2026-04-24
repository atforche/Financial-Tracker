namespace Models.Transactions;

/// <summary>
/// Enum representing the different transaction types exposed by the REST API.
/// </summary>
public enum TransactionTypeModel
{
    /// <summary>
    /// Spending transaction.
    /// </summary>
    Spending,

    /// <summary>
    /// Income transaction.
    /// </summary>
    Income,

    /// <summary>
    /// Account transaction.
    /// </summary>
    Account,

    /// <summary>
    /// Fund transaction.
    /// </summary>
    Fund,
}