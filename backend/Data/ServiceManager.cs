using Data.AccountingPeriods;
using Data.Accounts;
using Data.FundPlans;
using Data.Funds;
using Data.Transactions;
using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Domain.Accounts;
using Domain.Accounts.Queries;
using Domain.FundPlans;
using Domain.FundPlans.Queries;
using Domain.Funds;
using Domain.Funds.Queries;
using Domain.Transactions;
using Domain.Transactions.Queries;
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

        _ = serviceCollection.AddScoped<IAccountBalanceHistoryRepository, AccountBalanceHistoryRepository>();
        _ = serviceCollection.AddScoped<IAccountPendingBalanceEffectRepository, PendingAccountBalanceEffectRepository>();

        _ = serviceCollection.AddScoped<IFundBalanceEventQueryRepository, FundBalanceEventQueryRepository>();
        _ = serviceCollection.AddScoped<IFundRepository, FundRepository>();
        _ = serviceCollection.AddScoped<IFundQueryRepository, FundQueryRepository>();

        _ = serviceCollection.AddScoped<IFundBalanceHistoryRepository, FundBalanceHistoryRepository>();

        _ = serviceCollection.AddScoped<IFundPlanBalanceEventQueryRepository, FundPlanBalanceEventQueryRepository>();
        _ = serviceCollection.AddScoped<IFundPlanRepository, FundPlanRepository>();
        _ = serviceCollection.AddScoped<IFundPlanQueryRepository, FundPlanQueryRepository>();
        _ = serviceCollection.AddScoped<IFundPlanTotalsHistoryRepository, FundPlanTotalsHistoryRepository>();

        _ = serviceCollection.AddScoped<ITransactionRepository, TransactionRepository>();
        _ = serviceCollection.AddScoped<ITransactionQueryRepository, TransactionQueryRepository>();
    }
}