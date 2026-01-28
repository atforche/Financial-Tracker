using Data;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
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
    ITransactionRepository transactionRepository,
    AccountService accountService,
    AccountingPeriodMapper accountingPeriodMapper,
    AccountMapper accountMapper,
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
        if (!accountMapper.TryToDomain(accountId, out Account? account, out IActionResult? errorResult))
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
        if (!accountingPeriodMapper.TryToDomain(createAccountModel.AccountingPeriodId, out AccountingPeriod? accountingPeriod, out IActionResult? errorResult))
        {
            return errorResult;
        }
        List<FundAmount> fundAmounts = [];
        foreach (CreateFundAmountModel fundAmountModel in createAccountModel.InitialFundAmounts)
        {
            if (!fundAmountMapper.TryToDomain(fundAmountModel, out FundAmount? fundAmount, out errorResult))
            {
                return errorResult;
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
            out Transaction? initialTransaction,
            out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(ErrorMapper.ToModel("Failed to create Account.", exceptions));
        }
        accountRepository.Add(newAccount);
        transactionRepository.Add(initialTransaction);
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
        if (!accountMapper.TryToDomain(accountId, out Account? accountToUpdate, out IActionResult? errorResult))
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
        if (!accountMapper.TryToDomain(accountId, out Account? accountToDelete, out IActionResult? errorResult))
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
}