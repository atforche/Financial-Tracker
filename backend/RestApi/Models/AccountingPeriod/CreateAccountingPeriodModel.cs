namespace RestApi.Models.AccountingPeriod;

/// <summary>
/// Rest model representing a request to create an Accounting Period
/// </summary>
public class CreateAccountingPeriodModel
{
    /// <summary>
    /// Year for this Accounting Period
    /// </summary>
    public required int Year { get; set; }

    /// <summary>
    /// Month for this Accounting Period
    /// </summary>
    public required int Month { get; set; }
}