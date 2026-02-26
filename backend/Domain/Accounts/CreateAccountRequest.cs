using Domain.AccountingPeriods;
using Domain.Funds;

namespace Domain.Accounts;

/// <summary>
/// Record representing a request to create an <see cref="Account"/>
/// </summary>
public record CreateAccountRequest
{
    /// <summary>
    /// Name for the Account
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Type for the Account
    /// </summary>
    public required AccountType Type { get; init; }

    /// <summary>
    /// Accounting Period that the Account is being added to
    /// </summary>
    public required AccountingPeriod AccountingPeriod { get; init; }

    /// <summary>
    /// Date the Account is being added
    /// </summary>
    public required DateOnly AddDate { get; init; }

    /// <summary>
    /// Initial amounts for each Fund in the Account
    /// </summary>
    public required IEnumerable<FundAmount> InitialFundAmounts { get; init; }
}