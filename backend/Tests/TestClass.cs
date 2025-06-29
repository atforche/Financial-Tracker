using Data;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

/// <summary>
/// Abstract base class representing a Test Class
/// </summary>
public abstract class TestClass
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    protected TestClass()
    {
        var serviceCollection = new ServiceCollection();
        ServiceManager.Register(serviceCollection);
        _serviceProvider = serviceCollection.BuildServiceProvider();
        if (DatabaseFixture.ShouldUseDatabase)
        {
            GetService<DatabaseContext>().Database.EnsureDeleted();
            GetService<DatabaseContext>().Database.EnsureCreated();
        }
    }

    /// <summary>
    /// Gets the service of the provided type from the service provider
    /// </summary>
    public TService GetService<TService>() => _serviceProvider.GetService<TService>() ?? throw new InvalidOperationException();
}