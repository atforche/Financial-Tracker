using Data;
using Data.AccountingPeriods;
using Domain.AccountingPeriods;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.AccountingPeriods;

namespace Rest.AccountingPeriods;

/// <summary>
/// Controller class that exposes endpoints related to Accounting Periods
/// </summary>
[ApiController]
[Route("/accounting-periods")]
public sealed class AccountingPeriodController(UnitOfWork unitOfWork,
    AccountingPeriodRepository accountingPeriodRepository,
    AccountingPeriodConverter accountingPeriodConverter,
    AccountingPeriodTrendsGetter accountingPeriodTrendsGetter,
    AccountingPeriodGetter accountingPeriodGetter,
    AccountingPeriodService accountingPeriodService,
    CurrentAccountingPeriodGetter accountingPeriodCurrentGetter) : ControllerBase
{
    /// <summary>
    /// Retrieves the Accounting Periods that match the specified criteria
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(typeof(CollectionModel<AccountingPeriodModel>), StatusCodes.Status200OK)]
    public IActionResult GetMany([FromQuery] AccountingPeriodQueryParameterModel queryParameters) =>
        Ok(accountingPeriodGetter.Get(queryParameters));

    /// <summary>
    /// Retrieves snapshot data for the current Accounting Period.
    /// </summary>
    [HttpGet("current")]
    [ProducesResponseType(typeof(CurrentAccountingPeriodModel), StatusCodes.Status200OK)]
    public IActionResult GetCurrent([FromQuery] CurrentAccountingPeriodQueryParameterModel queryParameters) =>
        Ok(accountingPeriodCurrentGetter.Get(queryParameters));

    /// <summary>
    /// Retrieves trends data for Accounting Periods across a range of Accounting Periods.
    /// </summary>
    [HttpGet("trends")]
    [ProducesResponseType(typeof(AccountingPeriodTrendsModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetTrends([FromQuery] AccountingPeriodTrendsQueryParameterModel queryParameters)
    {
        if (!accountingPeriodTrendsGetter.TryGet(queryParameters, out AccountingPeriodTrendsModel? trends, out Dictionary<string, string[]> errors))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Accounting Period trends.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        return Ok(trends);
    }

    /// <summary>
    /// Retrieves all the open Accounting Periods from the database
    /// </summary>
    [HttpGet("open")]
    public IReadOnlyCollection<AccountingPeriodModel> GetAllOpen() => accountingPeriodRepository.GetAllOpenPeriods()
        .OrderByDescending(accountingPeriod => accountingPeriod.PeriodStartDate)
        .Select(accountingPeriodConverter.ToModel).ToList();

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