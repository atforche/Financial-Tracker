using Data;
using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Domain.Transactions.Income;
using Domain.Validation;
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
    AccountingPeriodQueryConverter accountingPeriodQueryConverter,
    AccountingPeriodQueryService accountingPeriodQueryService,
    AccountingPeriodService accountingPeriodService,
    IAccountingPeriodRepository accountingPeriodRepository) : ControllerBase
{
    /// <summary>
    /// Retrieves the Accounting Periods that match the specified criteria
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(typeof(CollectionModel<AccountingPeriodModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionModel<AccountingPeriodModel>>> GetManyAsync(
        [FromQuery] AccountingPeriodQueryParameterModel queryParameters,
        CancellationToken cancellationToken) =>
        Ok(accountingPeriodQueryConverter.ToModel(await accountingPeriodQueryService.GetAsync(
            accountingPeriodQueryConverter.ToDomain(queryParameters),
            cancellationToken)));

    /// <summary>
    /// Retrieves snapshot data for the current Accounting Period.
    /// </summary>
    [HttpGet("with-balances")]
    [ProducesResponseType(typeof(CollectionModel<AccountingPeriodWithBalanceModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionModel<AccountingPeriodWithBalanceModel>>> GetWithBalancesAsync(
        [FromQuery] AccountingPeriodWithBalanceQueryParameterModel queryParameters,
        CancellationToken cancellationToken) =>
        Ok(accountingPeriodQueryConverter.ToModel(await accountingPeriodQueryService.GetWithBalancesAsync(
            accountingPeriodQueryConverter.ToDomain(queryParameters),
            cancellationToken)));

    /// <summary>
    /// Retrieves the Accounting Period with the provided ID.
    /// </summary>
    [HttpGet("{accountingPeriodId:guid}")]
    [ProducesResponseType(typeof(AccountingPeriodWithBalanceModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountingPeriodWithBalanceModel>> GetByIdAsync(
        Guid accountingPeriodId,
        CancellationToken cancellationToken)
    {
        AccountingPeriodBalance? balance = await accountingPeriodQueryService.GetBalanceByIdAsync(accountingPeriodId, cancellationToken);
        return balance == null ? NotFound() : Ok(accountingPeriodQueryConverter.ToModel(balance));
    }

    /// <summary>
    /// Retrieves an Accounting Period with its Transactions.
    /// </summary>
    [HttpGet("{accountingPeriodId:guid}/transactions")]
    [ProducesResponseType(typeof(AccountingPeriodWithTransactionsModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountingPeriodWithTransactionsModel>> GetWithTransactionsAsync(
        Guid accountingPeriodId,
        [FromQuery] AccountingPeriodWithTransactionsQueryParameterModel query,
        CancellationToken cancellationToken)
    {
        AccountingPeriodTransactions? result = await accountingPeriodQueryService.GetWithTransactionsAsync(
            accountingPeriodQueryConverter.ToDomain(accountingPeriodId, query),
            cancellationToken);
        return result == null ? NotFound() : Ok(accountingPeriodQueryConverter.ToModel(result));
    }

    /// <summary>
    /// Retrieves Accounting Periods and totals for a contiguous range.
    /// </summary>
    [HttpGet("range")]
    [ProducesResponseType(typeof(AccountingPeriodsInRangeModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AccountingPeriodsInRangeModel>> GetRangeAsync(
        [FromQuery] AccountingPeriodsInRangeQueryParameterModel query,
        CancellationToken cancellationToken)
    {
        AccountingPeriodRangeQueryResult result = await accountingPeriodQueryService.GetRangeAsync(
            accountingPeriodQueryConverter.ToDomain(query),
            cancellationToken);
        if (result.Range != null)
        {
            return Ok(accountingPeriodQueryConverter.ToModel(result.Range));
        }
        return UnprocessableEntity(AccountingPeriodRangeValidationProblem.Create(
            result.Failure,
            query.Range.Start,
            query.Range.End,
            "Unable to retrieve Accounting Period range."));
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
        if (!accountingPeriodService.TryCreate(
                new CreateAccountingPeriodRequest
                {
                    Year = createAccountingPeriodModel.Year,
                    Month = createAccountingPeriodModel.Month,
                    ExpectedIncomeSources = ToRequest(createAccountingPeriodModel.ExpectedIncomeSources),
                },
                out AccountingPeriod? newAccountingPeriod,
                out IEnumerable<ValidationError> validationErrors))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Accounting Period.",
                Errors = ValidationErrorHelper.GroupValidationErrors(validationErrors),
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        accountingPeriodRepository.Add(newAccountingPeriod);
        await unitOfWork.SaveChangesAsync();
        AccountingPeriodBalance? balance = await accountingPeriodQueryService.GetBalanceByIdAsync(newAccountingPeriod.Id.Value);
        return Ok(accountingPeriodQueryConverter.ToModel(balance!));
    }

    /// <summary>
    /// Replaces expected income sources for an open Accounting Period.
    /// </summary>
    [HttpPost("{accountingPeriodId:guid}/expected-income-sources")]
    [ProducesResponseType(typeof(AccountingPeriodWithBalanceModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ReplaceExpectedIncomeSourcesAsync(
        Guid accountingPeriodId,
        [FromBody] IReadOnlyCollection<ExpectedIncomeSourceRequestModel> sources)
    {
        if (!accountingPeriodRepository.TryGetById(accountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            return NotFound();
        }
        if (!AccountingPeriodService.TryReplaceExpectedIncomeSources(accountingPeriod, ToRequest(sources), out IEnumerable<ValidationError> validationErrors))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update expected income sources.",
                Errors = ValidationErrorHelper.GroupValidationErrors(validationErrors),
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        await unitOfWork.SaveChangesAsync();
        AccountingPeriodBalance? balance = await accountingPeriodQueryService.GetBalanceByIdAsync(accountingPeriodId);
        return Ok(accountingPeriodQueryConverter.ToModel(balance!));
    }

    /// <summary>
    /// Closes the Accounting Period with the provided ID
    /// </summary>
    [HttpPost("{accountingPeriodId}/close")]
    [ProducesResponseType(typeof(AccountingPeriodModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CloseAsync(Guid accountingPeriodId)
    {
        Dictionary<string, string[]> errors = [];
        if (!accountingPeriodRepository.TryGetById(accountingPeriodId, out AccountingPeriod? accountingPeriod))
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
        if (!accountingPeriodService.TryClose(accountingPeriod, out IEnumerable<ValidationError> validationErrors))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to close Accounting Period.",
                Errors = ValidationErrorHelper.GroupValidationErrors(validationErrors),
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        AccountingPeriodBalance? balance = await accountingPeriodQueryService.GetBalanceByIdAsync(accountingPeriodId);
        return Ok(accountingPeriodQueryConverter.ToModel(balance!));
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
        if (!accountingPeriodRepository.TryGetById(accountingPeriodId, out AccountingPeriod? accountingPeriod))
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
        if (!accountingPeriodService.TryReopen(accountingPeriod, out IEnumerable<ValidationError> validationErrors))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to reopen Accounting Period.",
                Errors = ValidationErrorHelper.GroupValidationErrors(validationErrors),
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        AccountingPeriodBalance? balance = await accountingPeriodQueryService.GetBalanceByIdAsync(accountingPeriodId);
        return Ok(accountingPeriodQueryConverter.ToModel(balance!));
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
        if (!accountingPeriodRepository.TryGetById(accountingPeriodId, out AccountingPeriod? accountingPeriod))
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
        if (!accountingPeriodService.TryDelete(accountingPeriod, out IEnumerable<ValidationError> validationErrors))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to delete Accounting Period.",
                Errors = ValidationErrorHelper.GroupValidationErrors(validationErrors),
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Converts expected-income API models to domain requests.
    /// </summary>
    private static List<ExpectedIncomeSourceRequest> ToRequest(
        IReadOnlyCollection<ExpectedIncomeSourceRequestModel> sources) => sources.Select(source => new ExpectedIncomeSourceRequest
        {
            Name = source.Name,
            IncomeLines = source.IncomeLines.Select(line => new IncomeLine(line.Description, line.Amount)).ToList(),
            IncomeDeductions = source.IncomeDeductions.Select(deduction => new IncomeDeduction(deduction.Description, deduction.Amount)).ToList(),
            ExpectedDates = source.ExpectedDates,
        }).ToList();
}