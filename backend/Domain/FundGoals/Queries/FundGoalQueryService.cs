using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Domain.Funds;

namespace Domain.FundGoals.Queries;

/// <summary>
/// Service for querying Fund Goals.
/// </summary>
public sealed class FundGoalQueryService(
    IFundGoalQueryRepository fundGoalQueryRepository,
    IFundGoalRepository fundGoalRepository,
    IAccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository,
    AccountingPeriodRangeService accountingPeriodRangeService)
{
    /// <summary>
    /// Retrieves Fund Goals matching the provided query.
    /// </summary>
    public Task<QueryPage<FundGoal>> GetAsync(FundGoalQuery query, CancellationToken cancellationToken = default) =>
        fundGoalQueryRepository.GetAsync(query, cancellationToken);

    /// <summary>
    /// Retrieves the Fund Goal with the specified ID, or null when it does not exist.
    /// </summary>
    public Task<FundGoal?> GetByIdAsync(Guid fundGoalId, CancellationToken cancellationToken = default) =>
        fundGoalQueryRepository.GetByIdAsync(new FundGoalId(fundGoalId), cancellationToken);

    /// <summary>
    /// Retrieves the fund's goals and their progress across an Accounting Period range, oldest first.
    /// </summary>
    public async Task<AccountingPeriodRangeQueryResult<IReadOnlyCollection<FundGoalPeriodProgress>>> GetHistoryAsync(
        Guid fundId,
        Guid startId,
        Guid endId,
        CancellationToken cancellationToken = default)
    {
        AccountingPeriodRangeResolution resolution = await accountingPeriodRangeService.ResolveAsync(startId, endId, cancellationToken);
        if (resolution.AccountingPeriods == null)
        {
            return new AccountingPeriodRangeQueryResult<IReadOnlyCollection<FundGoalPeriodProgress>>(null, resolution.Failure);
        }
        var goalsByPeriod = fundGoalRepository.GetAllByFund(new FundId(fundId))
            .Where(goal => goal.AccountingPeriod != null)
            .ToDictionary(goal => goal.AccountingPeriod!.Id);
        var historiesByPeriod = accountingPeriodBalanceHistoryRepository
            .GetForAccountingPeriods(resolution.AccountingPeriods.Select(period => period.Id).ToList())
            .ToDictionary(history => history.AccountingPeriod.Id);
        var results = new List<FundGoalPeriodProgress>();
        foreach (AccountingPeriod period in resolution.AccountingPeriods)
        {
            if (goalsByPeriod.TryGetValue(period.Id, out FundGoal? goal) &&
                FundGoalService.GetProgresses([goal], historiesByPeriod[period.Id]).TryGetValue(goal.Id, out FundGoalProgress? progress))
            {
                results.Add(new FundGoalPeriodProgress(goal, progress));
            }
        }
        return new AccountingPeriodRangeQueryResult<IReadOnlyCollection<FundGoalPeriodProgress>>(results, AccountingPeriodRangeQueryFailure.None);
    }
}
