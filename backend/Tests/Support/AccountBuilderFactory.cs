namespace Tests.Support;

/// <summary>
/// Starts builders for account setup.
/// </summary>
internal sealed class AccountBuilderFactory(TestApiClient apiClient)
{
    /// <summary>
    /// Starts a builder for an account onboarded before accounting begins.
    /// </summary>
    public AccountBuilder Onboard(string name) => new(apiClient, name);

    /// <summary>
    /// Starts a builder for an account created in an accounting period.
    /// </summary>
    public AccountBuilder Create(string name, AccountingPeriodHandle period) => new AccountBuilder(apiClient, name).In(period);
}
