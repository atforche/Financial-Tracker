using Rest.Mappers;

namespace Rest;

/// <summary>
/// Static class for managing all the DI services required for the Rest assembly
/// </summary>
public static class ServiceManager
{
    /// <summary>
    /// Registers all the Data DI services in the provided service collection
    /// </summary>
    /// <param name="serviceCollection">Service Collection</param>
    public static void Register(IServiceCollection serviceCollection)
    {
        _ = serviceCollection.AddScoped<AccountingPeriodMapper>();
        _ = serviceCollection.AddScoped<AccountMapper>();
        _ = serviceCollection.AddScoped<FundMapper>();
        _ = serviceCollection.AddScoped<FundAmountMapper>();
        _ = serviceCollection.AddScoped<TransactionMapper>();
    }
}