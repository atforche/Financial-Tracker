using Models.Locations;

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
    public LocationInputModel? Location { get; init; }

    /// <summary>
    /// Income lines for the source.
    /// </summary>
    public required IReadOnlyCollection<UpdateIncomeLineModel> IncomeLines { get; init; }

    /// <summary>
    /// Income deductions for the source.
    /// </summary>
    public required IReadOnlyCollection<UpdateIncomeDeductionModel> IncomeDeductions { get; init; }
}
