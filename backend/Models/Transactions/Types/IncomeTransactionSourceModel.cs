using Models.BalanceEvents;

namespace Models.Transactions.Types;

/// <summary>
/// Model representing the source of an income transaction response.
/// </summary>
public sealed class IncomeTransactionSourceModel
{
    /// <summary>
    /// Optional account for the source.
    /// </summary>
    public AccountBalanceEventModel? Account { get; init; }

    /// <summary>
    /// Optional location for the source.
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Income lines for the source.
    /// </summary>
    public required IReadOnlyCollection<IncomeLineModel> IncomeLines { get; init; }

    /// <summary>
    /// Income deductions for the source.
    /// </summary>
    public required IReadOnlyCollection<IncomeDeductionModel> IncomeDeductions { get; init; }
}