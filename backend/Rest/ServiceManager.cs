using Microsoft.AspNetCore.Authorization;
using Rest.AccountGoals;
using Rest.AccountingPeriods;
using Rest.Accounts;
using Rest.Authentication;
using Rest.FundGoals;
using Rest.Funds;
using Rest.Locations;
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
        _ = serviceCollection.AddScoped<ICurrentApplicationUserAccessor, CurrentApplicationUserAccessor>();
        _ = serviceCollection.AddScoped<IAuthorizationHandler, ProviderIdentityAuthorizationHandler>();
        _ = serviceCollection.AddScoped<IAuthorizationHandler, ActiveUserAuthorizationHandler>();
        _ = serviceCollection.AddScoped<IAuthorizationHandler, WriteCapableUserAuthorizationHandler>();
        _ = serviceCollection.AddScoped<IAuthorizationHandler, AdministratorAuthorizationHandler>();
        _ = serviceCollection.AddScoped<IAuthorizationHandler, ApplicationAccessAuthorizationHandler>();

        _ = serviceCollection.AddScoped<AccountingPeriodConverter>();
        _ = serviceCollection.AddScoped<AccountingPeriodQueryConverter>();

        _ = serviceCollection.AddScoped<AccountGoalConverter>();

        _ = serviceCollection.AddScoped<AccountConverter>();
        _ = serviceCollection.AddScoped<AccountBalanceEventConverter>();

        _ = serviceCollection.AddScoped<FundConverter>();
        _ = serviceCollection.AddScoped<FundBalanceEventConverter>();

        _ = serviceCollection.AddScoped<FundGoalConverter>();
        _ = serviceCollection.AddScoped<FundGoalBalanceEventConverter>();

        _ = serviceCollection.AddScoped<TransactionRequestConverter>();
        _ = serviceCollection.AddScoped<TransactionConverter>();
        _ = serviceCollection.AddScoped<LocationConverter>();
    }
}
