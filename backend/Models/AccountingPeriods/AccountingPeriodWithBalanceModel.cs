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
    /// Income received during the Accounting Period that was not planned as any Expected Income Source.
    /// </summary>
    public required IncomeAmountModel UnplannedIncome { get; init; }

    /// <summary>
    /// Sum of the planned monthly contributions for all Fund Goals in the Accounting Period.
    /// </summary>
    public required decimal PlannedGoalContributions { get; init; }

    /// <summary>
    /// Amount required to satisfy all Fund Goals for the Accounting Period.
    /// </summary>
    public required decimal ExpectedGoalContributions { get; init; }

    /// <summary>
    /// Amount assigned toward expected monthly contributions for all Fund Goals in the Accounting Period.
    /// </summary>
    public required decimal ActualGoalContributions { get; init; }

    /// <summary>
    /// Amount of unplanned income assigned to Fund Goals in the Accounting Period, which does not count toward their
    /// contributions.
    /// </summary>
    public required decimal ActualUnplannedGoalContributions { get; init; }
}
