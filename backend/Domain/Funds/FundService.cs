using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Exceptions;
using Domain.Goals;
using Domain.Transactions;

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
        out IEnumerable<Exception> exceptions)
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
        out IEnumerable<Exception> exceptions)
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
            out IEnumerable<Exception> assignmentGoalExceptions))
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
            out IEnumerable<Exception> spendingGoalExceptions))
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
    public bool TryUpdate(Fund fund, string name, string description, out IEnumerable<Exception> exceptions)
    {
        if (!ValidateUpdate(fund, name, out exceptions))
        {
            return false;
        }
        fund.Name = name;
        fund.Description = description;
        return true;
    }

    /// <summary>
    /// Attempts to delete an existing Fund
    /// </summary>
    public bool TryDelete(Fund fund, out IEnumerable<Exception> exceptions)
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
        out IEnumerable<Exception> exceptions)
    {
        fund = null;

        if (!ValidateCreate(Fund.UnassignedFundName, openingAccountingPeriod, out exceptions))
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
        out IEnumerable<Exception> exceptions)
    {
        fund = null;

        if (!ValidateOnboard(Fund.UnassignedFundName, onboardedBalance, out exceptions))
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
    private bool ValidateName(string name, Fund? existingFund, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (string.IsNullOrEmpty(name))
        {
            exceptions = exceptions.Append(new InvalidNameException("Fund name cannot be empty"));
        }
        if (fundRepository.TryGetByName(name, out Fund? existingFundWithName) && existingFundWithName != existingFund)
        {
            exceptions = exceptions.Append(new InvalidNameException("Fund name must be unique"));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the provided request to create a fund
    /// </summary>
    private bool ValidateCreate(CreateFundRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidateName(request.Name, null, out IEnumerable<Exception> nameExceptions))
        {
            exceptions = exceptions.Concat(nameExceptions);
        }
        if (!request.OpeningAccountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new InvalidAccountingPeriodException("The provided accounting period is closed."));
        }
        if (!ValidateGoalConfiguration(request.AssignmentGoalType, request.AssignmentGoalAmount, request.SpendingGoalType, out IEnumerable<Exception> goalExceptions))
        {
            exceptions = exceptions.Concat(goalExceptions);
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the provided request to onboard a Fund.
    /// </summary>
    private bool ValidateOnboard(OnboardFundRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidateName(request.Name, null, out IEnumerable<Exception> nameExceptions))
        {
            exceptions = exceptions.Concat(nameExceptions);
        }
        if (accountingPeriodRepository.GetAll().Count > 0)
        {
            exceptions = exceptions.Append(new InvalidAccountingPeriodException("Funds can only be onboarded before any Accounting Periods have been created."));
        }
        if (!ValidateGoalConfiguration(request.AssignmentGoalType, request.AssignmentGoalAmount, request.SpendingGoalType, out IEnumerable<Exception> goalExceptions))
        {
            exceptions = exceptions.Concat(goalExceptions);
        }
        if (request.Name != Fund.UnassignedFundName)
        {
            Fund? unassignedFund = fundRepository.GetUnassignedFund();
            if (unassignedFund == null)
            {
                exceptions = exceptions.Append(new InvalidFundException("The unassigned fund must exist before onboarding a Fund."));
            }
            else if (unassignedFund.OnboardedBalance < request.OnboardedBalance)
            {
                exceptions = exceptions.Append(new InvalidFundException("There is not enough unassigned balance to onboard this Fund."));
            }
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the name for a Fund during unassigned onboarding.
    /// </summary>
    private bool ValidateOnboard(string name, decimal onboardedBalance, out IEnumerable<Exception> exceptions)
    {
        var request = new OnboardFundRequest
        {
            Name = name,
            Description = Fund.UnassignedFundDescription,
            OnboardedBalance = onboardedBalance,
            AssignmentGoalType = AssignmentGoalType.MonthlyTarget,
            AssignmentGoalAmount = 1,
            SpendingGoalType = SpendingGoalType.Standard,
        };
        return ValidateOnboard(request, out exceptions);
    }

    /// <summary>
    /// Validates the provided information to update a fund
    /// </summary>
    private bool ValidateUpdate(Fund fund, string name, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidateName(name, fund, out IEnumerable<Exception> nameExceptions))
        {
            exceptions = exceptions.Concat(nameExceptions);
        }
        if (fund.IsUnassignedFund)
        {
            exceptions = exceptions.Append(new UnableToUpdateException("The unassigned fund cannot be updated."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates whether a fund can be deleted
    /// </summary>
    private bool ValidateDelete(Fund fund, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (fund.IsUnassignedFund)
        {
            exceptions = exceptions.Append(new UnableToDeleteException("The unassigned fund cannot be deleted."));
        }
        if (fund.IsOnboarded && accountingPeriodRepository.GetLatestAccountingPeriod() != null)
        {
            exceptions = exceptions.Append(new UnableToDeleteException("Cannot delete an onboarded Fund."));
        }
        if (transactionRepository.DoAnyTransactionsExistForFund(fund.Id))
        {
            exceptions = exceptions.Append(new UnableToDeleteException("Cannot delete a Fund that has Transactions."));
        }
        return !exceptions.Any();
    }

    private static bool ValidateGoalConfiguration(
        AssignmentGoalType assignmentGoalType,
        decimal assignmentGoalAmount,
        SpendingGoalType spendingGoalType,
        out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!Enum.IsDefined(assignmentGoalType))
        {
            exceptions = exceptions.Append(new InvalidGoalTypeException("The provided assignment goal type is invalid."));
        }
        if (assignmentGoalAmount < 0)
        {
            exceptions = exceptions.Append(new InvalidFundException("Goal amount must be greater than or equal to zero."));
        }
        if (!Enum.IsDefined(spendingGoalType))
        {
            exceptions = exceptions.Append(new InvalidGoalTypeException("The provided spending goal type is invalid."));
        }
        return !exceptions.Any();
    }

    private IEnumerable<AccountingPeriod> GetAccountingPeriodsFrom(AccountingPeriod openingAccountingPeriod)
    {
        AccountingPeriod? accountingPeriod = openingAccountingPeriod;
        while (accountingPeriod != null)
        {
            yield return accountingPeriod;
            accountingPeriod = accountingPeriodRepository.GetNextAccountingPeriod(accountingPeriod.Id);
        }
    }

    private bool TryCreateGoalsForAccountingPeriods(
        Fund fund,
        IEnumerable<AccountingPeriod> accountingPeriods,
        AssignmentGoalType assignmentGoalType,
        decimal assignmentGoalAmount,
        SpendingGoalType spendingGoalType,
        List<AssignmentGoal> assignmentGoals,
        List<SpendingGoal> spendingGoals,
        out IEnumerable<Exception> exceptions)
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
                out IEnumerable<Exception> assignmentGoalExceptions))
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
                out IEnumerable<Exception> spendingGoalExceptions))
            {
                exceptions = exceptions.Concat(spendingGoalExceptions);
                return false;
            }
            assignmentGoals.Add(assignmentGoal);
            spendingGoals.Add(spendingGoal);
        }
        return true;
    }

    private bool ValidateCreate(string name, AccountingPeriod openingAccountingPeriod, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidateName(name, null, out IEnumerable<Exception> nameExceptions))
        {
            exceptions = exceptions.Concat(nameExceptions);
        }
        if (!openingAccountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new InvalidAccountingPeriodException("The provided accounting period is closed."));
        }
        return !exceptions.Any();
    }
}