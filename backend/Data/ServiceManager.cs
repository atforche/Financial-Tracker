using Data.AccountingPeriods;
using Data.Accounts;
using Data.BalanceEvents;
using Data.Funds;
using Data.Goals;
using Data.Transactions;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Goals;
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
        _ = serviceCollection.AddScoped<AccountingPeriodQueryService>();

        _ = serviceCollection.AddScoped<IAccountingPeriodBalanceHistoryRepository, AccountingPeriodBalanceHistoryRepository>();
        _ = serviceCollection.AddScoped<AccountingPeriodBalanceHistoryRepository>();

        _ = serviceCollection.AddScoped<IAccountRepository, AccountRepository>();
        _ = serviceCollection.AddScoped<AccountRepository>();
        _ = serviceCollection.AddScoped<AccountQueryService>();

        _ = serviceCollection.AddScoped<IAccountBalanceHistoryRepository, AccountBalanceHistoryRepository>();
        _ = serviceCollection.AddScoped<AccountBalanceHistoryRepository>();

        _ = serviceCollection.AddScoped<IFundRepository, FundRepository>();
        _ = serviceCollection.AddScoped<FundRepository>();
        _ = serviceCollection.AddScoped<FundQueryService>();

        _ = serviceCollection.AddScoped<IFundBalanceHistoryRepository, FundBalanceHistoryRepository>();
        _ = serviceCollection.AddScoped<FundBalanceHistoryRepository>();

        _ = serviceCollection.AddScoped<IGoalBalanceHistoryRepository, GoalBalanceHistoryRepository>();
        _ = serviceCollection.AddScoped<GoalBalanceHistoryRepository>();

        _ = serviceCollection.AddScoped<IAssignmentGoalRepository, AssignmentGoalRepository>();
        _ = serviceCollection.AddScoped<AssignmentGoalRepository>();
        _ = serviceCollection.AddScoped<ISpendingGoalRepository, SpendingGoalRepository>();
        _ = serviceCollection.AddScoped<SpendingGoalRepository>();
        _ = serviceCollection.AddScoped<GoalQueryService>();

        _ = serviceCollection.AddScoped<ITransactionRepository, TransactionRepository>();
        _ = serviceCollection.AddScoped<TransactionRepository>();
        _ = serviceCollection.AddScoped<TransactionModelMapper>();
        _ = serviceCollection.AddScoped<TransactionQueryService>();
    }
}