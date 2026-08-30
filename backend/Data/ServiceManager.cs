using Data.AccountingPeriods;
using Data.AccountGoals;
using Data.Accounts;
using Data.FundGoals;
using Data.Funds;
using Data.Locations;
using Data.Transactions;
using Data.Users;
using Domain.AccountingPeriods;
using Domain.AccountGoals;
using Domain.AccountGoals.Queries;
using Domain.AccountingPeriods.Queries;
using Domain.Accounts;
using Domain.Accounts.Queries;
using Domain.FundGoals;
using Domain.FundGoals.Queries;
using Domain.Funds;
using Domain.Funds.Queries;
using Domain.Locations;
using Domain.Locations.Queries;
using Domain.Transactions;
using Domain.Transactions.Queries;
using Domain.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Data;

/// <summary>
/// Static class for managing all the DI services required for the Data assembly
/// </summary>
public static class ServiceManager
{
    /// <summary>
    /// Registers all the Data DI services in the provided service collection
    /// </summary>
    /// <param name="serviceCollection">Service Collection</param>
    public static void Register(IServiceCollection serviceCollection)
    {
        _ = serviceCollection.AddDbContext<DatabaseContext>();
        _ = serviceCollection.AddScoped<UnitOfWork>();

        _ = serviceCollection.AddScoped<IAccountingPeriodRepository, AccountingPeriodRepository>();
        _ = serviceCollection.AddScoped<IAccountingPeriodQueryRepository, AccountingPeriodQueryRepository>();

        _ = serviceCollection.AddScoped<IAccountingPeriodBalanceHistoryRepository, AccountingPeriodBalanceHistoryRepository>();
        _ = serviceCollection.AddScoped<AccountingPeriodBalanceHistoryRepository>();

        _ = serviceCollection.AddScoped<IAccountBalanceEventQueryRepository, AccountBalanceEventQueryRepository>();
        _ = serviceCollection.AddScoped<IAccountRepository, AccountRepository>();
        _ = serviceCollection.AddScoped<IAccountQueryRepository, AccountQueryRepository>();

        _ = serviceCollection.AddScoped<IAccountGoalRepository, AccountGoalRepository>();
        _ = serviceCollection.AddScoped<IAccountGoalQueryRepository, AccountGoalQueryRepository>();

        _ = serviceCollection.AddScoped<IAccountBalanceHistoryRepository, AccountBalanceHistoryRepository>();
        _ = serviceCollection.AddScoped<IAccountPendingBalanceEffectRepository, PendingAccountBalanceEffectRepository>();

        _ = serviceCollection.AddScoped<IFundBalanceEventQueryRepository, FundBalanceEventQueryRepository>();
        _ = serviceCollection.AddScoped<IFundRepository, FundRepository>();
        _ = serviceCollection.AddScoped<IFundQueryRepository, FundQueryRepository>();
        _ = serviceCollection.AddScoped<IFundBalanceHistoryRepository, FundBalanceHistoryRepository>();
        _ = serviceCollection.AddScoped<IFundPendingBalanceEffectRepository, PendingFundBalanceEffectRepository>();

        _ = serviceCollection.AddScoped<IFundGoalBalanceEventQueryRepository, FundGoalBalanceEventQueryRepository>();
        _ = serviceCollection.AddScoped<IFundGoalRepository, FundGoalRepository>();
        _ = serviceCollection.AddScoped<IFundGoalQueryRepository, FundGoalQueryRepository>();
        _ = serviceCollection.AddScoped<IFundGoalTotalsHistoryRepository, FundGoalTotalsHistoryRepository>();
        _ = serviceCollection.AddScoped<IFundGoalPendingTotalsEffectRepository, PendingFundGoalTotalsEffectRepository>();

        _ = serviceCollection.AddScoped<ITransactionRepository, TransactionRepository>();
        _ = serviceCollection.AddScoped<ITransactionBalanceEventQueryRepository, TransactionBalanceEventQueryRepository>();
        _ = serviceCollection.AddScoped<ITransactionQueryRepository, TransactionQueryRepository>();

        _ = serviceCollection.AddScoped<ILocationRepository, LocationRepository>();
        _ = serviceCollection.AddScoped<ILocationQueryRepository, LocationQueryRepository>();

        _ = serviceCollection.AddScoped<IUserRepository, UserRepository>();
        _ = serviceCollection.AddScoped<IUserInvitationRepository, UserInvitationRepository>();
        _ = serviceCollection.AddScoped<IUserAdministrationAuditEventRepository, UserAdministrationAuditEventRepository>();
        _ = serviceCollection.AddScoped<UserManagementBootstrapper>();
    }
}
