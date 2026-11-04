using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Domain.Accounts;

namespace Domain.AccountGoals.Queries;

/// <summary>
/// Service for querying Account Goals.
/// </summary>
public sealed class AccountGoalQueryService(
    IAccountGoalQueryRepository accountGoalQueryRepository,
    IAccountGoalRepository accountGoalRepository,
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository,
    AccountingPeriodRangeService accountingPeriodRangeService)
{
    /// <summary>
    /// Retrieves Account Goals matching the provided query.
    /// </summary>
    public Task<QueryPage<AccountGoal>> GetAsync(AccountGoalQuery query, CancellationToken cancellationToken = default) =>
        accountGoalQueryRepository.GetAsync(query, cancellationToken);

    /// <summary>
    /// Retrieves the Account Goal with the specified ID, or null when it does not exist.
    /// </summary>
    public Task<AccountGoal?> GetByIdAsync(Guid accountGoalId, CancellationToken cancellationToken = default) =>
        accountGoalQueryRepository.GetByIdAsync(new AccountGoalId(accountGoalId), cancellationToken);

    /// <summary>
    /// Retrieves the Account Goal for the specified Account and Accounting Period, or null when it does not exist.
    /// </summary>
    public Task<AccountGoal?> GetByAccountAndAccountingPeriodAsync(
        Guid accountId,
        Guid? accountingPeriodId,
        CancellationToken cancellationToken = default) =>
        accountGoalQueryRepository.GetByAccountAndAccountingPeriodAsync(
            new AccountId(accountId),
            accountingPeriodId == null ? null : new AccountingPeriodId(accountingPeriodId.Value),
            cancellationToken);

    /// <summary>
    /// Retrieves the account's goals and their progress across an Accounting Period range, oldest first.
    /// </summary>
    public async Task<AccountingPeriodRangeQueryResult<IReadOnlyCollection<AccountGoalPeriodProgress>>> GetHistoryAsync(
        Guid accountId,
        Guid startId,
        Guid endId,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriodRangeResolution resolution = await accountingPeriodRangeService.ResolveAsync(startId, endId, cancellationToken);
        if (resolution.AccountingPeriods == null)
        {
            return new AccountingPeriodRangeQueryResult<IReadOnlyCollection<AccountGoalPeriodProgress>>(null, resolution.Failure);
        }
        var goalsByPeriod = accountGoalRepository.GetAllByAccount(new AccountId(accountId))
            .Where(goal => goal.AccountingPeriod != null)
            .ToDictionary(goal => goal.AccountingPeriod!.Id);
        var historiesByPeriod = accountingPeriodBalanceHistoryRepository
            .GetForAccountingPeriods(resolution.AccountingPeriods.Select(period => period.Id).ToList())
            .ToDictionary(history => history.AccountingPeriod.Id);
        var results = new List<AccountGoalPeriodProgress>();
        foreach (AccountingPeriod period in resolution.AccountingPeriods)
        {
            if (goalsByPeriod.TryGetValue(period.Id, out AccountGoal? goal) &&
                AccountGoalService.GetProgresses([goal], historiesByPeriod[period.Id]).TryGetValue(goal.Id, out AccountGoalProgress? progress))
            {
                results.Add(new AccountGoalPeriodProgress(goal, progress));
            }
        }
        return new AccountingPeriodRangeQueryResult<IReadOnlyCollection<AccountGoalPeriodProgress>>(results, AccountingPeriodRangeQueryFailure.None);
    }
}
