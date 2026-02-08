namespace Models.Transactions;

/// <summary>
/// Model representing a request to post a Transaction
/// </summary>
public class PostTransactionModel
{
    /// <summary>
    /// Account to post the Transaction to
    /// </summary>
    public required Guid AccountId { get; init; }

    /// <summary>
    /// Date for the Transaction
    /// </summary>
    public required DateOnly Date { get; init; }
}