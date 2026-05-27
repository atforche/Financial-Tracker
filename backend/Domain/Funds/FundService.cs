using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Exceptions;
using Domain.Transactions;

namespace Domain.Funds;

/// <summary>
/// Service for managing Funds
/// </summary>
public class FundService(
    IFundRepository fundRepository,
    IAccountingPeriodRepository accountingPeriodRepository,
    ITransactionRepository transactionRepository,
    AccountingPeriodBalanceService accountingPeriodBalanceService)
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

        if (!ValidateCreate(request.Name, request.OpeningAccountingPeriod, out exceptions))
        {
            return false;
        }
        fund = new Fund(request.Name, request.Description, request.OpeningAccountingPeriod.Id);
        accountingPeriodBalanceService.AddFund(fund);
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

        if (!ValidateOnboard(request.Name, request.OnboardedBalance, out exceptions))
        {
            return false;
        }
        fund = new Fund(request.Name, request.Description, request.OnboardedBalance);
        fundRepository.Add(fund);
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

    /// <summary>
    /// Validates the provided request to onboard a Fund.
    /// </summary>
    private bool ValidateOnboard(string name, decimal onboardedBalance, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidateName(name, null, out IEnumerable<Exception> nameExceptions))
        {
            exceptions = exceptions.Concat(nameExceptions);
        }
        if (accountingPeriodRepository.GetAll().Count > 0)
        {
            exceptions = exceptions.Append(new InvalidAccountingPeriodException("Funds can only be onboarded before any Accounting Periods have been created."));
        }
        if (name != Fund.UnassignedFundName)
        {
            Fund? unassignedFund = fundRepository.GetUnassignedFund();
            if (unassignedFund == null)
            {
                exceptions = exceptions.Append(new InvalidFundException("The unassigned fund must exist before onboarding a Fund."));
            }
            else if (unassignedFund.OnboardedBalance < onboardedBalance)
            {
                exceptions = exceptions.Append(new InvalidFundException("There is not enough unassigned balance to onboard this Fund."));
            }
        }
        return !exceptions.Any();
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
}