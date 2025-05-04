using Data;
using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Rest.Models.Account;

namespace Rest.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Accounts
/// </summary>
[ApiController]
[Route("/accounts")]
internal sealed class AccountController(
    IUnitOfWork unitOfWork,
    AddAccountAction addAccountAction,
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    IFundRepository fundRepository) : ControllerBase
{
    /// <summary>
    /// Retrieves all the Accounts from the database
    /// </summary>
    /// <returns>A collection of all Accounts</returns>
    [HttpGet("")]
    public IReadOnlyCollection<AccountModel> GetAllAccounts() =>
        accountRepository.FindAll().Select(account => new AccountModel(account)).ToList();

    /// <summary>
    /// Retrieves the Account that matches the provided ID
    /// </summary>
    /// <param name="accountId">ID of the Account to retrieve</param>
    /// <returns>The Account that matches the provided ID</returns>
    [HttpGet("{accountId}")]
    public IActionResult GetAccount(Guid accountId)
    {
        Account? account = accountRepository.FindByExternalIdOrNull(accountId);
        return account != null ? Ok(new AccountModel(account)) : NotFound();
    }

    /// <summary>
    /// Creates a new Account with the provided properties
    /// </summary>
    /// <param name="createAccountModel">Request to create an Account</param>
    /// <returns>The created Account</returns>
    [HttpPost("")]
    public async Task<IActionResult> CreateAccountAsync(CreateAccountModel createAccountModel)
    {
        var funds = fundRepository.FindAll().ToDictionary(fund => fund.Id.ExternalId, fund => fund);
        AccountingPeriod? accountingPeriod = accountingPeriodRepository.FindByExternalIdOrNull(createAccountModel.AccountingPeriodId);
        if (accountingPeriod == null)
        {
            return NotFound();
        }
        Account newAccount = addAccountAction.Run(createAccountModel.Name, createAccountModel.Type, accountingPeriod, createAccountModel.Date,
            createAccountModel.StartingFundBalances.Select(fundBalance => new FundAmount
            {
                Fund = funds[fundBalance.FundId],
                Amount = fundBalance.Amount,
            }));
        accountRepository.Add(newAccount);
        await unitOfWork.SaveChangesAsync();
        return Ok(new AccountModel(newAccount));
    }
}