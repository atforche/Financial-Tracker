using Data;
using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Domain.Funds;
using Domain.Funds.Queries;
using Domain.Validation;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Funds;
using Rest.AccountingPeriods;

namespace Rest.Funds;

/// <summary>
/// Controller class that exposes endpoints related to Funds
/// </summary>
[ApiController]
[Route("/funds")]
public sealed class FundController(
    UnitOfWork unitOfWork,
    AccountingPeriodQueryService accountingPeriodQueryService,
    FundConverter fundConverter,
    FundQueryService fundQueryService,
    FundService fundService,
    FundBalanceEventQueryService fundBalanceEventQueryService,
    FundBalanceEventConverter fundBalanceEventConverter) : ControllerBase
{
    /// <summary>
    /// Retrieves Fund Balance Events in a date range.
    /// </summary>
    [HttpGet("balance-events/date-range")]
    [ProducesResponseType(typeof(CollectionModel<FundBalanceEventModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionModel<FundBalanceEventModel>>> GetBalanceEventsAsync(
        [FromQuery] FundBalanceEventsInDateRangeQueryParameterModel query,
        CancellationToken cancellationToken) =>
        Ok(fundBalanceEventConverter.ToModel(await fundBalanceEventQueryService.GetAsync(
            fundBalanceEventConverter.ToDomain(query),
            cancellationToken)));

    /// <summary>
    /// Retrieves Fund Balance Events in an Accounting Period range.
    /// </summary>
    [HttpGet("balance-events/accounting-period-range")]
    [ProducesResponseType(typeof(CollectionModel<FundBalanceEventModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CollectionModel<FundBalanceEventModel>>> GetBalanceEventsAsync(
        [FromQuery] FundBalanceEventsInAccountingPeriodRangeQueryParameterModel query,
        CancellationToken cancellationToken)
    {
        FundBalanceEventAccountingPeriodRangeQueryResult result = await fundBalanceEventQueryService.GetAsync(
            fundBalanceEventConverter.ToDomain(query),
            cancellationToken);
        return result.Page == null
            ? UnprocessableEntity(AccountingPeriodRangeValidationProblem.Create(result.Failure, query.Range.Start, query.Range.End, "Unable to retrieve Fund balance events."))
            : Ok(fundBalanceEventConverter.ToModel(result.Page));
    }

    /// <summary>
    /// Retrieves the Fund that matches the provided ID
    /// </summary>
    [HttpGet("{fundId}")]
    [ProducesResponseType(typeof(FundModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<FundModel>> GetAsync(Guid fundId, CancellationToken cancellationToken)
    {
        Fund? fund = await fundQueryService.GetByIdAsync(fundId, cancellationToken);
        return fund == null ? NotFound() : Ok(fundConverter.ToModel(fund));
    }

    /// <summary>
    /// Retrieves the Funds that match the specified criteria
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(typeof(CollectionModel<FundModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionModel<FundModel>>> GetManyAsync(
        [FromQuery] FundQueryParameterModel queryParameters,
        CancellationToken cancellationToken) =>
        Ok(fundConverter.ToModel(await fundQueryService.GetAsync(
            fundConverter.ToDomain(queryParameters), cancellationToken)));

    /// <summary>
    /// Retrieves Funds with current balances.
    /// </summary>
    [HttpGet("with-balances")]
    [ProducesResponseType(typeof(CollectionModel<FundWithBalanceModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionModel<FundWithBalanceModel>>> GetWithBalancesAsync(
        [FromQuery] FundWithBalanceQueryParameterModel query,
        CancellationToken cancellationToken) =>
        Ok(fundConverter.ToModel(await fundQueryService.GetWithBalancesAsync(
            fundConverter.ToDomain(query), cancellationToken)));

    /// <summary>
    /// Retrieves Fund balances over a date range.
    /// </summary>
    [HttpGet("date-range")]
    [ProducesResponseType(typeof(FundsInDateRangeModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<FundsInDateRangeModel>> GetDateRangeAsync(
        [FromQuery] FundsInDateRangeQueryParameterModel query,
        CancellationToken cancellationToken) =>
        Ok(fundConverter.ToModel(await fundQueryService.GetDateRangeAsync(
            fundConverter.ToDomain(query),
            cancellationToken)));

    /// <summary>
    /// Retrieves Fund balances over an Accounting Period range.
    /// </summary>
    [HttpGet("accounting-period-range")]
    [ProducesResponseType(typeof(FundsInAccountingPeriodRangeModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<FundsInAccountingPeriodRangeModel>> GetAccountingPeriodRangeAsync(
        [FromQuery] FundsInAccountingPeriodRangeQueryParameterModel query,
        CancellationToken cancellationToken)
    {
        FundAccountingPeriodRangeQueryResult result = await fundQueryService.GetAccountingPeriodRangeAsync(
            fundConverter.ToDomain(query),
            cancellationToken);
        return result.Range == null
            ? UnprocessableEntity(AccountingPeriodRangeValidationProblem.Create(result.Failure, query.Range.Start, query.Range.End, "Unable to retrieve Fund range."))
            : Ok(fundConverter.ToModel(result.Range));
    }

    /// <summary>
    /// Creates a new Fund with the provided properties
    /// </summary>
    [HttpPost("")]
    [ProducesResponseType(typeof(FundModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync(CreateFundModel createFundModel)
    {
        Dictionary<string, string[]> errors = [];
        AccountingPeriod? accountingPeriod = await accountingPeriodQueryService.GetByIdAsync(createFundModel.AccountingPeriodId);
        if (accountingPeriod == null)
        {
            errors.Add(nameof(createFundModel.AccountingPeriodId), [$"Accounting Period with ID {createFundModel.AccountingPeriodId} was not found."]);
        }
        if (errors.Count > 0 || accountingPeriod == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Fund.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }

        if (!fundService.TryCreate(
            new CreateFundRequest
            {
                Name = createFundModel.Name,
                Description = createFundModel.Description,
                OpeningAccountingPeriod = accountingPeriod,
                RegularContribution = createFundModel.RegularContribution,
                MinimumFundedBalance = createFundModel.MinimumFundedBalance,
                MaximumFundedBalance = createFundModel.MaximumFundedBalance,
                TargetEndingBalance = createFundModel.TargetEndingBalance,
            },
            out Fund? newFund,
            out IEnumerable<ValidationError> validationErrors))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Fund.",
                Errors = ValidationErrorHelper.GroupValidationErrors(validationErrors, path => path == nameof(CreateFundRequest.OpeningAccountingPeriod)
                    ? nameof(CreateFundModel.AccountingPeriodId)
                    : path),
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(fundConverter.ToModel(newFund));
    }

    /// <summary>
    /// Onboards a new Fund with the provided properties
    /// </summary>
    [HttpPost("onboard")]
    [ProducesResponseType(typeof(FundModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> OnboardAsync(OnboardFundModel onboardFundModel)
    {
        if (!fundService.TryOnboard(
            new OnboardFundRequest
            {
                Name = onboardFundModel.Name,
                Description = onboardFundModel.Description,
                OnboardedBalance = onboardFundModel.OnboardedBalance,
                RegularContribution = onboardFundModel.RegularContribution,
                MinimumFundedBalance = onboardFundModel.MinimumFundedBalance,
                MaximumFundedBalance = onboardFundModel.MaximumFundedBalance,
                TargetEndingBalance = onboardFundModel.TargetEndingBalance,
            },
            out Fund? newFund,
            out IEnumerable<ValidationError> validationErrors))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to onboard Fund.",
                Errors = ValidationErrorHelper.GroupValidationErrors(validationErrors),
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(fundConverter.ToModel(newFund));
    }

    /// <summary>
    /// Updates the provided Fund with the provided properties
    /// </summary>
    [HttpPost("{fundId}")]
    [ProducesResponseType(typeof(FundModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateAsync(Guid fundId, UpdateFundModel updateFundModel)
    {
        Dictionary<string, string[]> errors = [];
        Fund? fundToUpdate = await fundQueryService.GetByIdAsync(fundId);
        if (fundToUpdate == null)
        {
            errors.Add(nameof(fundId), [$"Fund with ID {fundId} was not found."]);
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Fund.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        if (!fundService.TryUpdate(
                fundToUpdate,
                new UpdateFundRequest
                {
                    Name = updateFundModel.Name,
                    Description = updateFundModel.Description,
                },
                out IEnumerable<ValidationError> validationErrors))
        {
            errors = ValidationErrorHelper.GroupValidationErrors(validationErrors);
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Fund.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(fundConverter.ToModel(fundToUpdate));
    }

    /// <summary>
    /// Deletes the Fund with the provided ID
    /// </summary>
    [HttpDelete("{fundId}")]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteAsync(Guid fundId)
    {
        Dictionary<string, string[]> errors = [];
        Fund? fundToDelete = await fundQueryService.GetByIdAsync(fundId);
        if (fundToDelete == null)
        {
            errors.Add(nameof(fundId), [$"Fund with ID {fundId} was not found."]);
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to delete Fund.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        if (!fundService.TryDelete(fundToDelete, out IEnumerable<ValidationError> validationErrors))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to delete Fund.",
                Errors = ValidationErrorHelper.GroupValidationErrors(validationErrors),
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok();
    }
}