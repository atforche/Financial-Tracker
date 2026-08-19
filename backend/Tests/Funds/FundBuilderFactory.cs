using Tests.Infrastructure;

namespace Tests.Funds;

/// <summary>
/// Starts builders for fund setup.
/// </summary>
internal sealed class FundBuilderFactory(TestApiClient apiClient)
{
    /// <summary>
    /// Starts a builder for a fund.
    /// </summary>
    public FundBuilder Create(string name) => new(apiClient, name);
}
