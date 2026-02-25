using Data;
using Data.AccountingPeriods;
using Data.Transactions;
using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Exceptions;
using Domain.Accounts;
using Domain.Transactions;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.AccountingPeriods;
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
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetAll([FromQuery] AccountingPeriodQueryParameterModel queryParameters)
    {
        Dictionary<string, string[]> errors = [];

        AccountingPeriodSortOrder? accountingPeriodSortOrder = null;
        if (queryParameters.SortBy != null && !AccountingPeriodSortOrderMapper.TryToData(queryParameters.SortBy.Value, out accountingPeriodSortOrder))
        {
            errors.Add(nameof(queryParameters.SortBy), new[] { $"Unrecognized Accounting Period Sort Order: {queryParameters.SortBy.Value}" });
        }
        if (errors.Count > 0)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Accounting Periods.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
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
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetTransactions(Guid accountingPeriodId, [FromQuery] AccountingPeriodTransactionQueryParameterModel queryParameters)
    {
        Dictionary<string, string[]> errors = [];
        if (!accountingPeriodMapper.TryToDomain(accountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            errors.Add(nameof(accountingPeriodId), new[] { $"Accounting Period with ID {accountingPeriodId} not found." });
        }
        AccountingPeriodTransactionSortOrder? sortBy = null;
        if (queryParameters.SortBy != null && !AccountingPeriodTransactionSortOrderMapper.TryToData(queryParameters.SortBy.Value, out sortBy))
        {
            errors.Add(nameof(queryParameters.SortBy), new[] { $"Unrecognized Accounting Period Transaction Sort Order: {queryParameters.SortBy.Value}" });
        }
        IEnumerable<Account> accounts = [];
        if (queryParameters.Accounts != null)
        {
            foreach ((int index, Guid accountId) in queryParameters.Accounts.Index())
            {
                if (!accountMapper.TryToDomain(accountId, out Account? account))
                {
                    errors.Add($"{nameof(queryParameters.Accounts)}[{index}]", new[] { $"Account with ID {accountId} not found." });
                }
                else
                {
                    accounts = accounts.Append(account);
                }
            }
        }
        if (errors.Count > 0 || accountingPeriod == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Accounting Period Transactions.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
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
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync(CreateAccountingPeriodModel createAccountingPeriodModel)
    {
        if (!accountingPeriodService.TryCreate(createAccountingPeriodModel.Year, createAccountingPeriodModel.Month, out AccountingPeriod? newAccountingPeriod, out IEnumerable<Exception> exceptions))
        {
            var errors = exceptions.GroupBy(exception => exception switch
            {
                InvalidYearException => nameof(createAccountingPeriodModel.Year),
                InvalidMonthException => nameof(createAccountingPeriodModel.Month),
                _ => string.Empty
            }).ToDictionary(grouping => grouping.Key, grouping => grouping.Select(exception => exception.Message).ToArray());
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Accounting Period.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
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
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CloseAsync(Guid accountingPeriodId)
    {
        Dictionary<string, string[]> errors = [];
        if (!accountingPeriodMapper.TryToDomain(accountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            errors.Add(nameof(accountingPeriodId), new[] { $"Accounting Period with ID {accountingPeriodId} not found." });
        }
        if (errors.Count > 0 || accountingPeriod == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to close Accounting Period.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        if (!accountingPeriodService.TryClose(accountingPeriod, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to close Accounting Period.",
                Errors = new Dictionary<string, string[]>
                {
                    { string.Empty, exceptions.Select(exception => exception.Message).ToArray() }
                },
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(AccountingPeriodMapper.ToModel(accountingPeriod));
    }

    /// <summary>
    /// Deletes the Accounting Period with the provided ID
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period to delete</param>
    [HttpDelete("{accountingPeriodId}")]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteAsync(Guid accountingPeriodId)
    {
        Dictionary<string, string[]> errors = [];
        if (!accountingPeriodMapper.TryToDomain(accountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            errors.Add(nameof(accountingPeriodId), new[] { $"Accounting Period with ID {accountingPeriodId} not found." });
        }
        if (errors.Count > 0 || accountingPeriod == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to delete Accounting Period.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        if (!accountingPeriodService.TryDelete(accountingPeriod, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to delete Accounting Period.",
                Errors = new Dictionary<string, string[]>
                {
                    { string.Empty, exceptions.Select(exception => exception.Message).ToArray() }
                },
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok();
    }
}