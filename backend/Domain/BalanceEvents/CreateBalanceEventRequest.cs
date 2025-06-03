using Domain.AccountingPeriods;
using Domain.Accounts;

namespace Domain.BalanceEvents;

/// <summary>
/// Record representing a request to create a <see cref="IBalanceEvent"/>
/// </summary>
public abstract record CreateBalanceEventRequest
{
    /// <summary>
    /// Accounting Period ID for this Balance Event
    /// </summary>
    public required AccountingPeriodId AccountingPeriodId { get; init; }

    /// <summary>
    /// Event Date for this Balance Event
    /// </summary>
    public required DateOnly EventDate { get; init; }

    /// <summary>
    /// Account ID for this Balance Event
    /// </summary>
    public required AccountId AccountId { get; init; }
}