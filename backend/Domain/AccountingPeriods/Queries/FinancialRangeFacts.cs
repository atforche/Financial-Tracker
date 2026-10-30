using Domain.Accounts;

namespace Domain.AccountingPeriods.Queries;

/// <summary>
/// Persisted income destination facts for a financial range, with the Expected Income Source the income was planned
/// as, if any.
/// </summary>
public sealed record FinancialRangeIncomeFact(
    decimal Amount,
    AccountType AccountType,
    DateOnly? PostedDate,
    ExpectedIncomeSourceId? ExpectedIncomeSourceId)
{
    /// <summary>
    /// Gets whether the income was planned as an Expected Income Source.
    /// </summary>
    public bool IsPlanned => ExpectedIncomeSourceId != null;
}

/// <summary>
/// Persisted spending facts for a financial range.
/// </summary>
public sealed record FinancialRangeSpendingFact(decimal Amount, DateOnly? PostedDate);
