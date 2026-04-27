using Rest.AccountingPeriods;
using Rest.Accounts;
using Rest.Funds;
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
        _ = serviceCollection.AddScoped<AccountingPeriodAccountConverter>();
        _ = serviceCollection.AddScoped<AccountingPeriodFundConverter>();
        _ = serviceCollection.AddScoped<AccountConverter>();
        _ = serviceCollection.AddScoped<AccountBalanceConverter>();
        _ = serviceCollection.AddScoped<FundConverter>();
        _ = serviceCollection.AddScoped<FundGoalConverter>();
        _ = serviceCollection.AddScoped<FundAmountConverter>();
        _ = serviceCollection.AddScoped<FundBalanceConverter>();
        _ = serviceCollection.AddScoped<TransactionRequestConverter>();
        _ = serviceCollection.AddScoped<TransactionConverter>();
    }
}