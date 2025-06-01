using Data;
using Microsoft.Extensions.DependencyInjection;
using Tests.Mocks;

namespace Tests.Setups;

/// <summary>
/// Abstract base class representing a Scenario Setup
/// </summary>
internal abstract class ScenarioSetup : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly bool shouldUseDatabase = Environment.GetEnvironmentVariable("USE_DATABASE") == "TRUE";

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    protected ScenarioSetup()
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

    /// <inheritdoc/>
    public void Dispose()
    {
        if (shouldUseDatabase)
        {
            GetService<DatabaseContext>().Database.EnsureDeleted();
        }
    }
}