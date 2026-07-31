using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Rest.Health;

/// <summary>
/// Reports whether the application database is ready to serve requests.
/// </summary>
public sealed class DatabaseHealthCheck(Data.DatabaseContext databaseContext) : IHealthCheck
{
    /// <inheritdoc/>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            databaseContext.RunHealthCheck();
            return Task.FromResult(HealthCheckResult.Healthy());
        }
        catch (Exception exception)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "The application database is not ready.",
                exception));
        }
    }
}