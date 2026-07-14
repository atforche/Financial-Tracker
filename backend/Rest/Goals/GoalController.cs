using Data;
using Data.Goals;
using Domain.Goals;
using Domain.Validation;
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
    AssignmentGoalRepository assignmentGoalRepository,
    AssignmentGoalService assignmentGoalService,
    SpendingGoalRepository spendingGoalRepository,
    SpendingGoalService spendingGoalService,
    GoalQueryService goalQueryService) : ControllerBase
{
    /// <summary>
    /// Retrieves Assignment Goals matching the specified criteria.
    /// </summary>
    [HttpGet("assignment")]
    [ProducesResponseType(typeof(CollectionModel<AssignmentGoalModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionModel<AssignmentGoalModel>>> GetAssignmentGoalsAsync(
        [FromQuery] AssignmentGoalQueryParameterModel query,
        CancellationToken cancellationToken) =>
        Ok(await goalQueryService.GetAssignmentGoalsAsync(query, cancellationToken));

    /// <summary>
    /// Retrieves an Assignment Goal by ID.
    /// </summary>
    [HttpGet("assignment/{goalId:guid}")]
    [ProducesResponseType(typeof(AssignmentGoalModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssignmentGoalModel>> GetAssignmentGoalAsync(Guid goalId, CancellationToken cancellationToken)
    {
        AssignmentGoalModel? model = await goalQueryService.GetAssignmentGoalByIdAsync(goalId, cancellationToken);
        return model == null ? NotFound() : Ok(model);
    }

    /// <summary>
    /// Retrieves Spending Goals matching the specified criteria.
    /// </summary>
    [HttpGet("spending")]
    [ProducesResponseType(typeof(CollectionModel<SpendingGoalModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionModel<SpendingGoalModel>>> GetSpendingGoalsAsync(
        [FromQuery] SpendingGoalQueryParameterModel query,
        CancellationToken cancellationToken) =>
        Ok(await goalQueryService.GetSpendingGoalsAsync(query, cancellationToken));

    /// <summary>
    /// Retrieves a Spending Goal by ID.
    /// </summary>
    [HttpGet("spending/{goalId:guid}")]
    [ProducesResponseType(typeof(SpendingGoalModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SpendingGoalModel>> GetSpendingGoalAsync(Guid goalId, CancellationToken cancellationToken)
    {
        SpendingGoalModel? model = await goalQueryService.GetSpendingGoalByIdAsync(goalId, cancellationToken);
        return model == null ? NotFound() : Ok(model);
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

        if (!assignmentGoalService.TryUpdate(
            goalToUpdate,
            new UpdateAssignmentGoalRequest
            {
                AssignmentGoalType = assignmentGoalType.Value,
                GoalAmount = updateGoalModel.GoalAmount,
            },
            out IEnumerable<ValidationError> assignmentGoalErrors))
        {
            AddValidationErrors(errors, assignmentGoalErrors);
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
        return Ok(await goalQueryService.GetAssignmentGoalByIdAsync(goalId));
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

        if (!spendingGoalService.TryUpdate(
            goalToUpdate,
            new UpdateSpendingGoalRequest { SpendingGoalType = spendingGoalType.Value },
            out IEnumerable<ValidationError> spendingGoalErrors))
        {
            AddValidationErrors(errors, spendingGoalErrors);
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
        return Ok(await goalQueryService.GetSpendingGoalByIdAsync(goalId));
    }

    /// <summary>
    /// Adds structured validation errors to an HTTP validation response.
    /// </summary>
    private static void AddValidationErrors(
        Dictionary<string, string[]> errors,
        IEnumerable<ValidationError> validationErrors)
    {
        foreach (IGrouping<ValidationErrorPath, ValidationError> grouping in validationErrors.GroupBy(error => error.Path))
        {
            foreach (ValidationError error in grouping)
            {
                AddError(errors, grouping.Key.ToString(), error.Message);
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