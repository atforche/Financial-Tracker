using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.FundPlans;
using Domain.Transactions;
using Domain.Validation;

namespace Domain.Funds;

/// <summary>
/// Service for managing Funds
/// </summary>
public class FundService(
    IFundRepository fundRepository,
    IAccountingPeriodRepository accountingPeriodRepository,
    IFundPlanRepository fundPlanRepository,
    ITransactionRepository transactionRepository,
    AccountingPeriodBalanceService accountingPeriodBalanceService,
    FundPlanService fundPlanService)
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
        List<FundPlan> fundPlans = [];
        AccountingPeriod? accountingPeriod = request.OpeningAccountingPeriod;
        while (accountingPeriod != null)
        {
            if (!fundPlanService.TryCreate(
                new CreateFundPlanRequest
                {
                    Fund = fund,
                    AccountingPeriod = accountingPeriod,
                    RegularContribution = request.RegularContribution,
                    MinimumFundedBalance = request.MinimumFundedBalance,
                    MaximumFundedBalance = request.MaximumFundedBalance,
                    TargetEndingBalance = request.TargetEndingBalance,
                },
                out FundPlan? fundPlan,
                out exceptions))
            {
                fund = null;
                return false;
            }
            fundPlans.Add(fundPlan);
            accountingPeriod = accountingPeriodRepository.GetNextAccountingPeriod(accountingPeriod.Id);
        }

        fundRepository.Add(fund);
        foreach (FundPlan fundPlan in fundPlans)
        {
            if (!fundPlanRepository.TryAdd(fundPlan))
            {
                foreach (FundPlan addedFundPlan in fundPlans.TakeWhile(plan => plan != fundPlan))
                {
                    fundPlanRepository.Delete(addedFundPlan);
                }
                fundRepository.Delete(fund);
                fund = null;
                exceptions = [new ValidationError(
                    new ValidationErrorPath(nameof(CreateFundRequest.Name)),
                    "A Fund Plan already exists for this Fund.")];
                return false;
            }
        }
        accountingPeriodBalanceService.AddFund(fund);
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
        if (!fundPlanService.TryCreate(
            new CreateFundPlanRequest
            {
                Fund = fund,
                AccountingPeriod = null,
                RegularContribution = request.RegularContribution,
                MinimumFundedBalance = request.MinimumFundedBalance,
                MaximumFundedBalance = request.MaximumFundedBalance,
                TargetEndingBalance = request.TargetEndingBalance,
            },
            out FundPlan? fundPlan,
            out IEnumerable<ValidationError> fundPlanExceptions))
        {
            exceptions = fundPlanExceptions;
            fund = null;
            return false;
        }

        fundRepository.Add(fund);
        if (!fundPlanRepository.TryAdd(fundPlan))
        {
            fundRepository.Delete(fund);
            fund = null;
            exceptions = [new ValidationError(
                new ValidationErrorPath(nameof(OnboardFundRequest.Name)),
                "A Fund Plan already exists for this Fund.")];
            return false;
        }
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
        foreach (FundPlan fundPlan in fundPlanRepository.GetAllByFund(fund.Id))
        {
            fundPlanRepository.Delete(fundPlan);
        }
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
            RegularContribution = null,
            MinimumFundedBalance = null,
            MaximumFundedBalance = null,
            TargetEndingBalance = null,
        };
        if (!ValidateCreate(request, out exceptions, allowUnassignedFund: true))
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
            RegularContribution = null,
            MinimumFundedBalance = null,
            MaximumFundedBalance = null,
            TargetEndingBalance = null,
        };
        if (!ValidateOnboard(request, out exceptions, allowUnassignedFund: true))
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

        if (string.IsNullOrWhiteSpace(name))
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
    private bool ValidateCreate(
        CreateFundRequest request,
        out IEnumerable<ValidationError> exceptions,
        bool allowUnassignedFund = false)
    {
        exceptions = [];

        if (!ValidateName(request.Name, new ValidationErrorPath(nameof(CreateFundRequest.Name)), null, out IEnumerable<ValidationError> nameExceptions))
        {
            exceptions = exceptions.Concat(nameExceptions);
        }
        if (!allowUnassignedFund && string.Equals(request.Name, Fund.UnassignedFundName, StringComparison.OrdinalIgnoreCase))
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateFundRequest.Name)),
                "The unassigned fund name is reserved."));
        }
        if (!request.OpeningAccountingPeriod.IsOpen)
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(CreateFundRequest.OpeningAccountingPeriod)),
                "The provided accounting period is closed."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Validates the provided request to onboard a Fund.
    /// </summary>
    private bool ValidateOnboard(
        OnboardFundRequest request,
        out IEnumerable<ValidationError> exceptions,
        bool allowUnassignedFund = false)
    {
        exceptions = [];

        if (!ValidateName(request.Name, new ValidationErrorPath(nameof(OnboardFundRequest.Name)), null, out IEnumerable<ValidationError> nameExceptions))
        {
            exceptions = exceptions.Concat(nameExceptions);
        }
        if (!allowUnassignedFund && string.Equals(request.Name, Fund.UnassignedFundName, StringComparison.OrdinalIgnoreCase))
        {
            exceptions = exceptions.Append(new ValidationError(
                new ValidationErrorPath(nameof(OnboardFundRequest.Name)),
                "The unassigned fund name is reserved."));
        }
        if (accountingPeriodRepository.GetAll().Count > 0)
        {
            exceptions = exceptions.Append(new ValidationError(ValidationErrorPath.Empty, "Funds can only be onboarded before any Accounting Periods have been created."));
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
}