namespace Models.AccountingPeriods;

/// <summary>
/// Model representing an Accounting Period with its Expected Income Sources.
/// </summary>
public class AccountingPeriodWithExpectedIncomeSourcesModel : AccountingPeriodModel
{
    /// <summary>
    /// Sources of expected income for the Accounting Period, with the income received from each so far.
    /// </summary>
    public required IReadOnlyCollection<ExpectedIncomeSourceModel> ExpectedIncomeSources { get; init; }
}
