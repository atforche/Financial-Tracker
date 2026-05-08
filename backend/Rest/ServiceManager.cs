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
        _ = serviceCollection.AddScoped<AccountingPeriodAccountConverter>();
        _ = serviceCollection.AddScoped<AccountingPeriodAccountGetter>();
        _ = serviceCollection.AddScoped<AccountingPeriodConverter>();
        _ = serviceCollection.AddScoped<AccountingPeriodFundConverter>();
        _ = serviceCollection.AddScoped<AccountingPeriodFundGetter>();
        _ = serviceCollection.AddScoped<AccountingPeriodGoalGetter>();
        _ = serviceCollection.AddScoped<AccountingPeriodGetter>();
        _ = serviceCollection.AddScoped<AccountingPeriodTransactionGetter>();

        _ = serviceCollection.AddScoped<AccountConverter>();
        _ = serviceCollection.AddScoped<AccountGetter>();
        _ = serviceCollection.AddScoped<AccountTransactionGetter>();

        _ = serviceCollection.AddScoped<FundAmountConverter>();
        _ = serviceCollection.AddScoped<FundConverter>();
        _ = serviceCollection.AddScoped<FundGetter>();
        _ = serviceCollection.AddScoped<FundTransactionGetter>();

        _ = serviceCollection.AddScoped<GoalConverter>();

        _ = serviceCollection.AddScoped<TransactionConverter>();
        _ = serviceCollection.AddScoped<TransactionRequestConverter>();
    }
}