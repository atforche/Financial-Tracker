using Data;
using Microsoft.Extensions.DependencyInjection;
using Tests.Mocks;

namespace Tests.Setups;

/// <summary>
/// Abstract base class representing a Scenario Setup
/// </summary>
internal class ScenarioSetup : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly bool shouldUseDatabase = Environment.GetEnvironmentVariable("USE_DATABASE") == "TRUE";

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    public ScenarioSetup()
    {
        var serviceCollection = new ServiceCollection();
        ServiceManager.Register(serviceCollection, shouldUseDatabase);
        _serviceProvider = serviceCollection.BuildServiceProvider();

        if (shouldUseDatabase)
        {
            if (!Directory.Exists(TestDatabaseContext.DatabaseDirectory))
            {
                Directory.CreateDirectory(TestDatabaseContext.DatabaseDirectory);
            }
            GetService<DatabaseContext>().Database.EnsureCreated();
        }
    }

    /// <summary>
    /// Gets the service of the provided type from the service provider
    /// </summary>
    public TService GetService<TService>() => _serviceProvider.GetService<TService>() ?? throw new InvalidOperationException();

    #region IDisposable

    private bool _disposed;

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            if (shouldUseDatabase)
            {
                GetService<DatabaseContext>().Database.EnsureDeleted();
            }
        }
        _disposed = true;
    }

    #endregion
}