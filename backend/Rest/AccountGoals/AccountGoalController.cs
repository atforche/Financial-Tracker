using Data;
using Domain.AccountGoals;
using Domain.AccountGoals.Queries;
using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Domain.Validation;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.AccountGoals;

namespace Rest.AccountGoals;

/// <summary>
/// Controller exposing Account Goal configuration and progress.
/// </summary>
[ApiController]
[Route("/account-goals")]
public sealed class AccountGoalController(
    UnitOfWork unitOfWork,
    AccountingPeriodQueryService accountingPeriodQueryService,
    IAccountGoalRepository accountGoalRepository,
    AccountGoalQueryService accountGoalQueryService,
    AccountGoalConverter accountGoalConverter,
    AccountGoalService accountGoalService) : ControllerBase
{
    /// <summary>
    /// Retrieves Account Goals matching the provided query.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<CollectionModel<AccountGoalModel>>> GetAsync(
        [FromQuery] AccountGoalQueryParameterModel query,
        CancellationToken cancellationToken) =>
        Ok(accountGoalConverter.ToModel(await accountGoalQueryService.GetAsync(
            accountGoalConverter.ToDomain(query),
            cancellationToken)));

    /// <summary>
    /// Retrieves progress for all Account Goals in an Accounting Period.
    /// </summary>
    [HttpGet("progress/{accountingPeriodId:guid}")]
    public async Task<ActionResult<IReadOnlyCollection<AccountGoalProgressResultModel>>> GetProgressesAsync(
        Guid accountingPeriodId,
        CancellationToken cancellationToken)
    {
        AccountingPeriod? accountingPeriod = await accountingPeriodQueryService.GetByIdAsync(
            accountingPeriodId,
            cancellationToken);
        if (accountingPeriod == null)
        {
            return NotFound();
        }

        IReadOnlyCollection<AccountGoal> accountGoals = accountGoalRepository.GetAllByAccountingPeriod(accountingPeriod.Id);
        IReadOnlyDictionary<AccountGoalId, AccountGoalProgress> progressByAccountGoalId = accountGoalService.GetProgresses(
            accountGoals,
            accountingPeriod);
        return Ok(accountGoals
            .Where(accountGoal => progressByAccountGoalId.ContainsKey(accountGoal.Id))
            .Select(accountGoal => new AccountGoalProgressResultModel
            {
                AccountGoalId = accountGoal.Id.Value,
                Progress = accountGoalConverter.ToModel(progressByAccountGoalId[accountGoal.Id]),
            })
            .ToList());
    }

    /// <summary>
    /// Retrieves an Account Goal by ID.
    /// </summary>
    [HttpGet("{accountGoalId:guid}")]
    public async Task<ActionResult<AccountGoalModel>> GetAsync(Guid accountGoalId, CancellationToken cancellationToken)
    {
        AccountGoal? accountGoal = await accountGoalQueryService.GetByIdAsync(accountGoalId, cancellationToken);
        return accountGoal == null ? NotFound() : Ok(accountGoalConverter.ToModel(accountGoal));
    }

    /// <summary>
    /// Retrieves the Account Goal associated with an Account and Accounting Period, or its onboarding goal when the period ID is null.
    /// </summary>
    [HttpGet("account/{accountId:guid}")]
    public async Task<ActionResult<AccountGoalModel>> GetByAccountAsync(
        Guid accountId,
        [FromQuery] Guid? accountingPeriodId,
        CancellationToken cancellationToken)
    {
        AccountGoal? accountGoal = await accountGoalQueryService.GetByAccountAndAccountingPeriodAsync(
            accountId,
            accountingPeriodId,
            cancellationToken);
        return accountGoal == null ? NotFound() : Ok(accountGoalConverter.ToModel(accountGoal));
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
    /// Retrieves Account Goal progress for an Accounting Period.
    /// </summary>
    [HttpGet("{accountGoalId:guid}/progress/{accountingPeriodId:guid}")]
    public async Task<ActionResult<AccountGoalProgressModel>> GetProgressAsync(
        Guid accountGoalId,
        Guid accountingPeriodId,
        CancellationToken cancellationToken)
    {
        AccountGoal? accountGoal = await accountGoalQueryService.GetByIdAsync(accountGoalId, cancellationToken);
        AccountingPeriod? accountingPeriod = await accountingPeriodQueryService.GetByIdAsync(
            accountingPeriodId,
            cancellationToken);
        if (accountGoal == null || accountingPeriod == null)
        {
            return NotFound();
        }

        return accountGoalService.TryGetProgress(
            accountGoal,
            accountingPeriod,
            out AccountGoalProgress? progress,
            out IEnumerable<ValidationError> errors)
            ? Ok(accountGoalConverter.ToModel(progress))
            : ValidationProblem("Unable to calculate Account Goal progress.", errors);
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
