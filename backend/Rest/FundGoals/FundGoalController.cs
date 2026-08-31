using Data;
using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Domain.FundGoals;
using Domain.FundGoals.Queries;
using Domain.Funds;
using Domain.Validation;
using Microsoft.AspNetCore.Mvc;
using Models;
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
    FundBalanceService fundBalanceService,
    FundGoalService fundGoalService,
    FundGoalBalanceEventQueryService fundGoalBalanceEventQueryService,
    FundGoalBalanceEventConverter fundGoalBalanceEventConverter) : ControllerBase
{
    /// <summary>
    /// Retrieves Fund Goal balance events in a date range.
    /// </summary>
    [HttpGet("balance-events/date-range")]
    public async Task<ActionResult<CollectionModel<FundGoalBalanceEventModel>>> GetBalanceEventsAsync(
        [FromQuery] FundGoalBalanceEventsInDateRangeQueryParameterModel query,
        CancellationToken cancellationToken) =>
        Ok(fundGoalBalanceEventConverter.ToModel(await fundGoalBalanceEventQueryService.GetAsync(
            fundGoalBalanceEventConverter.ToDomain(query),
            cancellationToken)));

    /// <summary>
    /// Retrieves Fund Goal balance events in an Accounting Period range.
    /// </summary>
    [HttpGet("balance-events/accounting-period-range")]
    public async Task<ActionResult<CollectionModel<FundGoalBalanceEventModel>>> GetBalanceEventsAsync(
        [FromQuery] FundGoalBalanceEventsInAccountingPeriodRangeQueryParameterModel query,
        CancellationToken cancellationToken)
    {
        FundGoalBalanceEventAccountingPeriodRangeQueryResult result = await fundGoalBalanceEventQueryService.GetAsync(
            fundGoalBalanceEventConverter.ToDomain(query),
            cancellationToken);
        return result.Page == null
            ? UnprocessableEntity(AccountingPeriodRangeValidationProblem.Create(result.Failure, query.Range.Start, query.Range.End, "Unable to retrieve Fund Goal balance events."))
            : Ok(fundGoalBalanceEventConverter.ToModel(result.Page));
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
    /// Retrieves progress for all Fund Goals in an Accounting Period.
    /// </summary>
    [HttpGet("progress/{accountingPeriodId:guid}")]
    public async Task<ActionResult<IReadOnlyCollection<FundGoalProgressResultModel>>> GetProgressesAsync(
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
        IReadOnlyCollection<FundGoal> fundGoals = fundGoalRepository.GetAllByAccountingPeriod(accountingPeriod.Id);
        IReadOnlyDictionary<FundGoalId, FundGoalProgress> progressByFundGoalId = fundGoalService.GetProgresses(
            fundGoals,
            accountingPeriod);
        return Ok(fundGoals
            .Where(fundGoal => progressByFundGoalId.ContainsKey(fundGoal.Id))
            .Select(fundGoal => new FundGoalProgressResultModel
            {
                FundGoalId = fundGoal.Id.Value,
                Progress = ToModel(progressByFundGoalId[fundGoal.Id]),
            })
            .ToList());
    }

    /// <summary>
    /// Retrieves a Fund Goal by ID.
    /// </summary>
    [HttpGet("{fundGoalId:guid}")]
    public async Task<ActionResult<FundGoalModel>> GetAsync(Guid fundGoalId, CancellationToken cancellationToken)
    {
        FundGoal? fundGoal = await fundGoalQueryService.GetByIdAsync(fundGoalId, cancellationToken);
        return fundGoal == null ? NotFound() : Ok(fundGoalConverter.ToModel(fundGoal));
    }

    /// <summary>
    /// Retrieves the Fund Goal associated with a Fund and Accounting Period, or its onboarded Fund Goal when the Accounting Period ID is null.
    /// </summary>
    [HttpGet("fund/{fundId:guid}")]
    public async Task<ActionResult<FundGoalModel>> GetByFundAsync(
        Guid fundId,
        [FromQuery] Guid? accountingPeriodId,
        CancellationToken cancellationToken)
    {
        FundGoal? fundGoal = await fundGoalQueryService.GetByFundAndAccountingPeriodAsync(
            fundId,
            accountingPeriodId,
            cancellationToken);
        return fundGoal == null ? NotFound() : Ok(fundGoalConverter.ToModel(fundGoal));
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
    /// Retrieves Fund Goal progress for an Accounting Period.
    /// </summary>
    [HttpGet("{fundGoalId:guid}/progress/{accountingPeriodId:guid}")]
    public async Task<ActionResult<FundGoalProgressModel>> GetProgress(Guid fundGoalId, Guid accountingPeriodId, CancellationToken cancellationToken)
    {
        FundGoal? fundGoal = await fundGoalQueryService.GetByIdAsync(fundGoalId, cancellationToken);
        AccountingPeriod? accountingPeriod = await accountingPeriodQueryService.GetByIdAsync(accountingPeriodId, cancellationToken);
        if (fundGoal == null || accountingPeriod == null)
        {
            return NotFound();
        }
        return fundGoalService.TryGetProgress(fundGoal, accountingPeriod, out FundGoalProgress? progress, out IEnumerable<ValidationError> errors)
            ? Ok(ToModel(progress))
            : ValidationProblem("Unable to calculate Fund Goal progress.", errors);
    }

    /// <summary>
    /// Retrieves current Fund availability.
    /// </summary>
    [HttpGet("{fundGoalId:guid}/availability")]
    public async Task<ActionResult<FundAvailabilityModel>> GetAvailability(Guid fundGoalId, CancellationToken cancellationToken)
    {
        FundGoal? fundGoal = await fundGoalQueryService.GetByIdAsync(fundGoalId, cancellationToken);
        if (fundGoal == null)
        {
            return NotFound();
        }
        FundBalance balance = fundBalanceService.GetCurrentBalance(fundGoal.Fund.Id);
        var availability = new FundAvailability(balance);
        return Ok(new FundAvailabilityModel
        {
            AvailableBalance = availability.AvailableBalance,
            AvailableBalanceIncludingPending = availability.AvailableBalanceIncludingPending,
            IsOverspent = availability.IsOverspent,
            IsOverspentIncludingPending = availability.IsOverspentIncludingPending,
        });
    }

    /// <summary>
    /// Maps the provided Update Fund Goal Model to an Update Fund Goal Request.
    /// </summary>
    private static UpdateFundGoalRequest ToRequest(UpdateFundGoalModel model) => new()
    {
        RegularContribution = model.RegularContribution,
        MinimumEndingBalance = model.MinimumEndingBalance,
        MaximumEndingBalance = model.MaximumEndingBalance,
    };

    /// <summary>
    /// Maps the provided Fund Goal Progress to a Fund Goal Progress Model.
    /// </summary>
    private static FundGoalProgressModel ToModel(FundGoalProgress progress) => new()
    {
        AvailableBalance = new AvailableBalanceProgressModel
        {
            CurrentBalance = progress.AvailableBalance.CurrentBalance,
            MinimumBalance = progress.AvailableBalance.MinimumBalance,
            Shortfall = progress.AvailableBalance.Shortfall,
            IsSatisfied = progress.AvailableBalance.IsSatisfied,
        },
        Contribution = progress.Contribution == null ? null : new ContributionProgressModel
        {
            TargetAmount = progress.Contribution.TargetAmount,
            AssignedAmount = progress.Contribution.AssignedAmount,
            RemainingAmount = progress.Contribution.RemainingAmount,
            IsSatisfied = progress.Contribution.IsSatisfied,
        },
        EndingBalance = progress.EndingBalance == null ? null : new FundGoalEndingBalanceProgressModel
        {
            CurrentBalance = progress.EndingBalance.CurrentBalance,
            MinimumBalance = progress.EndingBalance.MinimumBalance,
            MaximumBalance = progress.EndingBalance.MaximumBalance,
            AmountBelowMinimum = progress.EndingBalance.AmountBelowMinimum,
            AmountAboveMaximum = progress.EndingBalance.AmountAboveMaximum,
            Status = (FundGoalEndingBalanceStatusModel)progress.EndingBalance.Status,
        },
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
