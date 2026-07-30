using Domain.AccountingPeriods;
using Domain.Funds;

namespace Domain.FundGoals.Queries;

/// <summary>
/// Defines read-only persistence operations for Fund Goals.
/// </summary>
public interface IFundGoalQueryRepository
{
    /// <summary>
    /// Retrieves the Fund Goal with the specified ID, or null when it does not exist.
    /// </summary>
    Task<FundGoal?> GetByIdAsync(FundGoalId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves Fund Goals matching the provided query.
    /// </summary>
    Task<QueryPage<FundGoal>> GetAsync(FundGoalQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the Fund Goal for the specified Fund and Accounting Period, or null when it does not exist.
    /// </summary>
    Task<FundGoal?> GetByFundAndAccountingPeriodAsync(
        FundId fundId,
        AccountingPeriodId? accountingPeriodId,
        CancellationToken cancellationToken = default);
}