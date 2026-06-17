using Data;
using Data.Goals;
using Domain.Exceptions;
using Domain.Goals;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Goals;

namespace Rest.Goals;

/// <summary>
/// Controller class that exposes endpoints related to Goals
/// </summary>
[ApiController]
[Route("/goals")]
public sealed class GoalController(
    UnitOfWork unitOfWork,
    AssignmentGoalGetter assignmentGoalGetter,
    AssignmentGoalRepository assignmentGoalRepository,
    AssignmentGoalService assignmentGoalService,
    CurrentGoalsGetter currentGoalsGetter,
    SpendingGoalGetter spendingGoalGetter,
    SpendingGoalRepository spendingGoalRepository,
    SpendingGoalService spendingGoalService,
    GoalTrendsGetter goalTrendsGetter,
    GoalConverter goalConverter) : ControllerBase
{
    /// <summary>
    /// Retrieves the Assignment Goal that matches the provided ID
    /// </summary>
    [HttpGet("assignment/{goalId}")]
    [ProducesResponseType(typeof(AssignmentGoalModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetAssignment(Guid goalId)
    {
        if (!assignmentGoalRepository.TryGetById(goalId, out AssignmentGoal? goal))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Goal.",
                Errors = { [nameof(goalId)] = new[] { $"Goal with ID {goalId} not found." } },
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        return Ok(goalConverter.ToModel(goal));
    }

    /// <summary>
    /// Retrieves the Assignment Goal that matches the provided accounting period for the provided Fund
    /// </summary>
    [HttpGet("assignment")]
    [ProducesResponseType(typeof(AssignmentGoalModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Get([FromQuery] GetGoalModel getGoalModel)
    {
        Dictionary<string, string[]> errors = [];
        if (!assignmentGoalRepository.TryGetByFundAndAccountingPeriod(getGoalModel.FundId, getGoalModel.AccountingPeriodId, out AssignmentGoal? goal))
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
    /// Retrieves the Assignment Goals that match the specified criteria
    /// </summary>
    [HttpGet("assignment/many")]
    [ProducesResponseType(typeof(CollectionModel<AssignmentGoalModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetManyAssignment([FromQuery] AssignmentGoalQueryParameterModel queryParameters)
    {
        if (!assignmentGoalGetter.TryGet(queryParameters, out CollectionModel<AssignmentGoalModel>? goals, out Dictionary<string, string[]> errors))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Goals.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }

        return Ok(goals);
    }

    /// <summary>
    /// Retrieves the Spending Goal that matches the provided ID
    /// </summary>
    [HttpGet("spending/{goalId}")]
    [ProducesResponseType(typeof(SpendingGoalModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetSpending(Guid goalId)
    {
        if (!spendingGoalRepository.TryGetById(goalId, out SpendingGoal? goal))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Goal.",
                Errors = { [nameof(goalId)] = new[] { $"Goal with ID {goalId} not found." } },
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        return Ok(goalConverter.ToModel(goal));
    }

    /// <summary>
    /// Retrieves the Spending Goal that matches the provided accounting period for the provided Fund
    /// </summary>
    [HttpGet("spending")]
    [ProducesResponseType(typeof(SpendingGoalModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetSpending([FromQuery] GetGoalModel getGoalModel)
    {
        Dictionary<string, string[]> errors = [];
        if (!spendingGoalRepository.TryGetByFundAndAccountingPeriod(getGoalModel.FundId, getGoalModel.AccountingPeriodId, out SpendingGoal? goal))
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
    /// Retrieves the Spending Goals that match the specified criteria
    /// </summary>
    [HttpGet("spending/many")]
    [ProducesResponseType(typeof(CollectionModel<SpendingGoalModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetManySpending([FromQuery] SpendingGoalQueryParameterModel queryParameters)
    {
        if (!spendingGoalGetter.TryGet(queryParameters, out CollectionModel<SpendingGoalModel>? goals, out Dictionary<string, string[]> errors))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Goals.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }

        return Ok(goals);
    }

    /// <summary>
    /// Retrieves current snapshot data for Goals.
    /// </summary>
    [HttpGet("current")]
    [ProducesResponseType(typeof(CurrentGoalsModel), StatusCodes.Status200OK)]
    public IActionResult GetCurrent([FromQuery] CurrentGoalsQueryParameterModel queryParameters) =>
        Ok(currentGoalsGetter.Get(queryParameters));

    /// <summary>
    /// Retrieves the Goal trends that matches the specified criteria.
    /// </summary>
    [HttpGet("trends")]
    [ProducesResponseType(typeof(GoalTrendsModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetTrends([FromQuery] GoalTrendsQueryParameterModel queryParameters)
    {
        if (!goalTrendsGetter.TryGet(queryParameters, out GoalTrendsModel? trends, out Dictionary<string, string[]> errors))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Goal trends.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }

        return Ok(trends);
    }

    /// <summary>
    /// Updates the provided Assignment Goal with the provided properties
    /// </summary>
    [HttpPost("assignment/{goalId}")]
    [ProducesResponseType(typeof(AssignmentGoalModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateAssignmentAsync(Guid goalId, UpdateAssignmentGoalModel updateGoalModel)
    {
        Dictionary<string, string[]> errors = [];
        if (!assignmentGoalRepository.TryGetById(goalId, out AssignmentGoal? goalToUpdate))
        {
            errors.Add(nameof(goalId), [$"Goal with ID {goalId} was not found."]);
        }
        if (errors.Count > 0 || goalToUpdate == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Assignment Goal.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        if (!GoalTypeConverter.TryToDomain(updateGoalModel.AssignmentGoalType, out AssignmentGoalType? assignmentGoalType))
        {
            AddError(errors, nameof(updateGoalModel.AssignmentGoalType), $"Unrecognized Assignment Goal Type: {updateGoalModel.AssignmentGoalType}");
        }
        if (errors.Count > 0 || assignmentGoalType == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Assignment Goal.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }

        if (!assignmentGoalService.TryUpdate(goalToUpdate, assignmentGoalType.Value, updateGoalModel.GoalAmount, out IEnumerable<Exception> assignmentGoalExceptions))
        {
            AddErrors(errors, assignmentGoalExceptions, nameof(updateGoalModel.AssignmentGoalType), nameof(updateGoalModel.GoalAmount));
        }
        if (errors.Count > 0)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Assignment Goal.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }

        await unitOfWork.SaveChangesAsync();
        return Ok(goalConverter.ToModel(goalToUpdate));
    }

    /// <summary>
    /// Updates the provided Spending Goal with the provided properties
    /// </summary>
    [HttpPost("spending/{goalId}")]
    [ProducesResponseType(typeof(SpendingGoalModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateSpendingAsync(Guid goalId, UpdateSpendingGoalModel updateGoalModel)
    {
        Dictionary<string, string[]> errors = [];
        if (!spendingGoalRepository.TryGetById(goalId, out SpendingGoal? goalToUpdate))
        {
            errors.Add(nameof(goalId), [$"Goal with ID {goalId} was not found."]);
        }
        if (errors.Count > 0 || goalToUpdate == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Spending Goal.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        if (!GoalTypeConverter.TryToDomain(updateGoalModel.SpendingGoalType, out SpendingGoalType? spendingGoalType))
        {
            AddError(errors, nameof(updateGoalModel.SpendingGoalType), $"Unrecognized Spending Goal Type: {updateGoalModel.SpendingGoalType}");
        }
        if (errors.Count > 0 || spendingGoalType == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Spending Goal.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }

        if (!spendingGoalService.TryUpdate(goalToUpdate, spendingGoalType.Value, out IEnumerable<Exception> spendingGoalExceptions))
        {
            AddErrors(errors, spendingGoalExceptions, nameof(updateGoalModel.SpendingGoalType), null);
        }
        if (errors.Count > 0)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Spending Goal.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }

        await unitOfWork.SaveChangesAsync();
        return Ok(goalConverter.ToModel(goalToUpdate));
    }

    /// <summary>
    /// Maps goal exceptions to validation errors
    /// </summary>
    private static void AddErrors(
        Dictionary<string, string[]> errors,
        IEnumerable<Exception> exceptions,
        string goalTypeKey,
        string? goalAmountKey)
    {
        foreach (IGrouping<string, Exception> grouping in exceptions.GroupBy(exception => exception switch
        {
            InvalidGoalTypeException => goalTypeKey,
            InvalidFundException invalidFundException
                when goalAmountKey != null && invalidFundException.Message.Contains("goal amount", StringComparison.InvariantCultureIgnoreCase) => goalAmountKey,
            _ => string.Empty,
        }))
        {
            foreach (Exception exception in grouping)
            {
                AddError(errors, grouping.Key, exception.Message);
            }
        }
    }

    private static void AddError(Dictionary<string, string[]> errors, string key, string message)
    {
        if (errors.TryGetValue(key, out string[]? existingMessages))
        {
            errors[key] = [.. existingMessages, message];
            return;
        }

        errors[key] = [message];
    }
}