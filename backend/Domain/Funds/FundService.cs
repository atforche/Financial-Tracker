using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Goals;
using Domain.Transactions;
using Domain.Validation;

namespace Domain.Funds;

/// <summary>
/// Service for managing Funds
/// </summary>
public class FundService(
    IFundRepository fundRepository,
    IAccountingPeriodRepository accountingPeriodRepository,
    IAssignmentGoalRepository assignmentGoalRepository,
    ISpendingGoalRepository spendingGoalRepository,
    ITransactionRepository transactionRepository,
    AccountingPeriodBalanceService accountingPeriodBalanceService,
    AssignmentGoalService assignmentGoalService,
    SpendingGoalService spendingGoalService)
{
    /// <summary>
    /// Attempts to create a new Fund
    /// </summary>
    public bool TryCreate(
        CreateFundRequest request,
        [NotNullWhen(true)] out Fund? fund,
        out IEnumerable<ValidationError> exceptions)
    {
        fund = null;

        if (!ValidateCreate(request, out exceptions))
        {
            return false;
        }
        fund = new Fund(request.Name, request.Description, request.OpeningAccountingPeriod.Id);
        accountingPeriodBalanceService.AddFund(fund);

        var assignmentGoals = new List<AssignmentGoal>();
        var spendingGoals = new List<SpendingGoal>();
        if (!TryCreateGoalsForAccountingPeriods(
                fund,
                GetAccountingPeriodsFrom(request.OpeningAccountingPeriod),
                request.AssignmentGoalType,
                request.AssignmentGoalAmount,
                request.SpendingGoalType,
                assignmentGoals,
                spendingGoals,
                out exceptions))
        {
            fund = null;
            return false;
        }

        fundRepository.Add(fund);
        foreach (AssignmentGoal assignmentGoal in assignmentGoals)
        {
            assignmentGoalRepository.Add(assignmentGoal);
        }
        foreach (SpendingGoal spendingGoal in spendingGoals)
        {
            spendingGoalRepository.Add(spendingGoal);
        }
        return true;
    }

    /// <summary>
    /// Attempts to onboard a new Fund.
    /// </summary>
    public bool TryOnboard(
        OnboardFundRequest request,
        [NotNullWhen(true)] out Fund? fund,
        out IEnumerable<ValidationError> exceptions)
    {
        fund = null;

        if (!ValidateOnboard(request, out exceptions))
        {
            return false;
        }
        fund = new Fund(request.Name, request.Description, request.OnboardedBalance);
        if (!assignmentGoalService.TryCreate(
            new CreateAssignmentGoalRequest
            {
                Fund = fund,
                AccountingPeriod = null,
                AssignmentGoalType = request.AssignmentGoalType,
                GoalAmount = request.AssignmentGoalAmount,
            },
            out AssignmentGoal? assignmentGoal,
            out IEnumerable<ValidationError> assignmentGoalExceptions))
        {
            exceptions = assignmentGoalExceptions;
            fund = null;
            return false;
        }
        if (!spendingGoalService.TryCreate(
            new CreateSpendingGoalRequest
            {
                Fund = fund,
                AccountingPeriod = null,
                SpendingGoalType = request.SpendingGoalType,
            },
            out SpendingGoal? spendingGoal,
            out IEnumerable<ValidationError> spendingGoalExceptions))
        {
            exceptions = spendingGoalExceptions;
            fund = null;
            return false;
        }

        fundRepository.Add(fund);
        assignmentGoalRepository.Add(assignmentGoal);
        spendingGoalRepository.Add(spendingGoal);
        if (request.Name != Fund.UnassignedFundName)
        {
            Fund? unassignedFund = fundRepository.GetUnassignedFund() ?? throw new InvalidOperationException();
            unassignedFund.OnboardedBalance -= request.OnboardedBalance;
        }
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Fund
    /// </summary>
    public bool TryUpdate(Fund fund, UpdateFundRequest request, out IEnumerable<ValidationError> exceptions)
    {
        if (!ValidateUpdate(fund, request, out exceptions))
        {
            return false;
        }
        fund.Name = request.Name;
        fund.Description = request.Description;
        return true;
    }

    /// <summary>
    /// Attempts to delete an existing Fund
    /// </summary>
    public bool TryDelete(Fund fund, out IEnumerable<ValidationError> exceptions)
    {
        if (!ValidateDelete(fund, out exceptions))
        {
            return false;
        }
        if (fund.OnboardedBalance != null)
        {
            Fund unassignedFund = fundRepository.GetUnassignedFund() ?? throw new InvalidOperationException();
            unassignedFund.OnboardedBalance += fund.OnboardedBalance.Value;
        }
        accountingPeriodBalanceService.DeleteFund(fund);
        fundRepository.Delete(fund);
        return true;
    }

    /// <summary>
    /// Attempts to create the unassigned Fund
    /// </summary>
    internal bool TryCreateUnassignedFund(
        AccountingPeriod openingAccountingPeriod,
        [NotNullWhen(true)] out Fund? fund,
        out IEnumerable<ValidationError> exceptions)
    {
        fund = null;

        var request = new CreateFundRequest
        {
            Name = Fund.UnassignedFundName,
            Description = Fund.UnassignedFundDescription,
            OpeningAccountingPeriod = openingAccountingPeriod,
            AssignmentGoalType = AssignmentGoalType.MonthlyTarget,
            AssignmentGoalAmount = 1,
            SpendingGoalType = SpendingGoalType.Standard,
        };
        if (!ValidateCreate(request, out exceptions))
        {
            return false;
        }

        fund = new Fund(openingAccountingPeriod.Id);
        accountingPeriodBalanceService.AddFund(fund);
        return true;
    }

    /// <summary>
    /// Attempts to onboard the unassigned Fund
    /// </summary>
    internal bool TryOnboardUnassignedFund(
        decimal onboardedBalance,
        [NotNullWhen(true)] out Fund? fund,
        out IEnumerable<ValidationError> exceptions)
    {
        fund = null;

        var request = new OnboardFundRequest
        {
            Name = Fund.UnassignedFundName,
            Description = Fund.UnassignedFundDescription,
            OnboardedBalance = onboardedBalance,
            AssignmentGoalType = AssignmentGoalType.MonthlyTarget,
            AssignmentGoalAmount = 1,
            SpendingGoalType = SpendingGoalType.Standard,
        };
        if (!ValidateOnboard(request, out exceptions))
        {
            return false;
        }
        fund = new Fund(onboardedBalance);
        fundRepository.Add(fund);
        return true;
    }

    /// <summary>
    /// Validates the name for a Fund
    /// </summary>
    private bool ValidateName(string name, ValidationErrorPath namePath, Fund? existingFund, out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (string.IsNullOrEmpty(name))
        {
            exceptions = exceptions.Append(new ValidationError(namePath, "Fund name cannot be empty"));
        }
        if (fundRepository.TryGetByName(name, out Fund? existingFundWithName) && existingFundWithName != existingFund)
        {
            exceptions = exceptions.Append(new ValidationError(namePath, "Fund name must be unique"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the provided request to create a fund
    /// </summary>
    private bool ValidateCreate(CreateFundRequest request, out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (!ValidateName(request.Name, new ValidationErrorPath(nameof(CreateFundRequest.Name)), null, out IEnumerable<ValidationError> nameExceptions))
        {
            exceptions = exceptions.Concat(nameExceptions);
        }
        if (!request.OpeningAccountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateFundRequest.OpeningAccountingPeriod)),
                "The provided accounting period is closed."));
        }
        if (!ValidateAssignmentGoalType(request.AssignmentGoalType, new ValidationErrorPath(nameof(CreateFundRequest.AssignmentGoalType)), out IEnumerable<ValidationError> assignmentGoalTypeExceptions))
        {
            exceptions = exceptions.Concat(assignmentGoalTypeExceptions);
        }
        if (!ValidateAssignmentGoalAmount(request.AssignmentGoalAmount, new ValidationErrorPath(nameof(CreateFundRequest.AssignmentGoalAmount)), out IEnumerable<ValidationError> assignmentGoalAmountExceptions))
        {
            exceptions = exceptions.Concat(assignmentGoalAmountExceptions);
        }
        if (!ValidateSpendingGoalType(request.SpendingGoalType, new ValidationErrorPath(nameof(CreateFundRequest.SpendingGoalType)), out IEnumerable<ValidationError> spendingGoalTypeExceptions))
        {
            exceptions = exceptions.Concat(spendingGoalTypeExceptions);
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the provided request to onboard a Fund.
    /// </summary>
    private bool ValidateOnboard(OnboardFundRequest request, out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (!ValidateName(request.Name, new ValidationErrorPath(nameof(OnboardFundRequest.Name)), null, out IEnumerable<ValidationError> nameExceptions))
        {
            exceptions = exceptions.Concat(nameExceptions);
        }
        if (accountingPeriodRepository.GetAll().Count > 0)
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "Funds can only be onboarded before any Accounting Periods have been created."));
        }
        if (!ValidateAssignmentGoalType(request.AssignmentGoalType, new ValidationErrorPath(nameof(OnboardFundRequest.AssignmentGoalType)), out IEnumerable<ValidationError> assignmentGoalTypeExceptions))
        {
            exceptions = exceptions.Concat(assignmentGoalTypeExceptions);
        }
        if (!ValidateAssignmentGoalAmount(request.AssignmentGoalAmount, new ValidationErrorPath(nameof(OnboardFundRequest.AssignmentGoalAmount)), out IEnumerable<ValidationError> assignmentGoalAmountExceptions))
        {
            exceptions = exceptions.Concat(assignmentGoalAmountExceptions);
        }
        if (!ValidateSpendingGoalType(request.SpendingGoalType, new ValidationErrorPath(nameof(OnboardFundRequest.SpendingGoalType)), out IEnumerable<ValidationError> spendingGoalTypeExceptions))
        {
            exceptions = exceptions.Concat(spendingGoalTypeExceptions);
        }
        if (request.Name != Fund.UnassignedFundName)
        {
            Fund? unassignedFund = fundRepository.GetUnassignedFund();
            if (unassignedFund == null)
            {
                exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "The unassigned fund must exist before onboarding a Fund."));
            }
            else if (unassignedFund.OnboardedBalance < request.OnboardedBalance)
            {
                exceptions = exceptions.Append(new ValidationError(
                    new ValidationErrorPath(nameof(OnboardFundRequest.OnboardedBalance)),
                    "There is not enough unassigned balance to onboard this Fund."));
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the provided information to update a fund
    /// </summary>
    private bool ValidateUpdate(Fund fund, UpdateFundRequest request, out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (!ValidateName(request.Name, new ValidationErrorPath(nameof(UpdateFundRequest.Name)), fund, out IEnumerable<ValidationError> nameExceptions))
        {
            exceptions = exceptions.Concat(nameExceptions);
        }
        if (fund.IsUnassignedFund)
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "The unassigned fund cannot be updated."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates whether a fund can be deleted
    /// </summary>
    private bool ValidateDelete(Fund fund, out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (fund.IsUnassignedFund)
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "The unassigned fund cannot be deleted."));
        }
        if (fund.IsOnboarded && accountingPeriodRepository.GetLatestAccountingPeriod() != null)
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "Cannot delete an onboarded Fund."));
        }
        if (transactionRepository.DoAnyTransactionsExistForFund(fund.Id))
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "Cannot delete a Fund that has Transactions."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the provided assignment goal type.
    /// </summary>
    private static bool ValidateAssignmentGoalType(AssignmentGoalType assignmentGoalType, ValidationErrorPath assignmentGoalTypePath, out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (!Enum.IsDefined(assignmentGoalType))
        {
            exceptions = exceptions.Append(new ValidationError(assignmentGoalTypePath, "The provided assignment goal type is invalid."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the provided assignment goal amount.
    /// </summary>
    private static bool ValidateAssignmentGoalAmount(decimal assignmentGoalAmount, ValidationErrorPath assignmentGoalAmountPath, out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (assignmentGoalAmount < 0)
        {
            exceptions = exceptions.Append(new ValidationError(assignmentGoalAmountPath, "Goal amount must be greater than or equal to zero."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the provided spending goal type.
    /// </summary>
    private static bool ValidateSpendingGoalType(SpendingGoalType spendingGoalType, ValidationErrorPath spendingGoalTypePath, out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        if (!Enum.IsDefined(spendingGoalType))
        {
            exceptions = exceptions.Append(new ValidationError(spendingGoalTypePath, "The provided spending goal type is invalid."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Gets all accounting periods that fall on or after the provided accounting period.
    /// </summary>
    private IEnumerable<AccountingPeriod> GetAccountingPeriodsFrom(AccountingPeriod openingAccountingPeriod)
    {
        AccountingPeriod? accountingPeriod = openingAccountingPeriod;
        while (accountingPeriod != null)
        {
            yield return accountingPeriod;
            accountingPeriod = accountingPeriodRepository.GetNextAccountingPeriod(accountingPeriod.Id);
        }
    }

    /// <summary>
    /// Attempts to create assignment and spending goals for the provided accounting periods.
    /// </summary>
    private bool TryCreateGoalsForAccountingPeriods(
        Fund fund,
        IEnumerable<AccountingPeriod> accountingPeriods,
        AssignmentGoalType assignmentGoalType,
        decimal assignmentGoalAmount,
        SpendingGoalType spendingGoalType,
        List<AssignmentGoal> assignmentGoals,
        List<SpendingGoal> spendingGoals,
        out IEnumerable<ValidationError> exceptions)
    {
        exceptions = [];

        foreach (AccountingPeriod accountingPeriod in accountingPeriods)
        {
            if (!assignmentGoalService.TryCreate(
                new CreateAssignmentGoalRequest
                {
                    Fund = fund,
                    AccountingPeriod = accountingPeriod,
                    AssignmentGoalType = assignmentGoalType,
                    GoalAmount = assignmentGoalAmount,
                },
                out AssignmentGoal? assignmentGoal,
                out IEnumerable<ValidationError> assignmentGoalExceptions))
            {
                exceptions = exceptions.Concat(assignmentGoalExceptions);
                return false;
            }
            if (!spendingGoalService.TryCreate(
                new CreateSpendingGoalRequest
                {
                    Fund = fund,
                    AccountingPeriod = accountingPeriod,
                    SpendingGoalType = spendingGoalType,
                },
                out SpendingGoal? spendingGoal,
                out IEnumerable<ValidationError> spendingGoalExceptions))
            {
                exceptions = exceptions.Concat(spendingGoalExceptions);
                return false;
            }
            assignmentGoals.Add(assignmentGoal);
            spendingGoals.Add(spendingGoal);
        }
        return true;
    }
}