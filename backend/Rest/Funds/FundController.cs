using Data;
using Data.Funds;
using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Goals;
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
    AccountingPeriodConverter accountingPeriodConverter,
    FundConverter fundConverter,
    FundQueryService fundQueryService,
    FinancialRangeQueryService financialRangeQueryService,
    FundService fundService) : ControllerBase
{
    /// <summary>
    /// Retrieves the Fund that matches the provided ID
    /// </summary>
    [HttpGet("{fundId}")]
    [ProducesResponseType(typeof(FundModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<FundModel>> GetAsync(Guid fundId, CancellationToken cancellationToken)
    {
        FundModel? model = await fundQueryService.GetByIdAsync(fundId, cancellationToken);
        return model == null ? NotFound() : Ok(model);
    }

    /// <summary>
    /// Retrieves the Funds that match the specified criteria
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(typeof(CollectionModel<FundModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionModel<FundModel>>> GetManyAsync(
        [FromQuery] FundQueryParameterModel queryParameters,
        CancellationToken cancellationToken) =>
        Ok(await fundQueryService.GetAsync(queryParameters, cancellationToken));

    /// <summary>
    /// Retrieves Funds with current balances.
    /// </summary>
    [HttpGet("with-balances")]
    [ProducesResponseType(typeof(CollectionModel<FundWithBalanceModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionModel<FundWithBalanceModel>>> GetWithBalancesAsync(
        [FromQuery] FundWithBalanceQueryParameterModel query,
        CancellationToken cancellationToken) =>
        Ok(await fundQueryService.GetWithBalancesAsync(query, cancellationToken));

    /// <summary>
    /// Retrieves Fund balances over a date range.
    /// </summary>
    [HttpGet("date-range")]
    [ProducesResponseType(typeof(FundsInDateRangeModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<FundsInDateRangeModel>> GetDateRangeAsync(
        [FromQuery] FundsInDateRangeQueryParameterModel query,
        CancellationToken cancellationToken) =>
        Ok(await financialRangeQueryService.GetFundsAsync(query, cancellationToken));

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
        FundsInAccountingPeriodRangeModel? model = await financialRangeQueryService.GetFundsAsync(query, cancellationToken);
        return model == null
            ? UnprocessableEntity(new ValidationProblemDetails { Title = "Unable to resolve Accounting Period range.", Status = StatusCodes.Status422UnprocessableEntity })
            : Ok(model);
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
        if (!accountingPeriodConverter.TryToDomain(createFundModel.AccountingPeriodId, out AccountingPeriod? accountingPeriod))
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

        if (!FundGoalTypeConverter.TryToDomain(createFundModel.AssignmentGoalType, out AssignmentGoalType? assignmentGoalType))
        {
            errors.Add(nameof(createFundModel.AssignmentGoalType), [$"Unrecognized assignment goal type: {createFundModel.AssignmentGoalType}"]);
        }
        if (!FundGoalTypeConverter.TryToDomain(createFundModel.SpendingGoalType, out SpendingGoalType? spendingGoalType))
        {
            errors.Add(nameof(createFundModel.SpendingGoalType), [$"Unrecognized spending goal type: {createFundModel.SpendingGoalType}"]);
        }
        if (errors.Count > 0 || assignmentGoalType == null || spendingGoalType == null)
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
                AssignmentGoalType = assignmentGoalType.Value,
                AssignmentGoalAmount = createFundModel.AssignmentGoalAmount,
                SpendingGoalType = spendingGoalType.Value,
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
        return Ok(await fundQueryService.GetByIdAsync(newFund.Id.Value));
    }

    /// <summary>
    /// Onboards a new Fund with the provided properties
    /// </summary>
    [HttpPost("onboard")]
    [ProducesResponseType(typeof(FundModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> OnboardAsync(OnboardFundModel onboardFundModel)
    {
        Dictionary<string, string[]> errors = [];
        if (!FundGoalTypeConverter.TryToDomain(onboardFundModel.AssignmentGoalType, out AssignmentGoalType? assignmentGoalType))
        {
            errors.Add(nameof(onboardFundModel.AssignmentGoalType), [$"Unrecognized assignment goal type: {onboardFundModel.AssignmentGoalType}"]);
        }
        if (!FundGoalTypeConverter.TryToDomain(onboardFundModel.SpendingGoalType, out SpendingGoalType? spendingGoalType))
        {
            errors.Add(nameof(onboardFundModel.SpendingGoalType), [$"Unrecognized spending goal type: {onboardFundModel.SpendingGoalType}"]);
        }
        if (errors.Count > 0 || assignmentGoalType == null || spendingGoalType == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to onboard Fund.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }

        if (!fundService.TryOnboard(
            new OnboardFundRequest
            {
                Name = onboardFundModel.Name,
                Description = onboardFundModel.Description,
                OnboardedBalance = onboardFundModel.OnboardedBalance,
                AssignmentGoalType = assignmentGoalType.Value,
                AssignmentGoalAmount = onboardFundModel.AssignmentGoalAmount,
                SpendingGoalType = spendingGoalType.Value,
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
        return Ok(await fundQueryService.GetByIdAsync(newFund.Id.Value));
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
        if (!fundConverter.TryToDomain(fundId, out Fund? fundToUpdate))
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
        return Ok(await fundQueryService.GetByIdAsync(fundId));
    }

    /// <summary>
    /// Deletes the Fund with the provided ID
    /// </summary>
    [HttpDelete("{fundId}")]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteAsync(Guid fundId)
    {
        Dictionary<string, string[]> errors = [];
        if (!fundConverter.TryToDomain(fundId, out Fund? fundToDelete))
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