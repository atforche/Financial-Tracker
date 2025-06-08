using Domain.Transactions;

namespace Rest.Models.Transactions;

/// <summary>
/// REST model representing a request to post a <see cref="Transaction"/>
/// </summary>
public class PostTransactionModel
{
    /// <inheritdoc cref="TransactionAccountType"/>
    public required TransactionAccountType AccountToPost { get; init; }

    /// <summary>
    /// Date to post this Transaction on
    /// </summary>
    public required DateOnly PostedStatementDate { get; init; }
}