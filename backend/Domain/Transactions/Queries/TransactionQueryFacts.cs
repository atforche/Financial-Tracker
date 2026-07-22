using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.FundPlans;
using Domain.Funds;

namespace Domain.Transactions.Queries;

/// <summary>
/// A Transaction page and its batched interpretation context.
/// </summary>
public sealed record TransactionQueryFacts(
    QueryPage<Transaction> Transactions,
    IReadOnlyCollection<AccountingPeriod> AccountingPeriods,
    IReadOnlyCollection<Fund> Funds,
    IReadOnlyCollection<AccountBalanceHistory> AccountHistories,
    IReadOnlyCollection<FundBalanceHistory> FundHistories,
    IReadOnlyCollection<FundPlanTotalsHistory> FundPlanHistories);