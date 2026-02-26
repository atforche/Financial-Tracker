namespace Data.Transactions;

/// <summary>
/// Enum representing the different types of Transactions
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Debit transaction, where money is taken out of an account or fund
    /// </summary>
    Debit,

    /// <summary>
    /// Credit transaction, where money is put into an account or fund
    /// </summary>
    Credit,

    /// <summary>
    /// Transfer transaction, where money is moved within an account or fund
    /// </summary>
    Transfer,
}