namespace Tests.Support;

/// <summary>
/// Starts builders for accounting-period setup and changes accounting-period state.
/// </summary>
internal sealed class AccountingPeriodBuilderFactory(TestApiClient apiClient)
{
    /// <summary>
    /// Starts a builder for an accounting period.
    /// </summary>
    public AccountingPeriodBuilder Create(int year, int month) => new(apiClient, year, month);

    /// <summary>
    /// Closes an accounting period.
    /// </summary>
    public Task CloseAsync(AccountingPeriodHandle period) => apiClient.PostAsync($"/accounting-periods/{period.Id}/close");

    /// <summary>
    /// Reopens a closed accounting period.
    /// </summary>
    public Task ReopenAsync(AccountingPeriodHandle period) => apiClient.PostAsync($"/accounting-periods/{period.Id}/reopen");
}
