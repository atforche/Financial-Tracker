using Domain.Accounts;
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
    IFundGoalRepository fundGoalRepository,
    ITransactionRepository transactionRepository,
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
        foreach (Fund fund in fundRepository.GetAll())
        {
            FundBalance currentBalance = fundBalanceService.GetCurrentBalance(fund.Id);
            currentBalance = new FundBalance(fund.Id, currentBalance.PostedBalance, 0, 0, 0, 0);
            fundBalanceHistories = fundBalanceHistories.Append(new AccountingPeriodFundBalanceHistory(
                fund,
                newAccountingPeriod,
                currentBalance,
                currentBalance));
        }
        accountingPeriodBalanceHistoryRepository.Add(new AccountingPeriodBalanceHistory(
            newAccountingPeriod,
            accountBalanceHistories,
            fundBalanceHistories));
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
        AccountingPeriod? accountingPeriod = accountingPeriodRepository.GetById(newFund.AddAccountingPeriodId);
        while (accountingPeriod != null)
        {
            AccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(accountingPeriod.Id);
            var balance = new FundBalance(newFund.Id, 0, 0, 0, 0, 0);
            balanceHistory.FundBalances = balanceHistory.FundBalances.Append(
                new AccountingPeriodFundBalanceHistory(newFund, accountingPeriod, balance, balance)).ToList();
            accountingPeriod = accountingPeriodRepository.GetNextAccountingPeriod(accountingPeriod.Id);
        }
    }

    /// <summary>
    /// Updates the Accounting Period Balances for a deleted Fund
    /// </summary>
    internal void DeleteFund(Fund fund)
    {
        AccountingPeriod? accountingPeriod = accountingPeriodRepository.GetById(fund.AddAccountingPeriodId);
        while (accountingPeriod != null)
        {
            AccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(accountingPeriod.Id);
            balanceHistory.FundBalances = balanceHistory.FundBalances.Where(f => f.Fund.Id != fund.Id).ToList();
            accountingPeriod = accountingPeriodRepository.GetNextAccountingPeriod(accountingPeriod.Id);
        }
    }

    /// <summary>
    /// Updates the Accounting Period Balances for a newly added Account
    /// </summary>
    internal void AddAccount(Account newAccount)
    {
        AccountingPeriod? accountingPeriod = accountingPeriodRepository.GetById(newAccount.AddAccountingPeriodId);
        while (accountingPeriod != null)
        {
            AccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(accountingPeriod.Id);
            balanceHistory.AccountBalances = balanceHistory.AccountBalances.Append(
                new AccountingPeriodAccountBalanceHistory(newAccount, accountingPeriod, 0, 0)).ToList();
            accountingPeriod = accountingPeriodRepository.GetNextAccountingPeriod(accountingPeriod.Id);
        }
    }

    /// <summary>
    /// Updates the Accounting Period Balances for a deleted Account
    /// </summary>
    internal void DeleteAccount(Account account)
    {
        AccountingPeriod? accountingPeriod = accountingPeriodRepository.GetById(account.AddAccountingPeriodId);
        while (accountingPeriod != null)
        {
            AccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(accountingPeriod.Id);
            balanceHistory.AccountBalances = balanceHistory.AccountBalances.Where(a => a.Account.Id != account.Id).ToList();
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
            fundBalanceHistory.Update(openingBalance, closingBalance, fundGoalRepository.GetByFundAndAccountingPeriod(fundBalanceHistory.Fund.Id, fundBalanceHistory.AccountingPeriod.Id));
        }
    }

    /// <summary>
    /// Updates the Accounting Period Balances for an updated Transaction
    /// </summary>
    internal void UpdateTransaction(Transaction updatedTransaction)
    {
        AccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(updatedTransaction.AccountingPeriodId);
        foreach (AccountingPeriodFundBalanceHistory fundBalanceHistory in GetAffectedFundBalanceHistories(balanceHistory, updatedTransaction, null))
        {
            // When we update a transaction, the existing affects of old version of the transaction have already
            // been incorporated into the current balances. So the easiest way to update the balances is to
            // simply recalculate the entire accounting period.
            FundBalance openingBalance = fundBalanceHistory.GetOpeningFundBalance();
            FundBalance closingBalance = openingBalance;
            foreach (Transaction transaction in transactionRepository.GetAllByAccountingPeriod(fundBalanceHistory.AccountingPeriod.Id)
                .Where(transaction => transaction.GetAllAffectedFundIds(null).Contains(fundBalanceHistory.Fund.Id)))
            {
                closingBalance = transaction.ApplyToFundBalance(closingBalance);
            }
            fundBalanceHistory.Update(
                openingBalance,
                closingBalance,
                fundGoalRepository.GetByFundAndAccountingPeriod(fundBalanceHistory.Fund.Id, fundBalanceHistory.AccountingPeriod.Id));
        }
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
                        openingFundBalance.FundId,
                        transaction.ApplyToFundBalance(openingFundBalance, accountId: accountId).PostedBalance,
                        openingFundBalance.AmountAssigned,
                        openingFundBalance.PendingAmountAssigned,
                        openingFundBalance.AmountSpent,
                        openingFundBalance.PendingAmountSpent);
                }
                FundBalance closingFundBalance = fundBalanceHistory.GetClosingFundBalance();
                closingFundBalance = transaction.AccountingPeriodId != accountingPeriod.Id
                    ? new FundBalance(
                        closingFundBalance.FundId,
                        transaction.ApplyToFundBalance(closingFundBalance, accountId: accountId).PostedBalance,
                        closingFundBalance.AmountAssigned,
                        closingFundBalance.PendingAmountAssigned,
                        closingFundBalance.AmountSpent,
                        closingFundBalance.PendingAmountSpent)
                    : transaction.ApplyToFundBalance(closingFundBalance, accountId: accountId);
                fundBalanceHistory.Update(
                    openingFundBalance,
                    closingFundBalance,
                    fundGoalRepository.GetByFundAndAccountingPeriod(fundBalanceHistory.Fund.Id, fundBalanceHistory.AccountingPeriod.Id));
            }
            accountingPeriod = accountingPeriodRepository.GetNextAccountingPeriod(accountingPeriod.Id);
        }
    }

    /// <summary>
    /// Updates the Accounting Period Balances for a newly unposted Transaction
    /// </summary>
    internal void UnpostTransaction(Transaction transaction)
    {
        AccountingPeriod? accountingPeriod = accountingPeriodRepository.GetById(transaction.AccountingPeriodId);
        foreach (Account account in transaction.GetAllAffectedAccountIds().Select(accountRepository.GetById))
        {
            DateOnly? postedDate = transaction.GetPostedDateForAccount(account.Id);
            if (postedDate == null)
            {
                return;
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
                            openingFundBalance.FundId,
                            transaction.ApplyToFundBalance(openingFundBalance, accountId: account.Id, reverse: true).PostedBalance,
                            openingFundBalance.AmountAssigned,
                            openingFundBalance.PendingAmountAssigned,
                            openingFundBalance.AmountSpent,
                            openingFundBalance.PendingAmountSpent);
                    }
                    FundBalance closingFundBalance = fundBalanceHistory.GetClosingFundBalance();
                    closingFundBalance = transaction.AccountingPeriodId != accountingPeriod.Id
                        ? new FundBalance(
                            closingFundBalance.FundId,
                            transaction.ApplyToFundBalance(closingFundBalance, accountId: account.Id, reverse: true).PostedBalance,
                            closingFundBalance.AmountAssigned,
                            closingFundBalance.PendingAmountAssigned,
                            closingFundBalance.AmountSpent,
                            closingFundBalance.PendingAmountSpent)
                        : transaction.ApplyToFundBalance(closingFundBalance, accountId: account.Id, reverse: true);
                    fundBalanceHistory.Update(
                        openingFundBalance,
                        closingFundBalance,
                        fundGoalRepository.GetByFundAndAccountingPeriod(fundBalanceHistory.Fund.Id, fundBalanceHistory.AccountingPeriod.Id));
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
            fundBalanceHistory.Update(openingBalance, closingBalance, fundGoalRepository.GetByFundAndAccountingPeriod(fundBalanceHistory.Fund.Id, fundBalanceHistory.AccountingPeriod.Id));
        }
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
}