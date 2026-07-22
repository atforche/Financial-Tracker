using Domain.Accounts;

namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Persisted facts for an income destination in an Accounting Period range.
/// </summary>
public sealed record AccountingPeriodRangeIncomeFact(
    decimal Amount,
    AccountType AccountType,
    bool HasInternalSource,
    DateOnly? PostedDate);

/// <summary>
/// Persisted facts for spending in an Accounting Period range.
/// </summary>
public sealed record AccountingPeriodRangeSpendingFact(decimal Amount, DateOnly? PostedDate);