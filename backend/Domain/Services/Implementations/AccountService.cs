using Domain.Entities;
using Domain.Repositories;
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
    public void CreateNewAccount(CreateAccountRequest createAccountRequest,
        out Account newAccount,
        out AccountStartingBalance newAccountStartingBalance)
    {
        ValidateNewAccountName(createAccountRequest.Name);
        newAccount = new Account(createAccountRequest.Name, createAccountRequest.Type);
        newAccountStartingBalance = new AccountStartingBalance(newAccount,
            _accountingPeriodRepository.FindOpenPeriods().Last(),
            createAccountRequest.StartingFundBalances.Select(request => new FundAmount(request.Fund.Id, request.Amount)).ToList());
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