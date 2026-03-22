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
                currentBalance.FundBalances.Select(fundAmount => new FundAmount
                {
                    FundId = fundAmount.FundId,
                    Amount = fundAmount.Amount
                }),
                currentBalance.FundBalances.Select(fundAmount => new FundAmount
                {
                    FundId = fundAmount.FundId,
                    Amount = fundAmount.Amount
                })));
        }

        IEnumerable<FundAccountingPeriodBalanceHistory> fundBalanceHistories = [];
        foreach (Fund fund in fundRepository.GetAll())
        {
            FundBalance currentBalance = fundBalanceService.GetCurrentBalance(fund.Id);
            fundBalanceHistories = fundBalanceHistories.Append(new FundAccountingPeriodBalanceHistory(
                fund,
                newAccountingPeriod,
                currentBalance.AccountBalances.Select(accountAmount => new AccountAmount
                {
                    AccountId = accountAmount.AccountId,
                    Amount = accountAmount.Amount
                }),
                currentBalance.AccountBalances.Select(accountAmount => new AccountAmount
                {
                    AccountId = accountAmount.AccountId,
                    Amount = accountAmount.Amount
                })));
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
                new FundAccountingPeriodBalanceHistory(newFund, accountingPeriod, [], [])).ToList();
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
                new AccountAccountingPeriodBalanceHistory(newAccount, accountingPeriod, [], [])).ToList();
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
    /// Updates the Accounting Period Balances for a newly postedTransaction
    /// </summary>
    internal void PostTransaction(TransactionAccount transactionAccount)
    {
        if (transactionAccount.PostedDate == null)
        {
            return;
        }
        AccountingPeriod? accountingPeriod = accountingPeriodRepository.GetById(transactionAccount.Transaction.AccountingPeriodId);
        while (accountingPeriod != null)
        {
            AccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(accountingPeriod.Id);
            AccountAccountingPeriodBalanceHistory accountBalanceHistory =
                balanceHistory.AccountBalances.Single(a => a.Account.Id == transactionAccount.AccountId);
            var fundBalanceHistories = balanceHistory.FundBalances
                .Where(f => transactionAccount.FundAmounts.Any(fundAmount => fundAmount.FundId == f.Fund.Id)).ToList();

            if (transactionAccount.Transaction.AccountingPeriodId != accountingPeriod.Id)
            {
                AccountBalance openingBalance = accountBalanceHistory.GetOpeningAccountBalance();
                accountBalanceHistory.OpeningFundBalances = transactionAccount.ApplyToAccountBalance(openingBalance, transactionAccount.PostedDate.Value).FundBalances;
                foreach (FundAccountingPeriodBalanceHistory fundBalanceHistory in fundBalanceHistories)
                {
                    FundBalance openingFundBalance = fundBalanceHistory.GetOpeningFundBalance();
                    fundBalanceHistory.OpeningAccountBalances = transactionAccount.ApplyToFundBalance(openingFundBalance, transactionAccount.PostedDate.Value).AccountBalances;
                }
            }
            AccountBalance closingBalance = accountBalanceHistory.GetClosingAccountBalance();
            accountBalanceHistory.ClosingFundBalances = transactionAccount.ApplyToAccountBalance(closingBalance, transactionAccount.PostedDate.Value).FundBalances;
            foreach (FundAccountingPeriodBalanceHistory fundBalanceHistory in fundBalanceHistories)
            {
                FundBalance closingFundBalance = fundBalanceHistory.GetClosingFundBalance();
                fundBalanceHistory.ClosingAccountBalances = transactionAccount.ApplyToFundBalance(closingFundBalance, transactionAccount.PostedDate.Value).AccountBalances;
            }
            accountingPeriod = accountingPeriodRepository.GetNextAccountingPeriod(accountingPeriod.Id);
        }
    }
}