using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Validation;

namespace Domain.FundGoals;

/// <summary>
/// Service for managing Fund Goals.
/// </summary>
public sealed class FundGoalService(
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository,
    IFundGoalRepository fundGoalRepository)
{
    /// <summary>
    /// Attempts to create a Fund Goal.
    /// </summary>
    public bool TryCreate(
        CreateFundGoalRequest request,
        [NotNullWhen(true)] out FundGoal? fundGoal,
        out IEnumerable<ValidationError> exceptions)
    {
        fundGoal = null;

        _ = Validate(
            request.RegularContribution,
            request.MinimumFundedBalance,
            request.MaximumFundedBalance,
            request.TargetEndingBalance,
            out exceptions);
        if (request.Fund.IsUnassignedFund)
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateFundGoalRequest.Fund)),
                "The unassigned fund cannot have a fund goal."));
        }
        if (fundGoalRepository.GetByFundAndAccountingPeriod(request.Fund.Id, request.AccountingPeriod?.Id) != null)
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateFundGoalRequest.Fund)),
                "A Fund Goal already exists for this Fund and Accounting Period."));
        }
        if (exceptions.Any())
        {
            return false;
        }

        fundGoal = new FundGoal(
            request.Fund,
            request.AccountingPeriod,
            request.RegularContribution,
            request.MinimumFundedBalance,
            request.MaximumFundedBalance,
            request.TargetEndingBalance);
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Fund Goal.
    /// </summary>
    public static bool TryUpdate(
        FundGoal fundGoal,
        UpdateFundGoalRequest request,
        out IEnumerable<ValidationError> exceptions)
    {
        _ = Validate(
            request.RegularContribution,
            request.MinimumFundedBalance,
            request.MaximumFundedBalance,
            request.TargetEndingBalance,
            out exceptions);
        if (fundGoal.AccountingPeriod is { IsOpen: false })
        {
            exceptions = exceptions.Append(new ValidationError(
                ValidationErrorPath.Empty,
                "A Fund Goal for a closed Accounting Period cannot be changed."));
        }
        if (exceptions.Any())
        {
            return false;
        }

        fundGoal.Update(
            request.RegularContribution,
            request.MinimumFundedBalance,
            request.MaximumFundedBalance,
            request.TargetEndingBalance);
        return true;
    }

    /// <summary>
    /// Copies Fund Goals from the previous Accounting Period, or onboarded fundGoals for the first Accounting Period.
    /// </summary>
    public void CopyToAccountingPeriod(AccountingPeriod? previousAccountingPeriod, AccountingPeriod accountingPeriod)
    {
        foreach (FundGoal existingGoal in fundGoalRepository.GetAllByAccountingPeriod(previousAccountingPeriod?.Id))
        {
            var copiedGoal = new FundGoal(
                existingGoal.Fund,
                accountingPeriod,
                existingGoal.RegularContribution,
                existingGoal.MinimumFundedBalance,
                existingGoal.MaximumFundedBalance,
                existingGoal.TargetEndingBalance);
            if (!fundGoalRepository.TryAdd(copiedGoal))
            {
                throw new InvalidOperationException("A Fund Goal already exists for the Fund and Accounting Period.");
            }
        }
    }

    /// <summary>
    /// Deletes all Fund Goals associated with an Accounting Period.
    /// </summary>
    public void DeleteForAccountingPeriod(AccountingPeriod accountingPeriod)
    {
        foreach (FundGoal fundGoal in fundGoalRepository.GetAllByAccountingPeriod(accountingPeriod.Id))
        {
            fundGoalRepository.Delete(fundGoal);
        }
    }

    /// <summary>
    /// Calculates progress for a Fund Goal in the provided Accounting Period.
    /// </summary>
    public bool TryGetProgress(
        FundGoal fundGoal,
        AccountingPeriod accountingPeriod,
        [NotNullWhen(true)] out FundGoalProgress? progress,
        out IEnumerable<ValidationError> exceptions)
    {
        progress = null;
        exceptions = [];
        if (fundGoal.AccountingPeriod?.Id != accountingPeriod.Id)
        {
            exceptions = [new ValidationError(
                new ValidationErrorPath(nameof(accountingPeriod)),
                "The Fund Goal does not apply to the provided Accounting Period.")];
            return false;
        }
        AccountingPeriodBalanceHistory balanceHistory = accountingPeriodBalanceHistoryRepository
            .GetForAccountingPeriod(accountingPeriod.Id);
        AccountingPeriodFundBalanceHistory? fundBalanceHistory = balanceHistory.FundBalances.SingleOrDefault(
            balance => balance.Fund.Id == fundGoal.Fund.Id);
        AccountingPeriodFundGoalTotals? totals = balanceHistory.FundGoalTotals.SingleOrDefault(
            totals => totals.Fund.Id == fundGoal.Fund.Id);
        if (fundBalanceHistory == null || totals == null)
        {
            exceptions = [new ValidationError(
                new ValidationErrorPath(nameof(accountingPeriod)),
                "The Fund Goal does not apply to the provided Accounting Period.")];
            return false;
        }

        progress = FundGoalProgressService.Calculate(
            fundBalanceHistory.OpeningBalance,
            totals.AmountAssigned,
            fundBalanceHistory.ClosingBalance,
            fundGoal.RegularContribution,
            fundGoal.MinimumFundedBalance,
            fundGoal.MaximumFundedBalance,
            fundGoal.TargetEndingBalance);
        return true;
    }

    /// <summary>
    /// Validates configurable Fund Goal quantities.
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
            .Concat(ValidateNonnegative(regularContribution, nameof(UpdateFundGoalRequest.RegularContribution)))
            .Concat(ValidateNonnegative(minimumFundedBalance, nameof(UpdateFundGoalRequest.MinimumFundedBalance)))
            .Concat(ValidateNonnegative(maximumFundedBalance, nameof(UpdateFundGoalRequest.MaximumFundedBalance)))
            .Concat(ValidateNonnegative(targetEndingBalance, nameof(UpdateFundGoalRequest.TargetEndingBalance)));
        if (minimumFundedBalance > maximumFundedBalance)
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(UpdateFundGoalRequest.MinimumFundedBalance)),
                "Minimum funded balance must be less than or equal to maximum funded balance."));
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(UpdateFundGoalRequest.MaximumFundedBalance)),
                "Maximum funded balance must be greater than or equal to minimum funded balance."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates an optional Fund Goal quantity.
    /// </summary>
    private static IEnumerable<ValidationError> ValidateNonnegative(decimal? value, string propertyName) =>
        value < 0
            ? [new ValidationError(
                new ValidationErrorPath(propertyName),
                "Fund fundGoal quantities must be greater than or equal to zero.")]
            : [];
}