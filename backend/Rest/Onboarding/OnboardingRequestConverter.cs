using System.Diagnostics.CodeAnalysis;
using Domain.Accounts;
using Domain.Onboarding;
using Models.Onboarding;
using Rest.Accounts;

namespace Rest.Onboarding;

/// <summary>
/// Converts onboarding models into domain requests.
/// </summary>
public sealed class OnboardingRequestConverter
{
    /// <summary>
    /// Attempts to convert the provided onboarding model into a domain request.
    /// </summary>
    public static bool TryToDomain(
        OnboardingModel onboardingModel,
        [NotNullWhen(true)] out OnboardingRequest? onboardingRequest,
        out Dictionary<string, string[]> errors)
    {
        onboardingRequest = null;
        errors = [];

        List<OnboardingAccountRequest> accountRequests = [];
        int accountIndex = 0;
        foreach (OnboardingAccountModel accountModel in onboardingModel.Accounts)
        {
            if (!AccountTypeConverter.TryToDomain(accountModel.Type, out AccountType? accountType))
            {
                errors.Add($"{nameof(onboardingModel.Accounts)}[{accountIndex}].{nameof(OnboardingAccountModel.Type)}", [$"Unrecognized Account Type: {accountModel.Type}"]);
                accountIndex++;
                continue;
            }
            accountRequests.Add(new OnboardingAccountRequest
            {
                Name = accountModel.Name,
                Type = accountType.Value,
                Balance = accountModel.Balance,
            });
            accountIndex++;
        }
        IReadOnlyCollection<OnboardingFundRequest> fundRequests = onboardingModel.Funds
            .Select(fundModel => new OnboardingFundRequest
            {
                Name = fundModel.Name,
                Description = fundModel.Description,
                Balance = fundModel.Balance,
            })
            .ToList();
        if (errors.Count > 0)
        {
            return false;
        }
        onboardingRequest = new OnboardingRequest
        {
            Accounts = accountRequests,
            Funds = fundRequests,
        };
        return true;
    }
}