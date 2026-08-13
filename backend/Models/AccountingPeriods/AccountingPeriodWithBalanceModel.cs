namespace Models.AccountingPeriods;

/// <summary>
/// Model representing an Accounting Period along with its balance
/// </summary>
public class AccountingPeriodWithBalanceModel : AccountingPeriodModel
{
    /// <summary>
    /// Opening balance for the Accounting Period
    /// </summary>
    public required decimal OpeningBalance { get; init; }

    /// <summary>
    /// Closing balance for the Accounting Period
    /// </summary>
    public required decimal ClosingBalance { get; init; }

    /// <summary>
    /// Sources of expected income for the Accounting Period.
    /// </summary>
    public required IReadOnlyCollection<ExpectedIncomeSourceModel> ExpectedIncomeSources { get; init; }

    /// <summary>
    /// Income expected for the Accounting Period.
    /// </summary>
    public required IncomeAmountModel ExpectedIncome { get; init; }

    /// <summary>
    /// Posted income for the Accounting Period.
    /// </summary>
    public required IncomeAmountModel ActualIncome { get; init; }

    /// <summary>
    /// Amount required to satisfy all Fund Goals for the Accounting Period.
    /// </summary>
    public required decimal ExpectedGoalContributions { get; init; }
}