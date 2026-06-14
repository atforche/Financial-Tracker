using Rest.AccountingPeriods;
using Rest.Accounts;
using Rest.Funds;
using Rest.Goals;
using Rest.Transactions;

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
        _ = serviceCollection.AddScoped<AccountingPeriodConverter>();
        _ = serviceCollection.AddScoped<AccountingPeriodGetter>();
        _ = serviceCollection.AddScoped<AccountingPeriodDashboardGetter>();

        _ = serviceCollection.AddScoped<AccountConverter>();
        _ = serviceCollection.AddScoped<AccountDashboardGetter>();
        _ = serviceCollection.AddScoped<AccountGetter>();

        _ = serviceCollection.AddScoped<FundAmountConverter>();
        _ = serviceCollection.AddScoped<FundConverter>();
        _ = serviceCollection.AddScoped<FundDashboardGetter>();
        _ = serviceCollection.AddScoped<FundGetter>();

        _ = serviceCollection.AddScoped<GoalConverter>();
        _ = serviceCollection.AddScoped<GoalDashboardGetter>();

        _ = serviceCollection.AddScoped<TransactionConverter>();
        _ = serviceCollection.AddScoped<TransactionDashboardGetter>();
        _ = serviceCollection.AddScoped<TransactionGetter>();
        _ = serviceCollection.AddScoped<TransactionRequestConverter>();
    }
}