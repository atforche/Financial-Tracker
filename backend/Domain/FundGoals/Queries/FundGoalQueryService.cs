using Domain.AccountingPeriods;
using Domain.Funds;

namespace Domain.FundGoals.Queries;

/// <summary>
/// Service for querying Fund Goals.
/// </summary>
public sealed class FundGoalQueryService(IFundGoalQueryRepository fundGoalQueryRepository)
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
    /// Retrieves the Fund Goal for the specified Fund and Accounting Period, or null when it does not exist.
    /// </summary>
    public Task<FundGoal?> GetByFundAndAccountingPeriodAsync(
        Guid fundId,
        Guid? accountingPeriodId,
        CancellationToken cancellationToken = default) =>
        fundGoalQueryRepository.GetByFundAndAccountingPeriodAsync(
            new FundId(fundId),
            accountingPeriodId == null ? null : new AccountingPeriodId(accountingPeriodId.Value),
            cancellationToken);
}
