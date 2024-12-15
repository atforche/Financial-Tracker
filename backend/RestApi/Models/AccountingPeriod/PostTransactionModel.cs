namespace RestApi.Models.AccountingPeriod;

/// <summary>
/// REST model representing a request to post a Transaction
/// </summary>
public class PostTransactionModel
{
    /// <summary>
    /// ID of the Account to post this Transaction in
    /// </summary>
    public required Guid AccountId { get; init; }

    /// <summary>
    /// Date to post this Transaction on
    /// </summary>
    public required DateOnly PostedStatementDate { get; init; }
}