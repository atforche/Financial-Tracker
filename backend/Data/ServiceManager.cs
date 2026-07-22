using Data.AccountingPeriods;
using Data.Accounts;
using Data.BalanceEvents;
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
        _ = serviceCollection.AddScoped<FinancialRangeQueryService>();
        _ = serviceCollection.AddScoped<BalanceEventQueryService>();

        _ = serviceCollection.AddScoped<IAccountingPeriodRepository, AccountingPeriodRepository>();
        _ = serviceCollection.AddScoped<AccountingPeriodRepository>();
        _ = serviceCollection.AddScoped<AccountingPeriods.AccountingPeriodQueryService>();
        _ = serviceCollection.AddScoped<IAccountingPeriodQueryRepository, AccountingPeriodQueryRepository>();

        _ = serviceCollection.AddScoped<IAccountingPeriodBalanceHistoryRepository, AccountingPeriodBalanceHistoryRepository>();
        _ = serviceCollection.AddScoped<AccountingPeriodBalanceHistoryRepository>();

        _ = serviceCollection.AddScoped<IAccountRepository, AccountRepository>();
        _ = serviceCollection.AddScoped<AccountRepository>();
        _ = serviceCollection.AddScoped<IAccountQueryRepository, AccountQueryRepository>();

        _ = serviceCollection.AddScoped<IAccountBalanceHistoryRepository, AccountBalanceHistoryRepository>();
        _ = serviceCollection.AddScoped<AccountBalanceHistoryRepository>();

        _ = serviceCollection.AddScoped<IFundRepository, FundRepository>();
        _ = serviceCollection.AddScoped<FundRepository>();
        _ = serviceCollection.AddScoped<IFundQueryRepository, FundQueryRepository>();

        _ = serviceCollection.AddScoped<IFundBalanceHistoryRepository, FundBalanceHistoryRepository>();
        _ = serviceCollection.AddScoped<FundBalanceHistoryRepository>();

        _ = serviceCollection.AddScoped<IFundPlanRepository, FundPlanRepository>();
        _ = serviceCollection.AddScoped<FundPlanRepository>();
        _ = serviceCollection.AddScoped<IFundPlanQueryRepository, FundPlanQueryRepository>();
        _ = serviceCollection.AddScoped<IFundPlanTotalsHistoryRepository, FundPlanTotalsHistoryRepository>();
        _ = serviceCollection.AddScoped<FundPlanTotalsHistoryRepository>();

        _ = serviceCollection.AddScoped<ITransactionRepository, TransactionRepository>();
        _ = serviceCollection.AddScoped<TransactionRepository>();
        _ = serviceCollection.AddScoped<TransactionModelMapper>();
        _ = serviceCollection.AddScoped<TransactionQueryService>();
    }
}