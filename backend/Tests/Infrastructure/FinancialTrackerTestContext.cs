using Tests.AccountingPeriods;
using Tests.Accounts;
using Tests.FundGoals;
using Tests.Funds;
using Tests.Transactions;

namespace Tests.Infrastructure;

/// <summary>
/// Owns an isolated application instance and exposes test-facing commands and queries.
/// </summary>
internal sealed class FinancialTrackerTestContext : IAsyncDisposable
{
    private readonly FinancialTrackerApplicationFactory _factory = new();
    private readonly TestApiClient _apiClient;

    private FinancialTrackerTestContext()
    {
        _apiClient = new TestApiClient(_factory.CreateClient());
    }

    /// <summary>
    /// Gets account setup commands.
    /// </summary>
    public AccountBuilderFactory Accounts => new(_apiClient);

    /// <summary>
    /// Gets accounting-period setup commands.
    /// </summary>
    public AccountingPeriodBuilderFactory Periods => new(_apiClient);

    /// <summary>
    /// Gets fund setup commands.
    /// </summary>
    public FundBuilderFactory Funds => new(_apiClient);

    /// <summary>
    /// Gets transaction setup commands.
    /// </summary>
    public TransactionBuilderFactory Transactions => new(_apiClient);

    /// <summary>
    /// Gets account queries.
    /// </summary>
    public AccountQueries AccountQueries => new(_apiClient);

    /// <summary>
    /// Gets accounting-period queries.
    /// </summary>
    public AccountingPeriodQueries AccountingPeriodQueries => new(_apiClient);

    /// <summary>
    /// Gets fund queries.
    /// </summary>
    public FundQueries FundQueries => new(_apiClient);

    /// <summary>
    /// Gets fund-goal queries.
    /// </summary>
    public FundGoalQueries FundGoalQueries => new(_apiClient);

    /// <summary>
    /// Creates an empty test context backed by the full application stack.
    /// </summary>
    public static async Task<FinancialTrackerTestContext> CreateAsync()
    {
        FinancialTrackerTestContext context = new();
        try
        {
            await context.InitializeAsync();
            return context;
        }
        catch
        {
            await context.DisposeAsync();
            throw;
        }
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        _apiClient.Dispose();
        _factory.Dispose();
        return ValueTask.CompletedTask;
    }

    private Task InitializeAsync() => _factory.InitializeDatabaseAsync();
}