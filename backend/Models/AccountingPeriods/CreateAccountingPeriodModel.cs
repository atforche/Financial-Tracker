namespace Models.AccountingPeriods;

/// <summary>
/// Model representing a request to create an Accounting Period
/// </summary>
public class CreateAccountingPeriodModel
{
    /// <summary>
    /// Year for the Accounting Period
    /// </summary>
    public required int Year { get; init; }

    /// <summary>
    /// Month for the Accounting Period
    /// </summary>
    public required int Month { get; init; }
}