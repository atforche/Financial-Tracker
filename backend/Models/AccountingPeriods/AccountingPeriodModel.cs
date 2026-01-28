namespace Models.AccountingPeriods;

/// <summary>
/// Model representing an Accounting Period
/// </summary>
public class AccountingPeriodModel
{
    /// <summary>
    /// ID for the Accounting Period
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Name of the Accounting Period
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Year for the Accounting Period
    /// </summary>
    public required int Year { get; init; }

    /// <summary>
    /// Month for the Accounting Period
    /// </summary>
    public required int Month { get; init; }

    /// <summary>
    /// True if the Accounting Period is open, false otherwise
    /// </summary>
    public required bool IsOpen { get; init; }
}