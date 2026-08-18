using Models.Locations;

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
    public LocationInputModel? Location { get; init; }

    /// <summary>
    /// Income lines for the source.
    /// </summary>
    public required IReadOnlyCollection<CreateIncomeLineModel> IncomeLines { get; init; }

    /// <summary>
    /// Income deductions for the source.
    /// </summary>
    public required IReadOnlyCollection<CreateIncomeDeductionModel> IncomeDeductions { get; init; }
}
