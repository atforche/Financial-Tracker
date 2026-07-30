using Models.Accounts;
using Tests.Infrastructure;

namespace Tests.Accounts;

/// <summary>
/// Builds an onboarded standard account.
/// </summary>
internal sealed class AccountBuilder(TestApiClient apiClient, string name)
{
    private decimal _openingBalance;

    /// <summary>
    /// Sets the account's onboarded balance.
    /// </summary>
    public AccountBuilder WithOpeningBalance(decimal value)
    {
        _openingBalance = value;
        return this;
    }

    /// <summary>
    /// Creates the account.
    /// </summary>
    public async Task<AccountHandle> CreateAsync()
    {
        AccountModel model = await apiClient.PostAsync<OnboardAccountModel, AccountModel>("/accounts/onboard", new OnboardAccountModel
        {
            Name = name,
            Type = AccountTypeModel.Standard,
            OnboardedBalance = _openingBalance
        });
        return new AccountHandle(model.Id, model.Name);
    }
}