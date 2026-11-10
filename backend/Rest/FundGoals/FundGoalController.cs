using Data;
using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Domain.BalanceEvents;
using Domain.FundGoals;
using Domain.FundGoals.Queries;
using Domain.Validation;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.AccountingPeriods;
using Models.BalanceEvents;
using Models.FundGoals;
using Rest.AccountingPeriods;

namespace Rest.FundGoals;

/// <summary>
/// Controller exposing Fund Goal configuration, progress, and availability.
/// </summary>
[ApiController]
[Route("/fund-goals")]
public sealed class FundGoalController(
    UnitOfWork unitOfWork,
    AccountingPeriodQueryService accountingPeriodQueryService,
    IFundGoalRepository fundGoalRepository,
    FundGoalQueryService fundGoalQueryService,
    FundGoalConverter fundGoalConverter,
    FundGoalService fundGoalService,
    FundGoalBalanceEventQueryService fundGoalBalanceEventQueryService,
    FundGoalBalanceEventConverter fundGoalBalanceEventConverter) : ControllerBase
{
    /// <summary>
    /// Retrieves posted credit and debit totals for all matching Fund Goal events.
    /// </summary>
    [HttpGet("balance-events/accounting-period-range/totals")]
    [ProducesResponseType(typeof(BalanceEventTotalsModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<BalanceEventTotalsModel>> GetBalanceEventTotalsAsync(
        [FromQuery] FundGoalBalanceEventsInAccountingPeriodRangeQueryParameterModel query,
        CancellationToken cancellationToken)
    {
        AccountingPeriodRangeQueryResult<IReadOnlyCollection<FundGoalBalanceEvent>> result = await fundGoalBalanceEventQueryService.GetForTotalsAsync(
            fundGoalBalanceEventConverter.ToDomain(query), cancellationToken);
        if (result.Range == null)
        {
            return UnprocessableEntity(AccountingPeriodRangeValidationProblem.Create(result.Failure,
                query.Range.Start, query.Range.End, "Unable to retrieve Fund Goal balance event totals."));
        }
        var totals = BalanceEventTotals.Calculate(result.Range,
            item => item.IsPosted, item => item.Type, item => item.Amount);
        return Ok(new BalanceEventTotalsModel { TotalInflow = totals.TotalInflow, TotalOutflow = totals.TotalOutflow });
    }

    /// <summary>
    /// Retrieves Fund Goals matching the provided query.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<CollectionModel<FundGoalModel>>> GetAsync(
        [FromQuery] FundGoalQueryParameterModel query,
        CancellationToken cancellationToken) =>
        Ok(fundGoalConverter.ToModel(await fundGoalQueryService.GetAsync(
            fundGoalConverter.ToDomain(query),
            cancellationToken)));

    /// <summary>
    /// Retrieves the Fund Goals in the given Accounting Periods with their progress.
    /// </summary>
    [HttpGet("progress")]
    public async Task<ActionResult<IReadOnlyCollection<FundGoalProgressResultModel>>> GetProgressesAsync(
        [FromQuery] IReadOnlyCollection<Guid> accountingPeriodIds,
        CancellationToken cancellationToken)
    {
        var results = new List<FundGoalProgressResultModel>();
        foreach (Guid accountingPeriodId in accountingPeriodIds.Distinct())
        {
            AccountingPeriod? accountingPeriod = await accountingPeriodQueryService.GetByIdAsync(
                accountingPeriodId,
                cancellationToken);
            if (accountingPeriod == null)
            {
                return NotFound();
            }
            IReadOnlyCollection<FundGoal> fundGoals = fundGoalRepository.GetAllByAccountingPeriod(accountingPeriod.Id);
            IReadOnlyDictionary<FundGoalId, FundGoalProgress> progressByFundGoalId = fundGoalService.GetProgresses(
                fundGoals,
                accountingPeriod);
            results.AddRange(fundGoals
                .Where(fundGoal => progressByFundGoalId.ContainsKey(fundGoal.Id))
                .Select(fundGoal => new FundGoalProgressResultModel
                {
                    FundGoal = fundGoalConverter.ToModel(fundGoal),
                    Progress = fundGoalConverter.ToModel(progressByFundGoalId[fundGoal.Id]),
                }));
        }
        return Ok(results.OrderBy(result => result.FundGoal.Fund.Name).ThenBy(result => result.FundGoal.Id).ToList());
    }

    /// <summary>
    /// Retrieves a Fund's goals and their progress across an Accounting Period range, oldest first.
    /// </summary>
    [HttpGet("fund/{fundId:guid}/history")]
    [ProducesResponseType(typeof(IReadOnlyCollection<FundGoalPeriodProgressModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<IReadOnlyCollection<FundGoalPeriodProgressModel>>> GetHistoryAsync(
        Guid fundId,
        [FromQuery] AccountingPeriodsInRangeQueryParameterModel query,
        CancellationToken cancellationToken)
    {
        AccountingPeriodRangeQueryResult<IReadOnlyCollection<FundGoalPeriodProgress>> result = await fundGoalQueryService.GetHistoryAsync(
            fundId,
            query.Range.Start,
            query.Range.End,
            cancellationToken);
        return result.Range == null
            ? UnprocessableEntity(AccountingPeriodRangeValidationProblem.Create(result.Failure, query.Range.Start, query.Range.End, "Unable to retrieve Fund Goal history."))
            : Ok(result.Range.Select(item => new FundGoalPeriodProgressModel
            {
                Goal = fundGoalConverter.ToModel(item.Goal),
                Progress = fundGoalConverter.ToModel(item.Progress),
            }).ToList());
    }

    /// <summary>
    /// Updates Fund Goal configuration for its associated open Accounting Period or onboarding state.
    /// </summary>
    [HttpPost("{fundGoalId:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid fundGoalId, UpdateFundGoalModel model, CancellationToken cancellationToken)
    {
        if (!fundGoalRepository.TryGetById(fundGoalId, out FundGoal? fundGoal))
        {
            return NotFound();
        }
        if (!FundGoalService.TryUpdate(fundGoal, ToRequest(model), out IEnumerable<ValidationError> errors))
        {
            return ValidationProblem("Unable to update Fund Goal.", errors);
        }
        await unitOfWork.SaveChangesAsync();
        FundGoal? updatedFundGoal = await fundGoalQueryService.GetByIdAsync(fundGoalId, cancellationToken);
        return Ok(fundGoalConverter.ToModel(updatedFundGoal!));
    }

    /// <summary>
    /// Maps the provided Update Fund Goal Model to an Update Fund Goal Request.
    /// </summary>
    private static UpdateFundGoalRequest ToRequest(UpdateFundGoalModel model) => new()
    {
        PlannedMonthlyContribution = model.PlannedMonthlyContribution,
        MinimumEndingBalance = model.MinimumEndingBalance,
        MaximumEndingBalance = model.MaximumEndingBalance,
        AllowExpectedContributionAboveMaximum = model.AllowExpectedContributionAboveMaximum,
    };

    /// <summary>
    /// Returns a Validation Problem response with the provided title and validation errors.
    /// </summary>
    private UnprocessableEntityObjectResult ValidationProblem(string title, IEnumerable<ValidationError> errors) =>
        UnprocessableEntity(new ValidationProblemDetails
        {
            Title = title,
            Errors = ValidationErrorHelper.GroupValidationErrors(errors),
            Status = StatusCodes.Status422UnprocessableEntity,
        });

}
