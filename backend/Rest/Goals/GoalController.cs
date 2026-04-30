using Data;
using Data.Goals;
using Domain.AccountingPeriods;
using Domain.Exceptions;
using Domain.Funds;
using Domain.Goals;
using Microsoft.AspNetCore.Mvc;
using Models.Goals;
using Rest.AccountingPeriods;
using Rest.Funds;

namespace Rest.Goals;

/// <summary>
/// Controller class that exposes endpoints related to Goals for a Fund
/// </summary>
[ApiController]
[Route("/goals")]
public sealed class GoalController(
    UnitOfWork unitOfWork,
    AccountingPeriodConverter accountingPeriodConverter,
    FundConverter fundConverter,
    GoalService goalService,
    GoalConverter goalConverter,
    GoalRepository goalRepository) : ControllerBase
{
    /// <summary>
    /// Retrieves the Goal that matches the provided accounting period for the provided Fund
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(typeof(GoalModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Get(GetGoalModel getGoalModel)
    {
        Dictionary<string, string[]> errors = [];
        if (!goalRepository.TryGetByFundAndAccountingPeriod(getGoalModel.FundId, getGoalModel.AccountingPeriodId, out Goal? goal))
        {
            errors.Add(nameof(getGoalModel.AccountingPeriodId), [$"Goal with Accounting Period ID {getGoalModel.AccountingPeriodId} not found."]);
        }
        if (errors.Count > 0 || goal == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Goal.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        return Ok(goalConverter.ToModel(goal));
    }

    /// <summary>
    /// Creates a new Goal for the provided Fund with the provided properties
    /// </summary>
    [HttpPost("")]
    [ProducesResponseType(typeof(GoalModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync(CreateGoalModel createGoalModel)
    {
        Dictionary<string, string[]> errors = [];
        if (!fundConverter.TryToDomain(createGoalModel.FundId, out Fund? fund))
        {
            errors.Add(nameof(createGoalModel.FundId), [$"Fund with ID {createGoalModel.FundId} was not found."]);
        }
        if (!accountingPeriodConverter.TryToDomain(createGoalModel.AccountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            errors.Add(nameof(createGoalModel.AccountingPeriodId), [$"Accounting Period with ID {createGoalModel.AccountingPeriodId} was not found."]);
        }
        if (!GoalTypeConverter.TryToDomain(createGoalModel.GoalType, out GoalType? goalType))
        {
            errors.Add(nameof(createGoalModel.GoalType), [$"Unrecognized Goal Type: {createGoalModel.GoalType}"]);
        }
        if (errors.Count > 0 || fund == null || accountingPeriod == null || goalType == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Goal.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }

        if (!goalService.TryCreate(
            new CreateGoalRequest
            {
                Fund = fund,
                AccountingPeriod = accountingPeriod,
                GoalType = goalType.Value,
                GoalAmount = createGoalModel.GoalAmount,
            },
            out Goal? newGoal,
            out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Goal.",
                Errors = GetGoalErrors(
                    exceptions,
                    nameof(createGoalModel.FundId),
                    nameof(createGoalModel.AccountingPeriodId),
                    nameof(createGoalModel.GoalType),
                    nameof(createGoalModel.GoalAmount)),
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }

        goalRepository.Add(newGoal);
        await unitOfWork.SaveChangesAsync();
        return Ok(goalConverter.ToModel(newGoal));
    }

    /// <summary>
    /// Updates the provided Goal with the provided properties
    /// </summary>
    [HttpPost("{goalId}")]
    [ProducesResponseType(typeof(GoalModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateAsync(Guid goalId, UpdateGoalModel updateGoalModel)
    {
        Dictionary<string, string[]> errors = [];
        if (!goalRepository.TryGetById(goalId, out Goal? goalToUpdate))
        {
            errors.Add(nameof(goalId), [$"Goal with ID {goalId} was not found."]);
        }
        if (errors.Count > 0 || goalToUpdate == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Goal.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        if (!GoalTypeConverter.TryToDomain(updateGoalModel.GoalType, out GoalType? goalType))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Goal.",
                Errors = { [nameof(updateGoalModel.GoalType)] = new[] { $"Unrecognized Goal Type: {updateGoalModel.GoalType}" } },
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        if (!goalService.TryUpdate(goalToUpdate, goalType.Value, updateGoalModel.GoalAmount, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Goal.",
                Errors = GetGoalErrors(
                    exceptions,
                    nameof(goalToUpdate.Fund.Id),
                    nameof(goalToUpdate.AccountingPeriodId),
                    nameof(updateGoalModel.GoalType),
                    nameof(updateGoalModel.GoalAmount)),
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }

        await unitOfWork.SaveChangesAsync();
        return Ok(goalConverter.ToModel(goalToUpdate));
    }

    /// <summary>
    /// Deletes the Goal with the provided Accounting Period ID for the provided Fund
    /// </summary>
    [HttpDelete("{goalId}")]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteAsync(Guid goalId)
    {
        Dictionary<string, string[]> errors = [];
        if (!goalRepository.TryGetById(goalId, out Goal? goalToDelete))
        {
            errors.Add(nameof(goalId), [$"Goal with ID {goalId} was not found."]);
        }
        if (errors.Count > 0 || goalToDelete == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to delete Goal.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        if (!goalService.TryDelete(goalToDelete, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to delete Goal.",
                Errors = {
                    { string.Empty, exceptions.Select(exception => exception.Message).ToArray() }
                },
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }

        await unitOfWork.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Maps goal exceptions to validation errors
    /// </summary>
    private static Dictionary<string, string[]> GetGoalErrors(
        IEnumerable<Exception> exceptions,
        string fundKey,
        string relationshipKey,
        string goalTypeKey,
        string goalAmountKey) =>
        exceptions.GroupBy(exception => exception switch
        {
            InvalidAccountingPeriodException => relationshipKey,
            InvalidGoalTypeException => goalTypeKey,
            InvalidFundException invalidFundException
                when invalidFundException.Message.Contains("goal amount", StringComparison.InvariantCultureIgnoreCase) => goalAmountKey,
            InvalidFundException invalidFundException
                when invalidFundException.Message.Contains("already exists", StringComparison.InvariantCultureIgnoreCase) => relationshipKey,
            InvalidFundException => fundKey,
            _ => string.Empty,
        }).ToDictionary(grouping => grouping.Key, grouping => grouping.Select(exception => exception.Message).ToArray());
}