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
        _ = serviceCollection.AddScoped<AccountingPeriodTrendsGetter>();
        _ = serviceCollection.AddScoped<CurrentAccountingPeriodGetter>();

        _ = serviceCollection.AddScoped<AccountConverter>();
        _ = serviceCollection.AddScoped<CurrentAccountsGetter>();
        _ = serviceCollection.AddScoped<AccountTrendsGetter>();
        _ = serviceCollection.AddScoped<AccountGetter>();
        _ = serviceCollection.AddScoped<AccountSummaryGetter>();

        _ = serviceCollection.AddScoped<FundAmountConverter>();
        _ = serviceCollection.AddScoped<FundConverter>();
        _ = serviceCollection.AddScoped<CurrentFundsGetter>();
        _ = serviceCollection.AddScoped<FundTrendsGetter>();
        _ = serviceCollection.AddScoped<FundGetter>();
        _ = serviceCollection.AddScoped<FundSummaryGetter>();

        _ = serviceCollection.AddScoped<AssignmentGoalGetter>();
        _ = serviceCollection.AddScoped<CurrentGoalsGetter>();
        _ = serviceCollection.AddScoped<GoalConverter>();
        _ = serviceCollection.AddScoped<GoalTrendsGetter>();
        _ = serviceCollection.AddScoped<SpendingGoalGetter>();

        _ = serviceCollection.AddScoped<TransactionConverter>();
        _ = serviceCollection.AddScoped<TransactionTrendsGetter>();
        _ = serviceCollection.AddScoped<TransactionGetter>();
        _ = serviceCollection.AddScoped<TransactionRequestConverter>();
    }
}