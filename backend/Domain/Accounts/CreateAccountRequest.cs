using Domain.AccountingPeriods;

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
    public required AccountingPeriod OpeningAccountingPeriod { get; init; }

    /// <summary>
    /// Date the Account is being opened
    /// </summary>
    public required DateOnly DateOpened { get; init; }
}