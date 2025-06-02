using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Actions;
using Domain.ChangeInValues;
using Domain.FundConversions;
using Domain.Funds;
using Domain.Services;
using Domain.Transactions;
using Microsoft.Extensions.DependencyInjection;

namespace Domain;

/// <summary>
/// Static class for managing all the DI services required for the Domain assembly
/// </summary>
public static class ServiceManager
{
    /// <summary>
    /// Registers all the Domain DI services in the provided service collection
    /// </summary>
    /// <param name="serviceCollection">Service Collection</param>
    public static void Register(IServiceCollection serviceCollection)
    {
        _ = serviceCollection.AddScoped<AccountingPeriodIdFactory>();
        _ = serviceCollection.AddScoped<AddAccountingPeriodAction>();
        _ = serviceCollection.AddScoped<CloseAccountingPeriodAction>();

        _ = serviceCollection.AddScoped<AccountFactory>();
        _ = serviceCollection.AddScoped<AccountIdFactory>();
        _ = serviceCollection.AddScoped<AccountAddedBalanceEventFactory>();
        _ = serviceCollection.AddScoped<AccountBalanceService>();

        _ = serviceCollection.AddScoped<ChangeInValueFactory>();
        _ = serviceCollection.AddScoped<ChangeInValueIdFactory>();

        _ = serviceCollection.AddScoped<AddFundAction>();
        _ = serviceCollection.AddScoped<FundIdFactory>();

        _ = serviceCollection.AddScoped<FundConversionFactory>();
        _ = serviceCollection.AddScoped<FundConversionIdFactory>();

        _ = serviceCollection.AddScoped<PostTransactionAction>();
        _ = serviceCollection.AddScoped<TransactionFactory>();
        _ = serviceCollection.AddScoped<TransactionBalanceEventFactory>();
        _ = serviceCollection.AddScoped<TransactionIdFactory>();
    }
}