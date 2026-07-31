using System.Net;
using Tests.Infrastructure;

namespace Tests.Health;

/// <summary>
/// Verifies the unauthenticated operational health endpoints.
/// </summary>
public sealed class HealthEndpointTests
{
    /// <summary>
    /// Verifies liveness does not depend on database readiness.
    /// </summary>
    [Fact]
    public async Task LiveReturnsSuccessWhenDatabaseIsNotInitialized()
    {
        using FinancialTrackerApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync(new Uri("/health/live", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Verifies readiness reflects whether database migrations have been applied.
    /// </summary>
    [Fact]
    public async Task ReadyReflectsDatabaseMigrationState()
    {
        using FinancialTrackerApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage unavailableResponse = await client.GetAsync(new Uri("/health/ready", UriKind.Relative));
        Assert.Equal(HttpStatusCode.ServiceUnavailable, unavailableResponse.StatusCode);

        await factory.InitializeDatabaseAsync();

        using HttpResponseMessage readyResponse = await client.GetAsync(new Uri("/health/ready", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, readyResponse.StatusCode);
    }
}