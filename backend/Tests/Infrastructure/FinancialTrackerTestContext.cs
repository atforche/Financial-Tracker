using Tests.AccountGoals;
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
    private TestApiClient ApiClient { get; }

    private FinancialTrackerTestContext()
    {
        ApiClient = new TestApiClient(_factory.CreateClient());
    }

    /// <summary>
    /// Gets account setup commands.
    /// </summary>
    public AccountBuilderFactory Accounts => new(ApiClient);

    /// <summary>
    /// Gets accounting-period setup commands.
    /// </summary>
    public AccountingPeriodBuilderFactory Periods => new(ApiClient);

    /// <summary>
    /// Gets fund setup commands.
    /// </summary>
    public FundBuilderFactory Funds => new(ApiClient);

    /// <summary>
    /// Gets transaction setup commands.
    /// </summary>
    public TransactionBuilderFactory Transactions => new(ApiClient);

    /// <summary>
    /// Gets the raw API client for response-contract assertions.
    /// </summary>
    public TestApiClient Api => ApiClient;

    /// <summary>
    /// Gets account queries.
    /// </summary>
    public AccountQueries AccountQueries => new(ApiClient);

    /// <summary>
    /// Gets accounting-period queries.
    /// </summary>
    public AccountingPeriodQueries AccountingPeriodQueries => new(ApiClient);

    /// <summary>
    /// Gets fund queries.
    /// </summary>
    public FundQueries FundQueries => new(ApiClient);

    /// <summary>
    /// Gets fund-goal queries.
    /// </summary>
    public FundGoalQueries FundGoalQueries => new(ApiClient);

    /// <summary>
    /// Gets Account Goal queries.
    /// </summary>
    public AccountGoalQueries AccountGoalQueries => new(_factory);

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
        ApiClient.Dispose();
        _factory.Dispose();
        return ValueTask.CompletedTask;
    }

    private Task InitializeAsync() => _factory.InitializeDatabaseAsync();
}
