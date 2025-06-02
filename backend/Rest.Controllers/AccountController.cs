using Data;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Microsoft.AspNetCore.Mvc;
using Rest.Models.Accounts;

namespace Rest.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Accounts
/// </summary>
[ApiController]
[Route("/accounts")]
public sealed class AccountController(
    UnitOfWork unitOfWork,
    IAccountRepository accountRepository,
    AccountFactory accountFactory,
    AccountIdFactory accountIdFactory,
    AccountingPeriodIdFactory accountingPeriodIdFactory,
    FundIdFactory fundIdFactory) : ControllerBase
{
    /// <summary>
    /// Retrieves all the Accounts from the database
    /// </summary>
    /// <returns>A collection of all Accounts</returns>
    [HttpGet("")]
    public IReadOnlyCollection<AccountModel> GetAll() =>
        accountRepository.FindAll().Select(account => new AccountModel(account)).ToList();

    /// <summary>
    /// Retrieves the Account that matches the provided ID
    /// </summary>
    /// <param name="accountId">ID of the Account to retrieve</param>
    /// <returns>The Account that matches the provided ID</returns>
    [HttpGet("{accountId}")]
    public IActionResult Get(Guid accountId)
    {
        AccountId id = accountIdFactory.Create(accountId);
        return Ok(new AccountModel(accountRepository.FindById(id)));
    }

    /// <summary>
    /// Creates a new Account with the provided properties
    /// </summary>
    /// <param name="createAccountModel">Request to create an Account</param>
    /// <returns>The created Account</returns>
    [HttpPost("")]
    public async Task<IActionResult> CreateAsync(CreateAccountModel createAccountModel)
    {
        AccountingPeriodId accountingPeriodId = accountingPeriodIdFactory.Create(createAccountModel.AccountingPeriodId);
        Account newAccount = accountFactory.Create(createAccountModel.Name, createAccountModel.Type, accountingPeriodId, createAccountModel.Date,
            createAccountModel.StartingFundBalances.Select(fundBalance => new FundAmount
            {
                FundId = fundIdFactory.Create(fundBalance.FundId),
                Amount = fundBalance.Amount,
            }));
        accountRepository.Add(newAccount);
        await unitOfWork.SaveChangesAsync();
        return Ok(new AccountModel(newAccount));
    }
}