using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;

namespace Domain.AccountingPeriods;

/// <summary>
/// Service for managing Accounting Period Balances
/// </summary>
public class AccountingPeriodBalanceService(
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository,
    IAccountRepository accountRepository,
    IFundRepository fundRepository,
    AccountBalanceService accountBalanceService,
    FundBalanceService fundBalanceService)
{
    /// <summary>
    /// Updates the Accounting Period Balances for a newly added Accounting Period
    /// </summary>
    internal void AddAccountingPeriod(AccountingPeriod newAccountingPeriod)
    {
        IEnumerable<AccountAccountingPeriodBalanceHistory> accountBalanceHistories = [];
        foreach (Account account in accountRepository.GetAll())
        {
            AccountBalance currentBalance = accountBalanceService.GetCurrentBalance(account);
            accountBalanceHistories = accountBalanceHistories.Append(new AccountAccountingPeriodBalanceHistory(
                account,
                newAccountingPeriod,
                currentBalance.PostedBalance,
                currentBalance.PostedBalance));
        }

        IEnumerable<FundAccountingPeriodBalanceHistory> fundBalanceHistories = [];
        foreach (Fund fund in fundRepository.GetAll())
        {
            FundBalance currentBalance = fundBalanceService.GetCurrentBalance(fund.Id);
            fundBalanceHistories = fundBalanceHistories.Append(new FundAccountingPeriodBalanceHistory(
                fund,
                newAccountingPeriod,
                currentBalance.PostedBalance,
                0,
                0,
                currentBalance.PostedBalance));
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
            balanceHistory.FundBalances = balanceHistory.FundBalances.Append(
                new FundAccountingPeriodBalanceHistory(newFund, accountingPeriod, 0, 0, 0, 0)).ToList();
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
                new AccountAccountingPeriodBalanceHistory(newAccount, accountingPeriod, 0, 0)).ToList();
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
            AccountAccountingPeriodBalanceHistory accountBalanceHistory = balanceHistory.AccountBalances.Single(a => a.Account.Id == accountId);
            List<FundAccountingPeriodBalanceHistory> fundBalanceHistories = GetAffectedFundBalanceHistories(balanceHistory, transaction, accountId);
            if (transaction.AccountingPeriodId != accountingPeriod.Id)
            {
                AccountBalance openingBalance = accountBalanceHistory.GetOpeningAccountBalance();
                accountBalanceHistory.OpeningBalance = transaction.ApplyToAccountBalance(openingBalance).PostedBalance;
                foreach (FundAccountingPeriodBalanceHistory fundBalanceHistory in fundBalanceHistories)
                {
                    FundBalance openingFundBalance = fundBalanceHistory.GetOpeningFundBalance();
                    fundBalanceHistory.OpeningBalance = transaction.ApplyToFundBalance(openingFundBalance, accountId: accountId).PostedBalance;
                }
            }
            else
            {
                UpdateFundPeriodAmounts(fundBalanceHistories, transaction, accountId, false);
            }
            AccountBalance closingBalance = accountBalanceHistory.GetClosingAccountBalance();
            accountBalanceHistory.ClosingBalance = transaction.ApplyToAccountBalance(closingBalance).PostedBalance;
            foreach (FundAccountingPeriodBalanceHistory fundBalanceHistory in fundBalanceHistories)
            {
                FundBalance closingFundBalance = fundBalanceHistory.GetClosingFundBalance();
                fundBalanceHistory.ClosingBalance = transaction.ApplyToFundBalance(closingFundBalance, accountId: accountId).PostedBalance;
            }
            balanceHistory.FundBalances = balanceHistory.FundBalances.ToList();
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
                continue;
            }
            while (accountingPeriod != null)
            {
                AccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(accountingPeriod.Id);
                AccountAccountingPeriodBalanceHistory accountBalanceHistory =
                    balanceHistory.AccountBalances.Single(a => a.Account.Id == account.Id);
                List<FundAccountingPeriodBalanceHistory> fundBalanceHistories = GetAffectedFundBalanceHistories(balanceHistory, transaction, account.Id);
                if (transaction.AccountingPeriodId != accountingPeriod.Id)
                {
                    AccountBalance openingBalance = accountBalanceHistory.GetOpeningAccountBalance();
                    accountBalanceHistory.OpeningBalance = transaction.ApplyToAccountBalance(openingBalance, reverse: true).PostedBalance;
                    foreach (FundAccountingPeriodBalanceHistory fundBalanceHistory in fundBalanceHistories)
                    {
                        FundBalance openingFundBalance = fundBalanceHistory.GetOpeningFundBalance();
                        fundBalanceHistory.OpeningBalance = transaction.ApplyToFundBalance(openingFundBalance, accountId: account.Id, reverse: true).PostedBalance;
                    }
                }
                else
                {
                    UpdateFundPeriodAmounts(fundBalanceHistories, transaction, account.Id, true);
                }
                AccountBalance closingBalance = accountBalanceHistory.GetClosingAccountBalance();
                accountBalanceHistory.ClosingBalance = transaction.ApplyToAccountBalance(closingBalance, reverse: true).PostedBalance;
                foreach (FundAccountingPeriodBalanceHistory fundBalanceHistory in fundBalanceHistories)
                {
                    FundBalance closingFundBalance = fundBalanceHistory.GetClosingFundBalance();
                    fundBalanceHistory.ClosingBalance = transaction.ApplyToFundBalance(closingFundBalance, accountId: account.Id, reverse: true).PostedBalance;
                }
                balanceHistory.FundBalances = balanceHistory.FundBalances.ToList();
                accountingPeriod = accountingPeriodRepository.GetNextAccountingPeriod(accountingPeriod.Id);
            }
        }
    }

    /// <summary>
    /// Gets the Fund Balance Histories within the provided Accounting Period Balance History affected by the provided Transaction and Account.
    /// </summary>
    private static List<FundAccountingPeriodBalanceHistory> GetAffectedFundBalanceHistories(
        AccountingPeriodBalanceHistory balanceHistory,
        Transaction transaction,
        AccountId accountId)
    {
        var affectedFundIds = transaction.GetAllAffectedFundIds(accountId).ToHashSet();
        return balanceHistory.FundBalances
            .Where(fundBalanceHistory => affectedFundIds.Contains(fundBalanceHistory.Fund.Id))
            .ToList();
    }

    /// <summary>
    /// Updates the assigned and spent totals for the provided Fund Balance Histories based on the provided Transaction.
    /// </summary>
    private static void UpdateFundPeriodAmounts(
        IEnumerable<FundAccountingPeriodBalanceHistory> fundBalanceHistories,
        Transaction transaction,
        AccountId accountId,
        bool reverse)
    {
        decimal direction = reverse ? -1 : 1;
        foreach (FundAccountingPeriodBalanceHistory fundBalanceHistory in fundBalanceHistories)
        {
            if (transaction is IncomeTransaction income && accountId == income.CreditAccountId)
            {
                fundBalanceHistory.AmountAssigned += direction * income.FundAssignments
                    .Where(fundAmount => fundAmount.FundId == fundBalanceHistory.Fund.Id)
                    .Sum(fundAmount => fundAmount.Amount);
            }
            else if (transaction is SpendingTransaction spending && accountId == spending.DebitAccountId)
            {
                fundBalanceHistory.AmountSpent += direction * spending.FundAssignments
                    .Where(fundAmount => fundAmount.FundId == fundBalanceHistory.Fund.Id)
                    .Sum(fundAmount => fundAmount.Amount);
            }
        }
    }
}