using Data;
using Data.AccountingPeriods;
using Data.Transactions;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Transactions;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.AccountingPeriods;
using Models.Errors;
using Models.Transactions;
using Rest.Mappers;

namespace Rest.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Accounting Periods
/// </summary>
[ApiController]
[Route("/accounting-periods")]
public sealed class AccountingPeriodController(UnitOfWork unitOfWork,
    AccountingPeriodRepository accountingPeriodRepository,
    TransactionRepository transactionRepository,
    AccountingPeriodService accountingPeriodService,
    AccountMapper accountMapper,
    AccountingPeriodMapper accountingPeriodMapper,
    TransactionMapper transactionMapper) : ControllerBase
{
    /// <summary>
    /// Retrieves the Accounting Periods that match the specified criteria
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(typeof(CollectionModel<AccountingPeriodModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetAll([FromQuery] AccountingPeriodQueryParameterModel queryParameters)
    {
        AccountingPeriodSortOrder? accountingPeriodSortOrder = null;
        if (queryParameters.SortBy != null && !AccountingPeriodSortOrderMapper.TryToData(queryParameters.SortBy.Value, out accountingPeriodSortOrder, out IActionResult? errorResult))
        {
            return errorResult;
        }
        PaginatedCollection<AccountingPeriod> paginatedAccountingPeriods = accountingPeriodRepository.GetMany(new GetAccountingPeriodsRequest
        {
            SortBy = accountingPeriodSortOrder,
            Years = queryParameters.Years,
            Months = queryParameters.Months,
            IsOpen = queryParameters.IsOpen,
            Limit = queryParameters.Limit,
            Offset = queryParameters.Offset,
        });
        return Ok(new CollectionModel<AccountingPeriodModel>()
        {
            Items = paginatedAccountingPeriods.Items.Select(AccountingPeriodMapper.ToModel).ToList(),
            TotalCount = paginatedAccountingPeriods.TotalCount
        });
    }

    /// <summary>
    /// Retrieves all the open Accounting Periods from the database
    /// </summary>
    /// <returns>The collection of all open Accounting Periods</returns>
    [HttpGet("open")]
    public IReadOnlyCollection<AccountingPeriodModel> GetAllOpen() => accountingPeriodRepository.GetAllOpenPeriods()
        .OrderByDescending(accountingPeriod => accountingPeriod.PeriodStartDate)
        .Select(AccountingPeriodMapper.ToModel).ToList();

    /// <summary>
    /// Retrieves the Transactions for the Accounting Period that matches the provided ID
    /// </summary>
    [HttpGet("{accountingPeriodId}/transactions")]
    [ProducesResponseType(typeof(CollectionModel<TransactionModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetTransactions(Guid accountingPeriodId, [FromQuery] AccountingPeriodTransactionQueryParameterModel queryParameters)
    {
        if (!accountingPeriodMapper.TryToDomain(accountingPeriodId, out AccountingPeriod? accountingPeriod, out IActionResult? errorResult))
        {
            return errorResult;
        }
        AccountingPeriodTransactionSortOrder? sortBy = null;
        if (queryParameters.SortBy != null && !AccountingPeriodTransactionSortOrderMapper.TryToData(queryParameters.SortBy.Value, out sortBy, out errorResult))
        {
            return errorResult;
        }
        IEnumerable<Account> accounts = [];
        if (queryParameters.Accounts != null)
        {
            foreach (Guid accountId in queryParameters.Accounts)
            {
                if (!accountMapper.TryToDomain(accountId, out Account? account, out errorResult))
                {
                    return errorResult;
                }
                accounts = accounts.Append(account);
            }
        }
        PaginatedCollection<Transaction> paginatedResults = transactionRepository.GetManyByAccountingPeriod(accountingPeriod.Id, new GetAccountingPeriodTransactionsRequest
        {
            SortBy = sortBy,
            MinDate = queryParameters.MinDate,
            MaxDate = queryParameters.MaxDate,
            Locations = queryParameters.Locations,
            Accounts = accounts.Any() ? accounts.Select(account => account.Id).ToList() : null,
            Limit = queryParameters.Limit,
            Offset = queryParameters.Offset
        });
        return Ok(new CollectionModel<TransactionModel>
        {
            Items = paginatedResults.Items.Select(transactionMapper.ToModel).ToList(),
            TotalCount = paginatedResults.TotalCount
        });
    }

    /// <summary>
    /// Creates a new Accounting Period with the provided properties
    /// </summary>
    /// <param name="createAccountingPeriodModel">Request to create an Accounting Period</param>
    /// <returns>The created Accounting Period</returns>
    [HttpPost("")]
    [ProducesResponseType(typeof(AccountingPeriodModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync(CreateAccountingPeriodModel createAccountingPeriodModel)
    {
        if (!accountingPeriodService.TryCreate(createAccountingPeriodModel.Year, createAccountingPeriodModel.Month, out AccountingPeriod? newAccountingPeriod, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(ErrorMapper.ToModel("Failed to create Accounting Period.", exceptions));
        }
        accountingPeriodRepository.Add(newAccountingPeriod);
        await unitOfWork.SaveChangesAsync();
        return Ok(AccountingPeriodMapper.ToModel(newAccountingPeriod));
    }

    /// <summary>
    /// Closes the Accounting Period with the provided ID
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period to close</param>
    /// <returns>The closed Accounting Period</returns>
    [HttpPost("{accountingPeriodId}/close")]
    [ProducesResponseType(typeof(AccountingPeriodModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CloseAsync(Guid accountingPeriodId)
    {
        if (!accountingPeriodMapper.TryToDomain(accountingPeriodId, out AccountingPeriod? accountingPeriod, out IActionResult? errorResult))
        {
            return errorResult;
        }
        if (!accountingPeriodService.TryClose(accountingPeriod, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(ErrorMapper.ToModel("Failed to close Accounting Period.", exceptions));
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(AccountingPeriodMapper.ToModel(accountingPeriod));
    }

    /// <summary>
    /// Deletes the Accounting Period with the provided ID
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period to delete</param>
    [HttpDelete("{accountingPeriodId}")]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteAsync(Guid accountingPeriodId)
    {
        if (!accountingPeriodMapper.TryToDomain(accountingPeriodId, out AccountingPeriod? accountingPeriod, out IActionResult? errorResult))
        {
            return errorResult;
        }
        if (!accountingPeriodService.TryDelete(accountingPeriod, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(ErrorMapper.ToModel("Failed to delete Accounting Period.", exceptions));
        }
        await unitOfWork.SaveChangesAsync();
        return Ok();
    }
}