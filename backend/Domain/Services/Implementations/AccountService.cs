using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.ValueObjects;

namespace Domain.Services.Implementations;

/// <inheritdoc/>
public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountingPeriodRepository _accountingPeriodRepository;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountRepository">Repository of Accounts</param>
    /// <param name="accountingPeriodRepository">Repository of Accounting Periods</param>
    public AccountService(IAccountRepository accountRepository, IAccountingPeriodRepository accountingPeriodRepository)
    {
        _accountRepository = accountRepository;
        _accountingPeriodRepository = accountingPeriodRepository;
    }

    /// <inheritdoc/>
    public Account CreateNewAccount(AccountingPeriod initialAccountingPeriod,
        string name,
        AccountType type,
        IEnumerable<FundAmount> startingFundBalances)
    {
        ValidateNewAccountName(name);
        ValidateInitialAccountingPeriodForAccount(initialAccountingPeriod);
        Account newAccount = new Account(name, type);
        initialAccountingPeriod.AddAccountBalanceCheckpoint(newAccount,
            AccountBalanceCheckpointType.StartOfPeriod,
            startingFundBalances);
        initialAccountingPeriod.AddAccountBalanceCheckpoint(newAccount,
            AccountBalanceCheckpointType.StartOfMonth,
            startingFundBalances);
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

    /// <summary>
    /// Validates that the provided Accounting Period is valid as an initial Accounting Period for an Account 
    /// </summary>
    /// <param name="initialAccountingPeriod">Initial Accounting Period for an Account</param>
    private void ValidateInitialAccountingPeriodForAccount(AccountingPeriod initialAccountingPeriod)
    {
        if (!initialAccountingPeriod.IsOpen)
        {
            throw new InvalidOperationException();
        }
        if (_accountingPeriodRepository.FindOpenPeriods().First() != initialAccountingPeriod)
        {
            throw new InvalidOperationException();
        }
    }
}