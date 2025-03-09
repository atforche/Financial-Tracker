using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.ValueObjects;

namespace Domain.Services.Implementations;

/// <inheritdoc/>
public class AccountService(IAccountRepository accountRepository, IAccountingPeriodRepository accountingPeriodRepository) : IAccountService
{
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly IAccountingPeriodRepository _accountingPeriodRepository = accountingPeriodRepository;

    /// <inheritdoc/>
    public Account CreateNewAccount(string name, AccountType type, IEnumerable<FundAmount> startingFundBalances)
    {
        ValidateNewAccountName(name);
        AccountingPeriod accountingPeriod = _accountingPeriodRepository.FindOpenPeriods().FirstOrDefault() ??
            throw new InvalidOperationException();
        var newAccount = new Account(name, type);
        accountingPeriod.AddAccountBalanceCheckpoint(newAccount, startingFundBalances);
        return newAccount;
    }

    /// <summary>
    /// Validates that the new name for an Account is unique among the existing Accounts
    /// </summary>
    /// <param name="newName">New name to be given to an Account</param>
    private void ValidateNewAccountName(string newName)
    {
        if (_accountRepository.FindByNameOrNull(newName) != null)
        {
            throw new InvalidOperationException();
        }
    }
}