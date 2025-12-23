using System.Diagnostics.CodeAnalysis;
using Data;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Microsoft.AspNetCore.Mvc;
using Models.Accounts;
using Models.Errors;
using Models.Funds;
using Rest.Mappers;

namespace Rest.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Accounts
/// </summary>
[ApiController]
[Route("/accounts")]
public sealed class AccountController(
    UnitOfWork unitOfWork,
    IAccountRepository accountRepository,
    IAccountingPeriodRepository accountingPeriodRepository,
    AccountService accountService,
    FundAmountMapper fundAmountMapper) : ControllerBase
{
    /// <summary>
    /// Retrieves all the Accounts from the database
    /// </summary>
    /// <returns>The collection of all Accounts</returns>
    [HttpGet("")]
    public IReadOnlyCollection<AccountModel> GetAll() => accountRepository.FindAll().Select(AccountMapper.ToModel).ToList();

    /// <summary>
    /// Retrieves the Account that matches the provided ID
    /// </summary>
    /// <param name="accountId">ID of the Account to retrieve</param>
    /// <returns>The Account that matches the provided ID</returns>
    [HttpGet("{accountId}")]
    public IActionResult Get(Guid accountId)
    {
        if (!TryFindById(accountId, out Account? account, out IActionResult? errorResult))
        {
            return errorResult;
        }
        return Ok(AccountMapper.ToModel(account));
    }

    /// <summary>
    /// Creates a new Account with the provided properties
    /// </summary>
    /// <param name="createAccountModel">Request to create an Account</param>
    /// <returns>The created Account</returns>
    [HttpPost("")]
    [ProducesResponseType(typeof(AccountModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync(CreateAccountModel createAccountModel)
    {
        if (!accountingPeriodRepository.TryFindById(createAccountModel.AccountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            return new UnprocessableEntityObjectResult(ErrorMapper.ToModel($"Accounting Period with ID {createAccountModel.AccountingPeriodId} was not found.", []));
        }
        List<FundAmount> fundAmounts = [];
        foreach (FundAmountModel fundAmountModel in createAccountModel.InitialFundAmounts)
        {
            if (!fundAmountMapper.TryMapToDomain(fundAmountModel, out FundAmount? fundAmount, out Exception? exception))
            {
                return new UnprocessableEntityObjectResult(ErrorMapper.ToModel("Fund with ID " + fundAmountModel.FundId + " could not be found.", [exception]));
            }
            fundAmounts.Add(fundAmount);
        }
        if (!accountService.TryCreate(
            new CreateAccountRequest
            {
                Name = createAccountModel.Name,
                Type = AccountTypeMapper.ToDomain(createAccountModel.Type),
                AccountingPeriodId = accountingPeriod.Id,
                AddDate = createAccountModel.AddDate,
                InitialFundAmounts = fundAmounts
            },
            out Account? newAccount,
            out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(ErrorMapper.ToModel("Failed to create Account.", exceptions));
        }
        accountRepository.Add(newAccount);
        await unitOfWork.SaveChangesAsync();
        return Ok(AccountMapper.ToModel(newAccount));
    }

    /// <summary>
    /// Updates the provided Account with the provided properties
    /// </summary>
    /// <param name="accountId">ID of the Account to update</param>
    /// <param name="updateAccountModel">Request to update an Account</param>
    /// <returns>The updated Account</returns>
    [HttpPost("{accountId}")]
    [ProducesResponseType(typeof(AccountModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateAsync(Guid accountId, UpdateAccountModel updateAccountModel)
    {
        if (!TryFindById(accountId, out Account? accountToUpdate, out IActionResult? errorResult))
        {
            return errorResult;
        }
        if (!accountService.TryUpdate(accountToUpdate, updateAccountModel.Name, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(ErrorMapper.ToModel("Failed to update Account.", exceptions));
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(AccountMapper.ToModel(accountToUpdate));
    }

    /// <summary>
    /// Deletes the Account with the provided ID
    /// </summary>
    /// <param name="accountId">ID of the Account to delete</param>
    [HttpDelete("{accountId}")]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteAsync(Guid accountId)
    {
        if (!TryFindById(accountId, out Account? accountToDelete, out IActionResult? errorResult))
        {
            return errorResult;
        }
        if (!accountService.TryDelete(accountToDelete, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(ErrorMapper.ToModel("Failed to delete Account.", exceptions));
        }
        await unitOfWork.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Attempts to find the Account with the provided ID
    /// </summary>
    /// <param name="accountId">ID of the Account to find</param>
    /// <param name="account">Account that was found</param>
    /// <param name="errorResult">Result to return if the account was not found</param>
    /// <returns>True if the account was found, false otherwise</returns>
    private bool TryFindById(Guid accountId, [NotNullWhen(true)] out Account? account, [NotNullWhen(false)] out IActionResult? errorResult)
    {
        errorResult = null;
        if (!accountRepository.TryFindById(accountId, out account))
        {
            errorResult = new NotFoundObjectResult(ErrorMapper.ToModel($"Account with ID {accountId} was not found.", []));
        }
        return errorResult == null;
    }
}