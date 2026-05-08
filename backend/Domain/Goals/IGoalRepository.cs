using Domain.AccountingPeriods;
using Domain.Funds;

namespace Domain.Goals;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="Goal"/>
/// </summary>
public interface IGoalRepository
{
    /// <summary>
    /// Gets the Goal with the specified ID.
    /// </summary>
    Goal GetById(GoalId id);

    /// <summary>
    /// Gets all Goals associated with the specified Fund.
    /// </summary>
    IReadOnlyCollection<Goal> GetAllByFund(FundId fundId);

    /// <summary>
    /// Gets all Goals associated with the specified Accounting Period.
    /// </summary>
    IReadOnlyCollection<Goal> GetAllByAccountingPeriod(AccountingPeriodId accountingPeriodId);

    /// <summary>
    /// Attempts to get the Goal for the specified Fund and Accounting Period.
    /// </summary>
    Goal? GetByFundAndAccountingPeriod(FundId fundId, AccountingPeriodId accountingPeriodId);

    /// <summary>
    /// Adds the provided Goal to the repository
    /// </summary>
    void Add(Goal goal);

    /// <summary>
    /// Deletes the provided Goal from the repository
    /// </summary>
    void Delete(Goal goal);
}