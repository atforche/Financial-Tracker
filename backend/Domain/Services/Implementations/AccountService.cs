using Domain.Entities;
using Domain.Repositories;

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
    public void CreateNewAccount(CreateAccountRequest createAccountRequest,
        decimal startingBalance,
        out Account newAccount,
        out AccountStartingBalance newAccountStartingBalance)
    {
        ValidateNewAccountName(createAccountRequest.Name);
        newAccount = new Account(createAccountRequest);

        var createAccountStartingBalanceRequest = new CreateAccountStartingBalanceRequest
        {
            Account = newAccount,
            AccountingPeriod = _accountingPeriodRepository.FindOpenPeriods().Last(),
            StartingBalance = startingBalance,
        };
        newAccountStartingBalance = new AccountStartingBalance(createAccountStartingBalanceRequest);
    }

    /// <summary>
    /// Validates that the new name for an Account is unique among the existing accounts
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