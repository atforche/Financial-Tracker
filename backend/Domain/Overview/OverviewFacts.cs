using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;

namespace Domain.Overview;

/// <summary>
/// Persisted Account facts and the posted balance before an overview date range.
/// </summary>
public sealed record OverviewAccountOpeningBalance(AccountId AccountId, AccountType Type, DateOnly? DateOpened, decimal Balance);

/// <summary>
/// Persisted Fund posted balance before an overview date range.
/// </summary>
public sealed record OverviewFundOpeningBalance(FundId FundId, decimal Balance);

/// <summary>
/// Persisted Account balances for an Accounting Period.
/// </summary>
public sealed record OverviewAccountPeriodBalance(
    AccountingPeriodId AccountingPeriodId,
    AccountType Type,
    decimal OpeningBalance,
    decimal ClosingBalance);

/// <summary>
/// Persisted Fund balances for an Accounting Period.
/// </summary>
public sealed record OverviewFundPeriodBalance(
    AccountingPeriodId AccountingPeriodId,
    FundId FundId,
    decimal OpeningBalance,
    decimal ClosingBalance);
