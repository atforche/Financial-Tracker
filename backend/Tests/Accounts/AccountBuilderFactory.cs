using Tests.Infrastructure;

namespace Tests.Accounts;

/// <summary>
/// Starts builders for account setup.
/// </summary>
internal sealed class AccountBuilderFactory(TestApiClient apiClient)
{
    /// <summary>
    /// Starts a builder for an onboarded standard account.
    /// </summary>
    public AccountBuilder Onboard(string name) => new(apiClient, name);
}