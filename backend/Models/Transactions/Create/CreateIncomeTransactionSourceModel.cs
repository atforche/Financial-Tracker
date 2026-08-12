using Models.Income;

namespace Models.Transactions.Create;

/// <summary>
/// Model representing the source of an income transaction create request.
/// </summary>
public sealed class CreateIncomeTransactionSourceModel
{
    /// <summary>
    /// Optional account ID for the income source.
    /// </summary>
    public Guid? AccountId { get; init; }

    /// <summary>
    /// Optional location for the income source.
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Economic composition of the income receipt.
    /// </summary>
    public required IncomeBreakdownRequestModel Income { get; init; }
}