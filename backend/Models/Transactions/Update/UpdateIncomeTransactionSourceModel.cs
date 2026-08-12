using Models.Income;

namespace Models.Transactions.Update;

/// <summary>
/// Model representing the source of an income transaction update request.
/// </summary>
public sealed class UpdateIncomeTransactionSourceModel
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