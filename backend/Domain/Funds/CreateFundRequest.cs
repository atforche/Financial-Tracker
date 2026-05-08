using Domain.AccountingPeriods;

namespace Domain.Funds;

/// <summary>
/// Record representing a request to create a <see cref="Fund"/>
/// </summary>
public record CreateFundRequest
{
    /// <summary>
    /// Name for the Fund
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Whether the Fund is a system-defined fund
    /// </summary>
    public bool IsSystemFund { get; init; }

    /// <summary>
    /// Description for the Fund
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Accounting Period that the Fund is being added to
    /// </summary>
    public required AccountingPeriod AccountingPeriod { get; init; }
}