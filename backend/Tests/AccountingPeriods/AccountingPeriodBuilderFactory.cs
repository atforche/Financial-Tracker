using Tests.Infrastructure;

namespace Tests.AccountingPeriods;

/// <summary>
/// Starts builders for accounting-period setup.
/// </summary>
internal sealed class AccountingPeriodBuilderFactory(TestApiClient apiClient)
{
    /// <summary>
    /// Starts a builder for an accounting period.
    /// </summary>
    public AccountingPeriodBuilder Create(int year, int month) => new(apiClient, year, month);
}