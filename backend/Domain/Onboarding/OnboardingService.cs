using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Exceptions;
using Domain.Funds;

namespace Domain.Onboarding;

/// <summary>
/// Service that performs the one-time system onboarding flow.
/// </summary>
public class OnboardingService(
    AccountService accountService,
    FundService fundService,
    IAccountingPeriodRepository accountingPeriodRepository,
    IAccountRepository accountRepository,
    IFundRepository fundRepository)
{
    /// <summary>
    /// Determines whether onboarding can run against the current system state.
    /// </summary>
    public bool IsEligible(out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (accountRepository.GetAll().Count > 0)
        {
            exceptions = exceptions.Append(new InvalidAccountException("Onboarding is only available before any accounts are created."));
        }
        if (fundRepository.GetAll().Count > 0)
        {
            exceptions = exceptions.Append(new InvalidFundException("Onboarding is only available before any funds are created."));
        }
        if (accountingPeriodRepository.GetAll().Count > 0)
        {
            exceptions = exceptions.Append(new InvalidAccountingPeriodException("Onboarding is only available before any accounting periods are created."));
        }
        return !exceptions.Any();
    }

    /// <summary>
    /// Creates the initial Accounts and Funds outside of any accounting period.
    /// </summary>
    public bool Onboard(OnboardingRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!IsEligible(out IEnumerable<Exception> eligibilityExceptions))
        {
            exceptions = exceptions.Concat(eligibilityExceptions);
        }
        if (!ValidateRequest(request, out IEnumerable<Exception> validationExceptions))
        {
            exceptions = exceptions.Concat(validationExceptions);
        }
        if (exceptions.Any())
        {
            return false;
        }
        foreach (OnboardingAccountRequest accountRequest in request.Accounts)
        {
            if (!accountService.TryCreate(new CreateAccountRequest
            {
                Name = accountRequest.Name,
                Type = accountRequest.Type,
                OpeningAccountingPeriod = null,
                DateOpened = null,
                OnboardedBalance = accountRequest.Balance,
            }, out Account? account, out IEnumerable<Exception> accountExceptions))
            {
                exceptions = exceptions.Concat(accountExceptions);
                return false;
            }
            accountRepository.Add(account);
        }

        decimal totalAccountBalances = request.Accounts.Sum(account => account.Type.IsDebt() ? -account.Balance : account.Balance);
        decimal totalFundBalances = request.Funds.Sum(fund => fund.Balance);
        decimal unassignedBalance = totalAccountBalances - totalFundBalances;
        if (!fundService.TryCreate(new CreateFundRequest
        {
            Name = Fund.UnassignedFundName,
            Description = "Fund that tracks money that has not been assigned to a specific fund",
            AccountingPeriod = null,
            OnboardedBalance = unassignedBalance,
        }, out Fund? unassignedFund, out IEnumerable<Exception> unassignedFundExceptions))
        {
            exceptions = exceptions.Concat(unassignedFundExceptions);
            return false;
        }
        fundRepository.Add(unassignedFund);

        foreach (OnboardingFundRequest fundRequest in request.Funds)
        {
            if (!fundService.TryCreate(new CreateFundRequest
            {
                Name = fundRequest.Name,
                Description = fundRequest.Description,
                AccountingPeriod = null,
                OnboardedBalance = fundRequest.Balance,
            }, out Fund? fund, out IEnumerable<Exception> fundExceptions))
            {
                exceptions = exceptions.Concat(fundExceptions);
                return false;
            }
            fundRepository.Add(fund);
        }
        return true;
    }

    /// <summary>
    /// Validates the onboarding request.
    /// </summary>
    private static bool ValidateRequest(OnboardingRequest request, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        decimal totalAccountBalances = request.Accounts.Sum(account => account.Type.IsDebt() ? -account.Balance : account.Balance);
        decimal totalFundBalances = request.Funds.Sum(fund => fund.Balance);
        if (totalFundBalances > totalAccountBalances)
        {
            exceptions = exceptions.Append(new InvalidFundException("Sum of fund balances cannot exceed sum of account balances during onboarding."));
        }

        return !exceptions.Any();
    }
}