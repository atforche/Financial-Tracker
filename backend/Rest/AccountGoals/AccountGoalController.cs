using Data;
using Domain.AccountGoals;
using Domain.AccountGoals.Queries;
using Domain.AccountingPeriods.Queries;
using Domain.Validation;
using Microsoft.AspNetCore.Mvc;
using Models.AccountGoals;
using Models.AccountingPeriods;
using Rest.AccountingPeriods;

namespace Rest.AccountGoals;

/// <summary>
/// Controller exposing Account Goal configuration and progress.
/// </summary>
[ApiController]
[Route("/account-goals")]
public sealed class AccountGoalController(
    UnitOfWork unitOfWork,
    IAccountGoalRepository accountGoalRepository,
    AccountGoalQueryService accountGoalQueryService,
    AccountGoalConverter accountGoalConverter) : ControllerBase
{
    /// <summary>
    /// Retrieves a Account's goals and their progress across an Accounting Period range, oldest first.
    /// </summary>
    [HttpGet("account/{accountId:guid}/history")]
    [ProducesResponseType(typeof(IReadOnlyCollection<AccountGoalPeriodProgressModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<IReadOnlyCollection<AccountGoalPeriodProgressModel>>> GetHistoryAsync(
        Guid accountId,
        [FromQuery] AccountingPeriodsInRangeQueryParameterModel query,
        CancellationToken cancellationToken)
    {
        AccountingPeriodRangeQueryResult<IReadOnlyCollection<AccountGoalPeriodProgress>> result = await accountGoalQueryService.GetHistoryAsync(
            accountId,
            query.Range.Start,
            query.Range.End,
            cancellationToken);
        return result.Range == null
            ? UnprocessableEntity(AccountingPeriodRangeValidationProblem.Create(result.Failure, query.Range.Start, query.Range.End, "Unable to retrieve Account Goal history."))
            : Ok(result.Range.Select(item => new AccountGoalPeriodProgressModel
            {
                Goal = accountGoalConverter.ToModel(item.Goal),
                Progress = accountGoalConverter.ToModel(item.Progress),
            }).ToList());
    }

    /// <summary>
    /// Updates Account Goal configuration for its associated open Accounting Period or onboarding state.
    /// </summary>
    [HttpPost("{accountGoalId:guid}")]
    public async Task<IActionResult> UpdateAsync(
        Guid accountGoalId,
        UpdateAccountGoalModel model,
        CancellationToken cancellationToken)
    {
        if (!accountGoalRepository.TryGetById(accountGoalId, out AccountGoal? accountGoal))
        {
            return NotFound();
        }
        if (!AccountGoalService.TryUpdate(accountGoal, new UpdateAccountGoalRequest
        {
            MinimumEndingBalance = model.MinimumEndingBalance,
            MaximumEndingBalance = model.MaximumEndingBalance,
        }, out IEnumerable<ValidationError> errors))
        {
            return ValidationProblem("Unable to update Account Goal.", errors);
        }

        await unitOfWork.SaveChangesAsync();
        AccountGoal? updatedAccountGoal = await accountGoalQueryService.GetByIdAsync(accountGoalId, cancellationToken);
        return Ok(accountGoalConverter.ToModel(updatedAccountGoal!));
    }

    /// <summary>
    /// Returns a Validation Problem response with the provided title.
    /// </summary>
    private UnprocessableEntityObjectResult ValidationProblem(string title, IEnumerable<ValidationError> errors) =>
        UnprocessableEntity(new ValidationProblemDetails
        {
            Title = title,
            Errors = ValidationErrorHelper.GroupValidationErrors(errors),
            Status = StatusCodes.Status422UnprocessableEntity,
        });
}
