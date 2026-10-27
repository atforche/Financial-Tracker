using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;

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
/// Persisted effect of a Transaction that has not yet posted to one of its Accounts.
/// </summary>
public sealed record OverviewUnpostedEffect(
    AccountId AccountId,
    string AccountName,
    TransactionId TransactionId,
    DateOnly Date,
    decimal PendingDebitAmount,
    decimal PendingCreditAmount);
