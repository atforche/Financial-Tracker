using Data;
using Data.Funds;
using Domain.AccountingPeriods;
using Domain.Exceptions;
using Domain.Funds;
using Domain.Goals;
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
    CurrentFundsGetter currentFundsGetter,
    FundTrendsGetter fundTrendsGetter,
    FundGetter fundGetter,
    FundSummaryGetter fundSummaryGetter,
    FundRepository fundRepository,
    FundService fundService) : ControllerBase
{
    /// <summary>
    /// Retrieves the Fund that matches the provided ID
    /// </summary>
    [HttpGet("{fundId}")]
    [ProducesResponseType(typeof(FundModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Get(Guid fundId)
    {
        if (!fundConverter.TryToDomain(fundId, out Fund? fund))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Fund.",
                Errors = { [nameof(fundId)] = new[] { $"Fund with ID {fundId} not found." } },
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        return Ok(fundConverter.ToModel(fund));
    }

    /// <summary>
    /// Retrieves the Funds that match the specified criteria
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(typeof(CollectionModel<FundModel>), StatusCodes.Status200OK)]
    public IActionResult GetMany([FromQuery] FundQueryParameterModel queryParameters) =>
        Ok(fundGetter.Get(queryParameters));

    /// <summary>
    /// Retrieves summary balances for Funds
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(FundSummaryModel), StatusCodes.Status200OK)]
    public IActionResult GetSummary() => Ok(fundSummaryGetter.Get());

    /// <summary>
    /// Retrieves current snapshot data for Funds.
    /// </summary>
    [HttpGet("current")]
    [ProducesResponseType(typeof(CurrentFundsModel), StatusCodes.Status200OK)]
    public IActionResult GetCurrent([FromQuery] CurrentFundsQueryParameterModel queryParameters) =>
        Ok(currentFundsGetter.Get(queryParameters));

    /// <summary>
    /// Retrieves trends data for Funds across a range of Accounting Periods.
    /// </summary>
    [HttpGet("trends")]
    [ProducesResponseType(typeof(FundTrendsModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetTrends([FromQuery] FundTrendsQueryParameterModel queryParameters)
    {
        if (!fundTrendsGetter.TryGet(queryParameters, out FundTrendsModel? trends, out Dictionary<string, string[]> errors))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Fund trends.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        return Ok(trends);
    }

    /// <summary>
    /// Retrieves the unassigned Fund
    /// </summary>
    [HttpGet("unassigned")]
    [ProducesResponseType(typeof(FundModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetUnassigned()
    {
        Fund? unassignedFund = fundRepository.GetUnassignedFund();
        if (unassignedFund == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Fund.",
                Errors = { [""] = ["The unassigned fund was not found."] },
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        return Ok(fundConverter.ToModel(unassignedFund));
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
            out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Fund.",
                Errors = exceptions.GroupBy(exception => exception switch
                {
                    InvalidNameException => nameof(createFundModel.Name),
                    InvalidAccountingPeriodException => nameof(createFundModel.AccountingPeriodId),
                    InvalidGoalTypeException when exception.Message.Contains("assignment", StringComparison.OrdinalIgnoreCase) => nameof(createFundModel.AssignmentGoalType),
                    InvalidGoalTypeException => nameof(createFundModel.SpendingGoalType),
                    InvalidFundException when exception.Message.Contains("Goal amount", StringComparison.OrdinalIgnoreCase) => nameof(createFundModel.AssignmentGoalAmount),
                    InvalidFundException => string.Empty,
                    _ => string.Empty,
                }).ToDictionary(grouping => grouping.Key, grouping => grouping.Select(exception => exception.Message).ToArray()),
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
            out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to onboard Fund.",
                Errors = exceptions.GroupBy(exception => exception switch
                {
                    InvalidNameException => nameof(onboardFundModel.Name),
                    InvalidAmountException => nameof(onboardFundModel.OnboardedBalance),
                    InvalidAccountingPeriodException => string.Empty,
                    InvalidGoalTypeException when exception.Message.Contains("assignment", StringComparison.OrdinalIgnoreCase) => nameof(onboardFundModel.AssignmentGoalType),
                    InvalidGoalTypeException => nameof(onboardFundModel.SpendingGoalType),
                    InvalidFundException when exception.Message.Contains("Goal amount", StringComparison.OrdinalIgnoreCase) => nameof(onboardFundModel.AssignmentGoalAmount),
                    InvalidFundException => string.Empty,
                    _ => string.Empty,
                }).ToDictionary(grouping => grouping.Key, grouping => grouping.Select(exception => exception.Message).ToArray()),
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
        if (!fundService.TryUpdate(fundToUpdate, updateFundModel.Name, updateFundModel.Description, out IEnumerable<Exception> exceptions))
        {
            errors = exceptions.GroupBy(e => e switch
            {
                InvalidNameException => nameof(updateFundModel.Name),
                _ => string.Empty
            }).ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray());
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
        if (!fundService.TryDelete(fundToDelete, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to delete Fund.",
                Errors = {
                    { string.Empty, exceptions.Select(e => e.Message).ToArray() }
                },
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok();
    }
}