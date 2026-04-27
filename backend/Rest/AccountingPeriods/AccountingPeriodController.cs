using Data;
using Data.AccountingPeriods;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Exceptions;
using Domain.Funds;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.AccountingPeriods;
using Models.Transactions;
using Rest.Accounts;
using Rest.Funds;

namespace Rest.AccountingPeriods;

/// <summary>
/// Controller class that exposes endpoints related to Accounting Periods
/// </summary>
[ApiController]
[Route("/accounting-periods")]
public sealed class AccountingPeriodController(UnitOfWork unitOfWork,
    AccountConverter accountConverter,
    AccountingPeriodRepository accountingPeriodRepository,
    AccountingPeriodAccountGetter accountingPeriodAccountGetter,
    AccountingPeriodConverter accountingPeriodConverter,
    AccountingPeriodFundConverter accountingPeriodFundConverter,
    AccountingPeriodFundGetter accountingPeriodFundGetter,
    AccountingPeriodGetter accountingPeriodGetter,
    AccountingPeriodService accountingPeriodService,
    AccountingPeriodTransactionGetter accountingPeriodTransactionGetter,
    FundConverter fundConverter,
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository) : ControllerBase
{
    /// <summary>
    /// Retrieves the Accounting Period that matches the provided ID
    /// </summary>
    [HttpGet("{accountingPeriodId}")]
    [ProducesResponseType(typeof(AccountingPeriodModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Get(Guid accountingPeriodId)
    {
        if (!accountingPeriodConverter.TryToDomain(accountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Accounting Period.",
                Errors = { [nameof(accountingPeriodId)] = new[] { $"Accounting Period with ID {accountingPeriodId} not found." } },
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        return Ok(accountingPeriodConverter.ToModel(accountingPeriod));
    }

    /// <summary>
    /// Retrieves the Accounting Periods that match the specified criteria
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(typeof(CollectionModel<AccountingPeriodModel>), StatusCodes.Status200OK)]
    public IActionResult GetMany([FromQuery] AccountingPeriodQueryParameterModel queryParameters) =>
        Ok(accountingPeriodGetter.Get(queryParameters));

    /// <summary>
    /// Retrieves all the open Accounting Periods from the database
    /// </summary>
    [HttpGet("open")]
    public IReadOnlyCollection<AccountingPeriodModel> GetAllOpen() => accountingPeriodRepository.GetAllOpenPeriods()
        .OrderByDescending(accountingPeriod => accountingPeriod.PeriodStartDate)
        .Select(accountingPeriodConverter.ToModel).ToList();

    /// <summary>
    /// Retrieves the Funds for the Accounting Period that match the specified criteria
    /// </summary>
    [HttpGet("{accountingPeriodId}/funds")]
    [ProducesResponseType(typeof(CollectionModel<AccountingPeriodFundModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetManyFunds(Guid accountingPeriodId, [FromQuery] AccountingPeriodFundQueryParameterModel queryParameters)
    {
        Dictionary<string, string[]> errors = [];
        if (!accountingPeriodConverter.TryToDomain(accountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            errors.Add(nameof(accountingPeriodId), new[] { $"Accounting Period with ID {accountingPeriodId} not found." });
        }
        if (errors.Count > 0 || accountingPeriod == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Accounting Period Funds.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        return Ok(accountingPeriodFundGetter.Get(accountingPeriod.Id, queryParameters));
    }

    /// <summary>
    /// Retrieves the Fund as it appeared in the provided Accounting Period
    /// </summary>
    [HttpGet("{accountingPeriodId}/funds/{fundId}")]
    [ProducesResponseType(typeof(AccountingPeriodFundModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetFund(Guid accountingPeriodId, Guid fundId)
    {
        Dictionary<string, string[]> errors = [];
        if (!fundConverter.TryToDomain(fundId, out Fund? fund))
        {
            errors.Add(nameof(fundId), new[] { $"Fund with ID {fundId} not found." });
        }
        if (!accountingPeriodConverter.TryToDomain(accountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            errors.Add(nameof(accountingPeriodId), new[] { $"Accounting Period with ID {accountingPeriodId} not found." });
        }
        if (errors.Count > 0 || fund == null || accountingPeriod == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Accounting Period Fund.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        AccountingPeriodBalanceHistory? accountingPeriodBalanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(accountingPeriod.Id);
        AccountingPeriodFundBalanceHistory? fundBalanceHistory = accountingPeriodBalanceHistory?.FundBalances.FirstOrDefault(fundHistory => fundHistory.Fund.Id == fund.Id);
        if (fundBalanceHistory == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Accounting Period Fund.",
                Errors = new Dictionary<string, string[]>
                {
                    { nameof(fundId), new[] { $"Fund with ID {fundId} not found for Accounting Period with ID {accountingPeriodId}." } }
                },
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        return Ok(accountingPeriodFundConverter.ToModel(fundBalanceHistory));
    }

    /// <summary>
    /// Retrieves the Accounts for the Accounting Period that match the specified criteria
    /// </summary>
    [HttpGet("{accountingPeriodId}/accounts")]
    [ProducesResponseType(typeof(CollectionModel<AccountingPeriodAccountModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetManyAccounts(Guid accountingPeriodId, [FromQuery] AccountingPeriodAccountQueryParameterModel queryParameters)
    {
        Dictionary<string, string[]> errors = [];
        if (!accountingPeriodConverter.TryToDomain(accountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            errors.Add(nameof(accountingPeriodId), new[] { $"Accounting Period with ID {accountingPeriodId} not found." });
        }
        if (errors.Count > 0 || accountingPeriod == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Accounting Period Accounts.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        return Ok(accountingPeriodAccountGetter.Get(accountingPeriod.Id, queryParameters));
    }

    /// <summary>
    /// Retrieves the Account as it appeared in the provided Accounting Period
    /// </summary>
    [HttpGet("{accountingPeriodId}/accounts/{accountId}")]
    [ProducesResponseType(typeof(AccountingPeriodAccountModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetAccountingPeriodAccount(Guid accountingPeriodId, Guid accountId)
    {
        Dictionary<string, string[]> errors = [];
        if (!accountConverter.TryToDomain(accountId, out Account? account))
        {
            errors.Add(nameof(accountId), new[] { $"Account with ID {accountId} not found." });
        }
        if (!accountingPeriodConverter.TryToDomain(accountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            errors.Add(nameof(accountingPeriodId), new[] { $"Accounting Period with ID {accountingPeriodId} not found." });
        }
        if (errors.Count > 0 || account == null || accountingPeriod == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Accounting Period Account.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        AccountingPeriodBalanceHistory? accountingPeriodBalanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(accountingPeriod.Id);
        AccountingPeriodAccountBalanceHistory? accountBalanceHistory = accountingPeriodBalanceHistory?.AccountBalances
            .FirstOrDefault(accountHistory => accountHistory.Account.Id == account.Id);
        if (accountBalanceHistory == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Accounting Period Account.",
                Errors = new Dictionary<string, string[]>
                {
                    { nameof(accountId), new[] { $"Account with ID {accountId} not found for Accounting Period with ID {accountingPeriodId}." } }
                },
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        return Ok(AccountingPeriodAccountConverter.ToModel(accountBalanceHistory));
    }

    /// <summary>
    /// Retrieves the Transactions for the Accounting Period that match the specified criteria
    /// </summary>
    [HttpGet("{accountingPeriodId}/transactions")]
    [ProducesResponseType(typeof(CollectionModel<TransactionModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetManyTransactions(Guid accountingPeriodId, [FromQuery] AccountingPeriodTransactionQueryParameterModel queryParameters)
    {
        Dictionary<string, string[]> errors = [];
        if (!accountingPeriodConverter.TryToDomain(accountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            errors.Add(nameof(accountingPeriodId), new[] { $"Accounting Period with ID {accountingPeriodId} not found." });
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
        return Ok(accountingPeriodTransactionGetter.Get(accountingPeriod.Id, queryParameters));
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
        if (!accountingPeriodService.TryCreate(createAccountingPeriodModel.Year, createAccountingPeriodModel.Month,
                out AccountingPeriod? newAccountingPeriod, out IEnumerable<Exception> exceptions))
        {
            var serviceErrors = exceptions.GroupBy(exception => exception switch
            {
                InvalidYearException => nameof(createAccountingPeriodModel.Year),
                InvalidMonthException => nameof(createAccountingPeriodModel.Month),
                _ => string.Empty
            }).ToDictionary(grouping => grouping.Key, grouping => grouping.Select(exception => exception.Message).ToArray());
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Accounting Period.",
                Errors = serviceErrors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        accountingPeriodRepository.Add(newAccountingPeriod);
        await unitOfWork.SaveChangesAsync();
        return Ok(accountingPeriodConverter.ToModel(newAccountingPeriod));
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
        if (!accountingPeriodConverter.TryToDomain(accountingPeriodId, out AccountingPeriod? accountingPeriod))
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
        return Ok(accountingPeriodConverter.ToModel(accountingPeriod));
    }

    /// <summary>
    /// Reopens the Accounting Period with the provided ID
    /// </summary>
    [HttpPost("{accountingPeriodId}/reopen")]
    [ProducesResponseType(typeof(AccountingPeriodModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ReopenAsync(Guid accountingPeriodId)
    {
        Dictionary<string, string[]> errors = [];
        if (!accountingPeriodConverter.TryToDomain(accountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            errors.Add(nameof(accountingPeriodId), [$"Accounting Period with ID {accountingPeriodId} not found."]);
        }
        if (errors.Count > 0 || accountingPeriod == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to reopen Accounting Period.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        if (!accountingPeriodService.TryReopen(accountingPeriod, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to reopen Accounting Period.",
                Errors = new Dictionary<string, string[]>
                {
                    { string.Empty, exceptions.Select(exception => exception.Message).ToArray() }
                },
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(accountingPeriodConverter.ToModel(accountingPeriod));
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
        if (!accountingPeriodConverter.TryToDomain(accountingPeriodId, out AccountingPeriod? accountingPeriod))
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