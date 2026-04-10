using Domain.AccountingPeriods;

namespace Domain.Funds;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="FundGoal"/>
/// </summary>
public interface IFundGoalRepository
{
    /// <summary>
    /// Gets the Fund Goal with the specified ID.
    /// </summary>
    FundGoal GetById(FundGoalId id);

    /// <summary>
    /// Gets all Fund Goals associated with the specified Fund.
    /// </summary>
    IReadOnlyCollection<FundGoal> GetAllByFund(FundId fundId);

    /// <summary>
    /// Gets all Fund Goals associated with the specified Accounting Period.
    /// </summary>
    IReadOnlyCollection<FundGoal> GetAllByAccountingPeriod(AccountingPeriodId accountingPeriodId);

    /// <summary>
    /// Attempts to get the Fund Goal for the specified Fund and Accounting Period.
    /// </summary>
    FundGoal? GetByFundAndAccountingPeriod(FundId fundId, AccountingPeriodId accountingPeriodId);

    /// <summary>
    /// Adds the provided Fund Goal to the repository
    /// </summary>
    void Add(FundGoal fundGoal);

    /// <summary>
    /// Deletes the provided Fund Goal from the repository
    /// </summary>
    void Delete(FundGoal fundGoal);
}
