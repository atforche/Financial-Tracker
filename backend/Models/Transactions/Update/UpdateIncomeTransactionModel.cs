using Models.Transactions.Create;

namespace Models.Transactions.Update;

/// <summary>
/// Model representing a request to update an income transaction.
/// </summary>
public sealed class UpdateIncomeTransactionModel : UpdateTransactionModel
{
    /// <summary>
    /// Source for the income transaction.
    /// </summary>
    public required CreateIncomeTransactionSourceModel Source { get; init; }

    /// <summary>
    /// Destinations for the income transaction.
    /// </summary>
    public required IReadOnlyCollection<CreateIncomeTransactionDestinationModel> Destinations { get; init; }

    /// <summary>
    /// ID of the Expected Income Source this income was planned as, or null when the income is unplanned.
    /// </summary>
    public required Guid? ExpectedIncomeSourceId { get; init; }
}
