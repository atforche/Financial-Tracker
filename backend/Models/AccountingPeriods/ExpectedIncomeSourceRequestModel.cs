using Models.Transactions.Create;

namespace Models.AccountingPeriods;

/// <summary>
/// Request model for an expected income source.
/// </summary>
public sealed class ExpectedIncomeSourceRequestModel
{
    /// <summary>
    /// Source name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Income lines expected for each payment.
    /// </summary>
    public required IReadOnlyCollection<CreateIncomeLineModel> IncomeLines { get; init; }

    /// <summary>
    /// Deductions expected for each payment.
    /// </summary>
    public required IReadOnlyCollection<CreateIncomeDeductionModel> IncomeDeductions { get; init; }

    /// <summary>
    /// Transfers expected from each payment to untracked accounts.
    /// </summary>
    public required IReadOnlyCollection<ExpectedUntrackedIncomeTransferRequestModel> UntrackedTransfers { get; init; }

    /// <summary>
    /// Expected payment dates.
    /// </summary>
    public required IReadOnlyCollection<DateOnly> ExpectedDates { get; init; }
}