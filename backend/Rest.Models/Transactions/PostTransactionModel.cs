using Domain.Transactions;

namespace Rest.Models.Transactions;

/// <summary>
/// REST model representing a request to post a <see cref="Transaction"/>
/// </summary>
public class PostTransactionModel
{
    /// <summary>
    /// Account to post this Transaction in
    /// </summary>
    public required Guid AccountId { get; init; }

    /// <summary>
    /// Date to post this Transaction on
    /// </summary>
    public required DateOnly PostedStatementDate { get; init; }
}