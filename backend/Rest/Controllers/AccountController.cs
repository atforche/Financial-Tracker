using Data;
using Data.Accounts;
using Data.Transactions;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Microsoft.AspNetCore.Mvc;
using Models.Accounts;
using Models.Errors;
using Models.Funds;
using Models.Transactions;
using Rest.Mappers;

namespace Rest.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Accounts
/// </summary>
[ApiController]
[Route("/accounts")]
public sealed class AccountController(
    UnitOfWork unitOfWork,
    AccountRepository accountRepository,
    TransactionRepository transactionRepository,
    AccountService accountService,
    AccountingPeriodMapper accountingPeriodMapper,
    AccountMapper accountMapper,
    FundAmountMapper fundAmountMapper,
    TransactionMapper transactionMapper) : ControllerBase
{
    /// <summary>
    /// Retrieves all the Accounts from the database
    /// </summary>
    /// <returns>The collection of all Accounts</returns>
    [HttpGet("")]
    [ProducesResponseType(typeof(IReadOnlyCollection<AccountModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetAll([FromQuery] AccountQueryParameterModel queryParameters)
    {
        List<AccountModel> results = [];

        AccountSortOrder? accountSortOrder = null;
        if (queryParameters.SortBy != null && !AccountSortOrderMapper.TryToData(queryParameters.SortBy.Value, out accountSortOrder, out IActionResult? errorResult))
        {
            return errorResult;
        }
        IEnumerable<AccountType> accountTypes = [];
        if (queryParameters.Types != null)
        {
            accountTypes = [];
            foreach (AccountTypeModel accountTypeModel in queryParameters.Types)
            {
                if (!AccountTypeMapper.TryToDomain(accountTypeModel, out AccountType? accountType, out errorResult))
                {
                    return errorResult;
                }
                accountTypes = accountTypes.Append(accountType.Value);
            }
        }

        foreach (Account account in accountRepository.GetMany(new GetAccountsRequest
        {
            SortBy = accountSortOrder,
            Names = queryParameters.Names,
            Types = accountTypes.Any() ? accountTypes.ToList() : null,
            Limit = queryParameters.Limit,
            Offset = queryParameters.Offset,
        }))
        {
            if (!accountMapper.TryToModel(account, out AccountModel? accountModel, out errorResult))
            {
                return errorResult;
            }
            results.Add(accountModel);
        }
        return Ok(results);
    }

    /// <summary>
    /// Retrieves the Transactions for the Account that matches the provided ID
    /// </summary>
    [HttpGet("{accountId}/transactions")]
    [ProducesResponseType(typeof(IReadOnlyCollection<TransactionModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetTransactions(Guid accountId)
    {
        if (!accountMapper.TryToDomain(accountId, out Account? account, out IActionResult? errorResult))
        {
            return errorResult;
        }
        return Ok(transactionRepository.GetManyByAccount(account.Id, new()).Select(transactionMapper.ToModel).ToList());
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
        if (!AccountTypeMapper.TryToDomain(createAccountModel.Type, out AccountType? accountType, out errorResult))
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
                Type = accountType.Value,
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
        if (!accountMapper.TryToModel(newAccount, out AccountModel? accountModel, out errorResult))
        {
            return errorResult;
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(accountModel);
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
        if (!accountMapper.TryToModel(accountToUpdate, out AccountModel? accountModel, out errorResult))
        {
            return errorResult;
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(accountModel);
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