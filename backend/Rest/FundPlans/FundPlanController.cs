using Data;
using Data.AccountingPeriods;
using Data.FundPlans;
using Domain.AccountingPeriods;
using Domain.FundPlans;
using Domain.Funds;
using Domain.Validation;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.FundPlans;

namespace Rest.FundPlans;

/// <summary>
/// Controller exposing Fund Plan configuration, progress, and availability.
/// </summary>
[ApiController]
[Route("/fund-plans")]
public sealed class FundPlanController(
    UnitOfWork unitOfWork,
    FundPlanRepository fundPlanRepository,
    FundPlanQueryService fundPlanQueryService,
    AccountingPeriodRepository accountingPeriodRepository,
    FundBalanceService fundBalanceService,
    FundPlanService fundPlanService) : ControllerBase
{
    /// <summary>
    /// Retrieves Fund Plans matching the provided query.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<CollectionModel<FundPlanModel>>> GetAsync(
        [FromQuery] FundPlanQueryParameterModel query,
        CancellationToken cancellationToken) =>
        Ok(await fundPlanQueryService.GetAsync(query, cancellationToken));

    /// <summary>
    /// Retrieves a Fund Plan by ID.
    /// </summary>
    [HttpGet("{fundPlanId:guid}")]
    public async Task<ActionResult<FundPlanModel>> GetAsync(Guid fundPlanId, CancellationToken cancellationToken)
    {
        FundPlanModel? model = await fundPlanQueryService.GetByIdAsync(fundPlanId, cancellationToken);
        return model == null ? NotFound() : Ok(model);
    }

    /// <summary>
    /// Retrieves the Fund Plan associated with a Fund and Accounting Period, or its onboarded plan when the Accounting Period ID is null.
    /// </summary>
    [HttpGet("fund/{fundId:guid}")]
    public async Task<ActionResult<FundPlanModel>> GetByFundAsync(
        Guid fundId,
        [FromQuery] Guid? accountingPeriodId,
        CancellationToken cancellationToken)
    {
        FundPlanModel? model = await fundPlanQueryService.GetByFundAndAccountingPeriodAsync(
            fundId,
            accountingPeriodId,
            cancellationToken);
        return model == null ? NotFound() : Ok(model);
    }

    /// <summary>
    /// Updates Fund Plan configuration for its associated open Accounting Period or onboarding state.
    /// </summary>
    [HttpPost("{fundPlanId:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid fundPlanId, UpdateFundPlanModel model, CancellationToken cancellationToken)
    {
        if (!fundPlanRepository.TryGetById(fundPlanId, out FundPlan? fundPlan))
        {
            return NotFound();
        }
        if (!FundPlanService.TryUpdate(fundPlan, ToRequest(model), out IEnumerable<ValidationError> errors))
        {
            return ValidationProblem("Unable to update Fund Plan.", errors);
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(await fundPlanQueryService.GetByIdAsync(fundPlanId, cancellationToken));
    }

    /// <summary>
    /// Retrieves Fund Plan progress for an Accounting Period.
    /// </summary>
    [HttpGet("{fundPlanId:guid}/progress/{accountingPeriodId:guid}")]
    public ActionResult<FundPlanProgressModel> GetProgress(Guid fundPlanId, Guid accountingPeriodId)
    {
        if (!fundPlanRepository.TryGetById(fundPlanId, out FundPlan? fundPlan)
            || !accountingPeriodRepository.TryGetById(accountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            return NotFound();
        }
        return fundPlanService.TryGetProgress(fundPlan, accountingPeriod, out FundPlanProgress? progress, out IEnumerable<ValidationError> errors)
            ? Ok(ToModel(progress))
            : ValidationProblem("Unable to calculate Fund Plan progress.", errors);
    }

    /// <summary>
    /// Retrieves current Fund availability.
    /// </summary>
    [HttpGet("{fundPlanId:guid}/availability")]
    public ActionResult<FundAvailabilityModel> GetAvailability(Guid fundPlanId)
    {
        if (!fundPlanRepository.TryGetById(fundPlanId, out FundPlan? fundPlan))
        {
            return NotFound();
        }
        FundBalance balance = fundBalanceService.GetCurrentBalance(fundPlan.Fund.Id);
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
    /// Maps the provided Update Fund Plan Model to an Update Fund Plan Request.
    /// </summary>
    private static UpdateFundPlanRequest ToRequest(UpdateFundPlanModel model) => new()
    {
        RegularContribution = model.RegularContribution,
        MinimumFundedBalance = model.MinimumFundedBalance,
        MaximumFundedBalance = model.MaximumFundedBalance,
        TargetEndingBalance = model.TargetEndingBalance,
    };

    /// <summary>
    /// Maps the provided Fund Plan Progress to a Fund Plan Progress Model.
    /// </summary>
    private static FundPlanProgressModel ToModel(FundPlanProgress progress) => new()
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
        FundedBalance = progress.FundedBalance == null ? null : new FundedBalanceProgressModel
        {
            Balance = progress.FundedBalance.Balance,
            MinimumBalance = progress.FundedBalance.MinimumBalance,
            MaximumBalance = progress.FundedBalance.MaximumBalance,
            AmountBelowMinimum = progress.FundedBalance.AmountBelowMinimum,
            AmountAboveMaximum = progress.FundedBalance.AmountAboveMaximum,
            Status = (FundedBalanceStatusModel)progress.FundedBalance.Status,
        },
        EndingBalance = progress.EndingBalance == null ? null : new EndingBalanceProgressModel
        {
            TargetBalance = progress.EndingBalance.TargetBalance,
            CurrentBalance = progress.EndingBalance.CurrentBalance,
            Variance = progress.EndingBalance.Variance,
            Status = (EndingBalanceStatusModel)progress.EndingBalance.Status,
            ProjectedEndingBalance = progress.EndingBalance.ProjectedEndingBalance,
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