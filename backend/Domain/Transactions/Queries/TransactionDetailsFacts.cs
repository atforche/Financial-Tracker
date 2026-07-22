using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.FundPlans;
using Domain.Funds;

namespace Domain.Transactions.Queries;

/// <summary>
/// Persisted facts required to interpret a Transaction response.
/// </summary>
public sealed record TransactionDetailsFacts(
    Transaction Transaction,
    AccountingPeriod AccountingPeriod,
    IReadOnlyCollection<Fund> Funds,
    IReadOnlyCollection<AccountBalanceHistory> AccountHistories,
    IReadOnlyCollection<FundBalanceHistory> FundHistories,
    IReadOnlyCollection<FundPlanTotalsHistory> FundPlanHistories);