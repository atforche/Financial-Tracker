using Domain.Accounts;
using Domain.FundPlans;
using Domain.Funds;
using Domain.Transactions;

namespace Domain.AccountingPeriods;

/// <summary>
/// Service for managing Accounting Period Balances
/// </summary>
public class AccountingPeriodBalanceService(
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository,
    IAccountRepository accountRepository,
    IFundRepository fundRepository,
    IFundPlanRepository fundPlanRepository,
    AccountBalanceService accountBalanceService,
    FundBalanceService fundBalanceService)
{
    /// <summary>
    /// Updates the Accounting Period Balances for a newly added Accounting Period
    /// </summary>
    internal void AddAccountingPeriod(AccountingPeriod newAccountingPeriod)
    {
        IEnumerable<AccountingPeriodAccountBalanceHistory> accountBalanceHistories = [];
        foreach (Account account in accountRepository.GetAll())
        {
            AccountBalance currentBalance = accountBalanceService.GetCurrentBalance(account);
            accountBalanceHistories = accountBalanceHistories.Append(new AccountingPeriodAccountBalanceHistory(
                account,
                newAccountingPeriod,
                currentBalance.PostedBalance,
                currentBalance.PostedBalance));
        }
        IEnumerable<AccountingPeriodFundBalanceHistory> fundBalanceHistories = [];
        IEnumerable<AccountingPeriodFundPlanTotals> fundPlanTotals = [];
        foreach (Fund fund in fundRepository.GetAll())
        {
            FundBalance currentBalance = fundBalanceService.GetCurrentBalance(fund.Id);
            currentBalance = new FundBalance(fund, currentBalance.PostedBalance, 0, 0);
            fundBalanceHistories = fundBalanceHistories.Append(new AccountingPeriodFundBalanceHistory(
                fund,
                newAccountingPeriod,
                currentBalance,
                currentBalance));
            if (!fund.IsUnassignedFund)
            {
                _ = fundPlanRepository.GetByFundAndAccountingPeriod(fund.Id, newAccountingPeriod.Id)
                    ?? throw new InvalidOperationException("Fund is missing its Fund Plan. Fund ID: " + fund.Id);
                fundPlanTotals = fundPlanTotals.Append(new AccountingPeriodFundPlanTotals(
                    fund,
                    newAccountingPeriod,
                    new FundPlanTotals(fund.Id, 0, 0, 0, 0)));
            }
        }
        accountingPeriodBalanceHistoryRepository.Add(new AccountingPeriodBalanceHistory(
            newAccountingPeriod,
            accountBalanceHistories,
            fundBalanceHistories,
            fundPlanTotals));
    }

    /// <summary>
    /// Updates the Accounting Period Balances for a deleted Accounting Period
    /// </summary>
    internal void DeleteAccountingPeriod(AccountingPeriod accountingPeriod) =>
        accountingPeriodBalanceHistoryRepository.Delete(accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(accountingPeriod.Id));

    /// <summary>
    /// Updates the Accounting Period Balances for a newly added Fund
    /// </summary>
    internal void AddFund(Fund newFund)
    {
        if (newFund.OpeningAccountingPeriodId == null)
        {
            return;
        }
        AccountingPeriod? accountingPeriod = accountingPeriodRepository.GetById(newFund.OpeningAccountingPeriodId);
        while (accountingPeriod != null)
        {
            AccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(accountingPeriod.Id);
            var balance = new FundBalance(newFund, 0, 0, 0);
            balanceHistory.AddFundBalance(new AccountingPeriodFundBalanceHistory(newFund, accountingPeriod, balance, balance));
            if (!newFund.IsUnassignedFund)
            {
                _ = fundPlanRepository.GetByFundAndAccountingPeriod(newFund.Id, accountingPeriod.Id)
                    ?? throw new InvalidOperationException("Fund is missing its Fund Plan. Fund ID: " + newFund.Id);
                balanceHistory.AddFundPlanTotals(new AccountingPeriodFundPlanTotals(
                    newFund,
                    accountingPeriod,
                    new FundPlanTotals(newFund.Id, 0, 0, 0, 0)));
            }
            accountingPeriod = accountingPeriodRepository.GetNextAccountingPeriod(accountingPeriod.Id);
        }
    }

    /// <summary>
    /// Updates the Accounting Period Balances for a deleted Fund
    /// </summary>
    internal void DeleteFund(Fund fund)
    {
        if (fund.OpeningAccountingPeriodId == null)
        {
            return;
        }
        AccountingPeriod? accountingPeriod = accountingPeriodRepository.GetById(fund.OpeningAccountingPeriodId);
        while (accountingPeriod != null)
        {
            AccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(accountingPeriod.Id);
            balanceHistory.RemoveFundBalance(fund.Id);
            balanceHistory.RemoveFundPlanTotals(fund.Id);
            accountingPeriod = accountingPeriodRepository.GetNextAccountingPeriod(accountingPeriod.Id);
        }
    }

    /// <summary>
    /// Updates the Accounting Period Balances for a newly added Account
    /// </summary>
    internal void AddAccount(Account newAccount)
    {
        if (newAccount.OpeningAccountingPeriodId == null)
        {
            return;
        }
        AccountingPeriod? accountingPeriod = accountingPeriodRepository.GetById(newAccount.OpeningAccountingPeriodId);
        while (accountingPeriod != null)
        {
            AccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(accountingPeriod.Id);
            balanceHistory.AddAccountBalance(new AccountingPeriodAccountBalanceHistory(newAccount, accountingPeriod, 0, 0));
            accountingPeriod = accountingPeriodRepository.GetNextAccountingPeriod(accountingPeriod.Id);
        }
    }

    /// <summary>
    /// Updates the Accounting Period Balances for a deleted Account
    /// </summary>
    internal void DeleteAccount(Account account)
    {
        if (account.OpeningAccountingPeriodId == null)
        {
            return;
        }
        AccountingPeriod? accountingPeriod = accountingPeriodRepository.GetById(account.OpeningAccountingPeriodId);
        while (accountingPeriod != null)
        {
            AccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(accountingPeriod.Id);
            balanceHistory.RemoveAccountBalance(account.Id);
            accountingPeriod = accountingPeriodRepository.GetNextAccountingPeriod(accountingPeriod.Id);
        }
    }

    /// <summary>
    /// Updates the Accounting Period Balances for a newly added Transaction
    /// </summary>
    internal void AddTransaction(Transaction transaction)
    {
        AccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(transaction.AccountingPeriodId);
        foreach (AccountingPeriodFundBalanceHistory fundBalanceHistory in GetAffectedFundBalanceHistories(balanceHistory, transaction, null))
        {
            FundBalance openingBalance = fundBalanceHistory.GetOpeningFundBalance();
            FundBalance closingBalance = transaction.ApplyToFundBalance(fundBalanceHistory.GetClosingFundBalance());
            fundBalanceHistory.Update(openingBalance, closingBalance);
        }
        UpdateFundPlanTotals(balanceHistory, transaction, null, false, false);
    }

    /// <summary>
    /// Updates the Accounting Period Balances for a newly posted Transaction
    /// </summary>
    internal void PostTransaction(Transaction transaction, AccountId accountId)
    {
        DateOnly? postedDate = transaction.GetPostedDateForAccount(accountId);
        if (postedDate == null)
        {
            return;
        }
        AccountingPeriod? accountingPeriod = accountingPeriodRepository.GetById(transaction.AccountingPeriodId);
        while (accountingPeriod != null)
        {
            AccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(accountingPeriod.Id);

            AccountingPeriodAccountBalanceHistory accountBalanceHistory = balanceHistory.AccountBalances.Single(a => a.Account.Id == accountId);
            if (transaction.AccountingPeriodId != accountingPeriod.Id)
            {
                AccountBalance openingBalance = accountBalanceHistory.GetOpeningAccountBalance();
                accountBalanceHistory.OpeningBalance = transaction.ApplyToAccountBalance(openingBalance).PostedBalance;
            }
            AccountBalance closingBalance = accountBalanceHistory.GetClosingAccountBalance();
            accountBalanceHistory.ClosingBalance = transaction.ApplyToAccountBalance(closingBalance).PostedBalance;

            List<AccountingPeriodFundBalanceHistory> fundBalanceHistories = GetAffectedFundBalanceHistories(balanceHistory, transaction, accountId);
            foreach (AccountingPeriodFundBalanceHistory fundBalanceHistory in fundBalanceHistories)
            {
                FundBalance openingFundBalance = fundBalanceHistory.GetOpeningFundBalance();
                if (transaction.AccountingPeriodId != accountingPeriod.Id)
                {
                    openingFundBalance = new FundBalance(
                        openingFundBalance.Fund,
                        transaction.ApplyToFundBalance(openingFundBalance, accountId: accountId).PostedBalance,
                        0,
                        0);
                }
                FundBalance closingFundBalance = fundBalanceHistory.GetClosingFundBalance();
                closingFundBalance = transaction.AccountingPeriodId != accountingPeriod.Id
                    ? new FundBalance(
                        closingFundBalance.Fund,
                        transaction.ApplyToFundBalance(closingFundBalance, accountId: accountId).PostedBalance,
                        0,
                        0)
                    : transaction.ApplyToFundBalance(closingFundBalance, accountId: accountId, postingOnly: true);
                fundBalanceHistory.Update(openingFundBalance, closingFundBalance);
            }
            balanceHistory.UpdateBalances();
            if (transaction.AccountingPeriodId == accountingPeriod.Id)
            {
                UpdateFundPlanTotals(balanceHistory, transaction, accountId, false, true);
            }
            accountingPeriod = accountingPeriodRepository.GetNextAccountingPeriod(accountingPeriod.Id);
        }
    }

    /// <summary>
    /// Updates the Accounting Period Balances for a newly unposted Transaction
    /// </summary>
    internal void UnpostTransaction(Transaction transaction)
    {
        foreach (Account account in transaction.GetAllAffectedAccountIds().Select(accountRepository.GetById))
        {
            AccountingPeriod? accountingPeriod = accountingPeriodRepository.GetById(transaction.AccountingPeriodId);
            DateOnly? postedDate = transaction.GetPostedDateForAccount(account.Id);
            if (postedDate == null)
            {
                continue;
            }
            while (accountingPeriod != null)
            {
                AccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(accountingPeriod.Id);

                AccountingPeriodAccountBalanceHistory accountBalanceHistory = balanceHistory.AccountBalances.Single(a => a.Account.Id == account.Id);
                if (transaction.AccountingPeriodId != accountingPeriod.Id)
                {
                    AccountBalance openingBalance = accountBalanceHistory.GetOpeningAccountBalance();
                    accountBalanceHistory.OpeningBalance = transaction.ApplyToAccountBalance(openingBalance, reverse: true).PostedBalance;
                }
                AccountBalance closingBalance = accountBalanceHistory.GetClosingAccountBalance();
                accountBalanceHistory.ClosingBalance = transaction.ApplyToAccountBalance(closingBalance, reverse: true).PostedBalance;

                List<AccountingPeriodFundBalanceHistory> fundBalanceHistories = GetAffectedFundBalanceHistories(balanceHistory, transaction, account.Id);
                foreach (AccountingPeriodFundBalanceHistory fundBalanceHistory in fundBalanceHistories)
                {
                    FundBalance openingFundBalance = fundBalanceHistory.GetOpeningFundBalance();
                    if (transaction.AccountingPeriodId != accountingPeriod.Id)
                    {
                        openingFundBalance = new FundBalance(
                            openingFundBalance.Fund,
                            transaction.ApplyToFundBalance(openingFundBalance, accountId: account.Id, reverse: true).PostedBalance,
                            0,
                            0);
                    }
                    FundBalance closingFundBalance = fundBalanceHistory.GetClosingFundBalance();
                    closingFundBalance = transaction.AccountingPeriodId != accountingPeriod.Id
                        ? new FundBalance(
                            closingFundBalance.Fund,
                            transaction.ApplyToFundBalance(closingFundBalance, accountId: account.Id, reverse: true).PostedBalance,
                            0,
                            0)
                        : transaction.ApplyToFundBalance(closingFundBalance, accountId: account.Id, reverse: true, postingOnly: true);
                    fundBalanceHistory.Update(openingFundBalance, closingFundBalance);
                }
                balanceHistory.UpdateBalances();
                if (transaction.AccountingPeriodId == accountingPeriod.Id)
                {
                    UpdateFundPlanTotals(balanceHistory, transaction, account.Id, true, true);
                }
                accountingPeriod = accountingPeriodRepository.GetNextAccountingPeriod(accountingPeriod.Id);
            }
        }
    }

    /// <summary>
    /// Updates the Accounting Period Balances for a deleted Transaction
    /// </summary>
    internal void DeleteTransaction(Transaction transaction)
    {
        AccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(transaction.AccountingPeriodId);
        foreach (AccountingPeriodFundBalanceHistory fundBalanceHistory in GetAffectedFundBalanceHistories(balanceHistory, transaction, null))
        {
            FundBalance openingBalance = fundBalanceHistory.GetOpeningFundBalance();
            FundBalance closingBalance = transaction.ApplyToFundBalance(fundBalanceHistory.GetClosingFundBalance(), reverse: true);
            fundBalanceHistory.Update(openingBalance, closingBalance);
        }
        UpdateFundPlanTotals(balanceHistory, transaction, null, true, false);
    }

    /// <summary>
    /// Gets the Fund Balance Histories within the provided Accounting Period Balance History affected by the provided Transaction and Account.
    /// </summary>
    private static List<AccountingPeriodFundBalanceHistory> GetAffectedFundBalanceHistories(
        AccountingPeriodBalanceHistory balanceHistory,
        Transaction transaction,
        AccountId? accountId)
    {
        var affectedFundIds = transaction.GetAllAffectedFundIds(accountId).ToHashSet();
        return balanceHistory.FundBalances
            .Where(fundBalanceHistory => affectedFundIds.Contains(fundBalanceHistory.Fund.Id))
            .ToList();
    }

    /// <summary>
    /// Updates Fund Plan totals affected by the provided Transaction and Account.
    /// </summary>
    private static void UpdateFundPlanTotals(
        AccountingPeriodBalanceHistory balanceHistory,
        Transaction transaction,
        AccountId? accountId,
        bool reverse,
        bool postingOnly)
    {
        var affectedFundIds = transaction.GetAllAffectedFundIds(accountId).ToHashSet();
        foreach (AccountingPeriodFundPlanTotals totals in balanceHistory.FundPlanTotals
            .Where(totals => affectedFundIds.Contains(totals.Fund.Id)))
        {
            totals.Update(transaction.ApplyToFundPlanTotals(
                totals.GetTotals(),
                accountId: accountId,
                reverse: reverse,
                postingOnly: postingOnly));
        }
    }
}