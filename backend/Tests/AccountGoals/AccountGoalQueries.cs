using Domain.AccountGoals;
using Domain.AccountingPeriods;
using Microsoft.Extensions.DependencyInjection;
using Tests.Infrastructure;

namespace Tests.AccountGoals;

/// <summary>
/// Reads Account Goals directly from the test application's domain repository.
/// </summary>
internal sealed class AccountGoalQueries(FinancialTrackerApplicationFactory factory)
{
    /// <summary>
    /// Gets all Account Goals for an Account.
    /// </summary>
    public IReadOnlyCollection<AccountGoal> GetForAccount(Guid accountId)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        IAccountGoalRepository repository = scope.ServiceProvider.GetRequiredService<IAccountGoalRepository>();
        return repository.GetAllByAccount(new global::Domain.Accounts.AccountId(accountId));
    }

    /// <summary>
    /// Gets all Account Goals for an Accounting Period.
    /// </summary>
    public IReadOnlyCollection<AccountGoal> GetForPeriod(Guid accountingPeriodId)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        IAccountGoalRepository repository = scope.ServiceProvider.GetRequiredService<IAccountGoalRepository>();
        return repository.GetAllByAccountingPeriod(new AccountingPeriodId(accountingPeriodId));
    }
}
