namespace RestApi.Models.AccountingPeriod;

/// <summary>
/// Rest model representing an Accounting Period
/// </summary>
public class AccountingPeriodModel
{
    /// <summary>
    /// Id for this Accounting Period
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Year for this Accounting Period
    /// </summary>
    public required int Year { get; set; }

    /// <summary>
    /// Month for this Accounting Period
    /// </summary>
    public required int Month { get; set; }

    /// <summary>
    /// Is open flag for this Accounting Period
    /// </summary>
    public required bool IsOpen { get; set; }
}