using Domain.AccountingPeriods;
using Domain.Funds;

namespace Domain.FundPlans.Queries;

/// <summary>
/// Service for querying Fund Plans.
/// </summary>
public sealed class FundPlanQueryService(IFundPlanQueryRepository fundPlanQueryRepository)
{
    /// <summary>
    /// Retrieves Fund Plans matching the provided query.
    /// </summary>
    public Task<QueryPage<FundPlan>> GetAsync(FundPlanQuery query, CancellationToken cancellationToken = default) =>
        fundPlanQueryRepository.GetAsync(query, cancellationToken);

    /// <summary>
    /// Retrieves the Fund Plan with the specified ID, or null when it does not exist.
    /// </summary>
    public Task<FundPlan?> GetByIdAsync(Guid fundPlanId, CancellationToken cancellationToken = default) =>
        fundPlanQueryRepository.GetByIdAsync(new FundPlanId(fundPlanId), cancellationToken);

    /// <summary>
    /// Retrieves the Fund Plan for the specified Fund and Accounting Period, or null when it does not exist.
    /// </summary>
    public Task<FundPlan?> GetByFundAndAccountingPeriodAsync(
        Guid fundId,
        Guid? accountingPeriodId,
        CancellationToken cancellationToken = default) =>
        fundPlanQueryRepository.GetByFundAndAccountingPeriodAsync(
            new FundId(fundId),
            accountingPeriodId == null ? null : new AccountingPeriodId(accountingPeriodId.Value),
            cancellationToken);
}