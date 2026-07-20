using Domain.AccountingPeriods;
using Domain.Funds;

namespace Domain.FundPlans;

/// <summary>
/// Interface representing methods to interact with Fund Plans.
/// </summary>
public interface IFundPlanRepository
{
    /// <summary>
    /// Gets the Fund Plan with the specified ID.
    /// </summary>
    FundPlan GetById(FundPlanId id);

    /// <summary>
    /// Gets all Fund Plans associated with the specified Fund.
    /// </summary>
    IReadOnlyCollection<FundPlan> GetAllByFund(FundId fundId);

    /// <summary>
    /// Gets all Fund Plans associated with the specified Accounting Period.
    /// </summary>
    IReadOnlyCollection<FundPlan> GetAllByAccountingPeriod(AccountingPeriodId? accountingPeriodId);

    /// <summary>
    /// Attempts to get the Fund Plan associated with the specified Fund and Accounting Period.
    /// </summary>
    FundPlan? GetByFundAndAccountingPeriod(FundId fundId, AccountingPeriodId? accountingPeriodId);

    /// <summary>
    /// Atomically attempts to add the provided Fund Plan when no plan exists for its Fund and Accounting Period.
    /// </summary>
    bool TryAdd(FundPlan fundPlan);

    /// <summary>
    /// Deletes the provided Fund Plan from the repository.
    /// </summary>
    void Delete(FundPlan fundPlan);
}