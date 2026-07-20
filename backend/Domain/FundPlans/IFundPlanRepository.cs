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
    /// Attempts to get the Fund Plan associated with the specified Fund.
    /// </summary>
    FundPlan? GetByFund(FundId fundId);

    /// <summary>
    /// Atomically attempts to add the provided Fund Plan when no plan exists for its Fund.
    /// </summary>
    bool TryAdd(FundPlan fundPlan);

    /// <summary>
    /// Deletes the provided Fund Plan from the repository.
    /// </summary>
    void Delete(FundPlan fundPlan);
}