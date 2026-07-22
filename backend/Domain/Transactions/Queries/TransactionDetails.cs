using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Accounts.Queries;
using Domain.BalanceEvents;
using Domain.FundPlans;
using Domain.FundPlans.Queries;
using Domain.Funds;
using Domain.Funds.Queries;

namespace Domain.Transactions.Queries;

/// <summary>
/// A Transaction and the Domain context required for pure response conversion.
/// </summary>
public sealed class TransactionDetails(TransactionDetailsFacts facts)
{
    private readonly Dictionary<FundId, Fund> _funds = facts.Funds.ToDictionary(fund => fund.Id);

    /// <summary>
    /// Gets the Transaction.
    /// </summary>
    public Transaction Transaction => facts.Transaction;

    /// <summary>
    /// Gets the Transaction's Accounting Period.
    /// </summary>
    public AccountingPeriod AccountingPeriod => facts.AccountingPeriod;

    /// <summary>
    /// Interprets an Account balance event for the Transaction.
    /// </summary>
    public AccountBalanceEvent GetAccountEvent(
        Account account,
        DateOnly? postedDate,
        decimal amount,
        BalanceEventType type)
    {
        var histories = facts.AccountHistories.Where(history => history.Account.Id == account.Id).ToList();
        AccountBalanceHistory? current = histories.LastOrDefault(history =>
            history.TransactionId == Transaction.Id && history.Date == (postedDate ?? Transaction.Date));
        int index = current == null ? -1 : histories.IndexOf(current);
        AccountBalanceHistory? previous = index > 0 ? histories[index - 1] : null;
        return new AccountBalanceEvent(
            AccountingPeriod,
            Transaction.Id,
            postedDate,
            type,
            amount,
            account,
            ToAccountBalance(account, previous, account.OnboardedBalance ?? 0),
            ToAccountBalance(account, current));
    }

    /// <summary>
    /// Interprets a Fund balance event for the Transaction.
    /// </summary>
    public FundBalanceEvent GetFundEvent(FundAmount amount, BalanceEventType type)
    {
        Fund fund = _funds[amount.FundId];
        var histories = facts.FundHistories.Where(history => history.Fund.Id == amount.FundId).ToList();
        FundBalanceHistory? current = histories.LastOrDefault(history => history.TransactionId == Transaction.Id);
        int index = current == null ? -1 : histories.IndexOf(current);
        FundBalanceHistory? previous = index > 0 ? histories[index - 1] : null;
        return new FundBalanceEvent(
            AccountingPeriod,
            Transaction.Id,
            Transaction.Date,
            type,
            amount.Amount,
            fund,
            ToFundBalance(fund, previous, fund.OnboardedBalance ?? 0),
            ToFundBalance(fund, current));
    }

    /// <summary>
    /// Interprets a Fund Plan balance event for the Transaction.
    /// </summary>
    public FundPlanBalanceEvent GetFundPlanEvent(
        FundAmount amount,
        DateOnly? postedDate,
        BalanceEventType type)
    {
        Fund fund = _funds[amount.FundId];
        var histories = facts.FundPlanHistories.Where(history =>
            history.FundId == amount.FundId && history.AccountingPeriodId == Transaction.AccountingPeriodId).ToList();
        FundPlanTotalsHistory? current = histories.SingleOrDefault(history => history.TransactionId == Transaction.Id);
        int index = current == null ? -1 : histories.IndexOf(current);
        FundPlanTotalsHistory? previous = index > 0 ? histories[index - 1] : null;
        return new FundPlanBalanceEvent(
            AccountingPeriod,
            Transaction.Id,
            postedDate,
            type,
            amount.Amount,
            fund,
            ToFundPlanTotals(amount.FundId, previous),
            ToFundPlanTotals(amount.FundId, current));
    }

    /// <summary>
    /// Creates an Account balance from persisted history.
    /// </summary>
    private static AccountBalance ToAccountBalance(
        Account account,
        AccountBalanceHistory? history,
        decimal fallback = 0) => new(
            account,
            history?.PostedBalance ?? fallback,
            history?.PendingDebitAmount ?? 0,
            history?.PendingCreditAmount ?? 0);

    /// <summary>
    /// Creates a Fund balance from persisted history.
    /// </summary>
    private static FundBalance ToFundBalance(
        Fund fund,
        FundBalanceHistory? history,
        decimal fallback = 0) => new(
            fund,
            history?.PostedBalance ?? fallback,
            history?.PendingDebitAmount ?? 0,
            history?.PendingCreditAmount ?? 0);

    /// <summary>
    /// Creates Fund Plan totals from persisted history.
    /// </summary>
    private static FundPlanTotals ToFundPlanTotals(FundId fundId, FundPlanTotalsHistory? history) => new(
        fundId,
        history?.AmountAssigned ?? 0,
        history?.PendingAmountAssigned ?? 0,
        history?.AmountSpent ?? 0,
        history?.PendingAmountSpent ?? 0);
}