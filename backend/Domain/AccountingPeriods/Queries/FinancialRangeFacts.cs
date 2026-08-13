using Domain.Accounts;

namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Persisted income destination facts for a financial range.
/// </summary>
public sealed record FinancialRangeIncomeFact(
    decimal Amount,
    AccountType AccountType,
    DateOnly? PostedDate);

/// <summary>
/// Persisted spending facts for a financial range.
/// </summary>
public sealed record FinancialRangeSpendingFact(decimal Amount, DateOnly? PostedDate);