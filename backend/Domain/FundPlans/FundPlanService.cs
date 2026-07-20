using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Validation;

namespace Domain.FundPlans;

/// <summary>
/// Service for managing Fund Plans.
/// </summary>
public sealed class FundPlanService(
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository,
    IFundPlanRepository fundPlanRepository)
{
    /// <summary>
    /// Attempts to create a Fund Plan.
    /// </summary>
    public bool TryCreate(
        CreateFundPlanRequest request,
        [NotNullWhen(true)] out FundPlan? fundPlan,
        out IEnumerable<ValidationError> exceptions)
    {
        fundPlan = null;

        _ = Validate(
            request.RegularContribution,
            request.MinimumFundedBalance,
            request.MaximumFundedBalance,
            request.TargetEndingBalance,
            out exceptions);
        if (request.Fund.IsUnassignedFund)
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateFundPlanRequest.Fund)),
                "The unassigned fund cannot have a fund plan."));
        }
        if (fundPlanRepository.GetByFundAndAccountingPeriod(request.Fund.Id, request.AccountingPeriod?.Id) != null)
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateFundPlanRequest.Fund)),
                "A Fund Plan already exists for this Fund and Accounting Period."));
        }
        if (exceptions.Any())
        {
            return false;
        }

        fundPlan = new FundPlan(
            request.Fund,
            request.AccountingPeriod,
            request.RegularContribution,
            request.MinimumFundedBalance,
            request.MaximumFundedBalance,
            request.TargetEndingBalance);
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Fund Plan.
    /// </summary>
    public static bool TryUpdate(
        FundPlan fundPlan,
        UpdateFundPlanRequest request,
        out IEnumerable<ValidationError> exceptions)
    {
        _ = Validate(
            request.RegularContribution,
            request.MinimumFundedBalance,
            request.MaximumFundedBalance,
            request.TargetEndingBalance,
            out exceptions);
        if (fundPlan.AccountingPeriod is { IsOpen: false })
        {
            exceptions = exceptions.Append(new ValidationError(
                ValidationErrorPath.Empty,
                "A Fund Plan for a closed Accounting Period cannot be changed."));
        }
        if (exceptions.Any())
        {
            return false;
        }

        fundPlan.Update(
            request.RegularContribution,
            request.MinimumFundedBalance,
            request.MaximumFundedBalance,
            request.TargetEndingBalance);
        return true;
    }

    /// <summary>
    /// Copies Fund Plans from the previous Accounting Period, or onboarded plans for the first Accounting Period.
    /// </summary>
    public void CopyToAccountingPeriod(AccountingPeriod? previousAccountingPeriod, AccountingPeriod accountingPeriod)
    {
        foreach (FundPlan existingPlan in fundPlanRepository.GetAllByAccountingPeriod(previousAccountingPeriod?.Id))
        {
            var copiedPlan = new FundPlan(
                existingPlan.Fund,
                accountingPeriod,
                existingPlan.RegularContribution,
                existingPlan.MinimumFundedBalance,
                existingPlan.MaximumFundedBalance,
                existingPlan.TargetEndingBalance);
            if (!fundPlanRepository.TryAdd(copiedPlan))
            {
                throw new InvalidOperationException("A Fund Plan already exists for the Fund and Accounting Period.");
            }
        }
    }

    /// <summary>
    /// Deletes all Fund Plans associated with an Accounting Period.
    /// </summary>
    public void DeleteForAccountingPeriod(AccountingPeriod accountingPeriod)
    {
        foreach (FundPlan fundPlan in fundPlanRepository.GetAllByAccountingPeriod(accountingPeriod.Id))
        {
            fundPlanRepository.Delete(fundPlan);
        }
    }

    /// <summary>
    /// Calculates progress for a Fund Plan in the provided Accounting Period.
    /// </summary>
    public bool TryGetProgress(
        FundPlan fundPlan,
        AccountingPeriod accountingPeriod,
        [NotNullWhen(true)] out FundPlanProgress? progress,
        out IEnumerable<ValidationError> exceptions)
    {
        progress = null;
        exceptions = [];
        if (fundPlan.AccountingPeriod?.Id != accountingPeriod.Id)
        {
            exceptions = [new ValidationError(
                new ValidationErrorPath(nameof(accountingPeriod)),
                "The Fund Plan does not apply to the provided Accounting Period.")];
            return false;
        }
        AccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository
            .GetForAccountingPeriod(accountingPeriod.Id);
        AccountingPeriodFundBalanceHistory? fundBalanceHistory = balanceHistory.FundBalances.SingleOrDefault(
            balance => balance.Fund.Id == fundPlan.Fund.Id);
        AccountingPeriodFundPlanTotals? totals = balanceHistory.FundPlanTotals.SingleOrDefault(
            totals => totals.Fund.Id == fundPlan.Fund.Id);
        if (fundBalanceHistory == null || totals == null)
        {
            exceptions = [new ValidationError(
                new ValidationErrorPath(nameof(accountingPeriod)),
                "The Fund Plan does not apply to the provided Accounting Period.")];
            return false;
        }

        progress = FundPlanProgressService.Calculate(
            fundBalanceHistory.OpeningBalance,
            totals.AmountAssigned,
            fundBalanceHistory.ClosingBalance,
            fundPlan.RegularContribution,
            fundPlan.MinimumFundedBalance,
            fundPlan.MaximumFundedBalance,
            fundPlan.TargetEndingBalance);
        return true;
    }

    /// <summary>
    /// Validates configurable Fund Plan quantities.
    /// </summary>
    private static bool Validate(
        decimal? regularContribution,
        decimal? minimumFundedBalance,
        decimal? maximumFundedBalance,
        decimal? targetEndingBalance,
        out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        exceptions = exceptions
            .Concat(ValidateNonnegative(regularContribution, nameof(UpdateFundPlanRequest.RegularContribution)))
            .Concat(ValidateNonnegative(minimumFundedBalance, nameof(UpdateFundPlanRequest.MinimumFundedBalance)))
            .Concat(ValidateNonnegative(maximumFundedBalance, nameof(UpdateFundPlanRequest.MaximumFundedBalance)))
            .Concat(ValidateNonnegative(targetEndingBalance, nameof(UpdateFundPlanRequest.TargetEndingBalance)));
        if (minimumFundedBalance > maximumFundedBalance)
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(UpdateFundPlanRequest.MinimumFundedBalance)),
                "Minimum funded balance must be less than or equal to maximum funded balance."));
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(UpdateFundPlanRequest.MaximumFundedBalance)),
                "Maximum funded balance must be greater than or equal to minimum funded balance."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates an optional Fund Plan quantity.
    /// </summary>
    private static IEnumerable<ValidationError> ValidateNonnegative(decimal? value, string propertyName) =>
        value < 0
            ? [new ValidationError(
                new ValidationErrorPath(propertyName),
                "Fund plan quantities must be greater than or equal to zero.")]
            : [];
}