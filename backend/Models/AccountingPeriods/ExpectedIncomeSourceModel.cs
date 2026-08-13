using Models.Transactions.Types;

namespace Models.AccountingPeriods;

/// <summary>
/// Expected income from a named source during an Accounting Period.
/// </summary>
public sealed class ExpectedIncomeSourceModel
{
    /// <summary>
    /// Identifier for the source.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Source name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Income lines expected for each payment.
    /// </summary>
    public required IReadOnlyCollection<IncomeLineModel> IncomeLines { get; init; }

    /// <summary>
    /// Deductions expected for each payment.
    /// </summary>
    public required IReadOnlyCollection<IncomeDeductionModel> IncomeDeductions { get; init; }

    /// <summary>
    /// Expected payment dates.
    /// </summary>
    public required IReadOnlyCollection<DateOnly> ExpectedDates { get; init; }

    /// <summary>
    /// Net income expected for one payment.
    /// </summary>
    public required IncomeAmountModel NetAmount { get; init; }

    /// <summary>
    /// Total income expected for this source.
    /// </summary>
    public required IncomeAmountModel ExpectedAmount { get; init; }

    /// <summary>
    /// Transfers expected from each payment to untracked accounts.
    /// </summary>
    public required IReadOnlyCollection<ExpectedUntrackedIncomeTransferModel> UntrackedTransfers { get; init; }
}