using Domain.AccountingPeriods;
using Domain.Funds;

namespace Domain.Goals;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="AssignmentGoal"/>
/// </summary>
public interface IAssignmentGoalRepository
{
    /// <summary>
    /// Gets all Assignment Goals.
    /// </summary>
    IReadOnlyCollection<AssignmentGoal> GetAll();

    /// <summary>
    /// Gets the Assignment Goal with the specified ID.
    /// </summary>
    AssignmentGoal GetById(AssignmentGoalId id);

    /// <summary>
    /// Gets all Assignment Goals associated with the specified Fund.
    /// </summary>
    IReadOnlyCollection<AssignmentGoal> GetAllByFund(FundId fundId);

    /// <summary>
    /// Gets all Assignment Goals associated with the specified Accounting Period.
    /// </summary>
    IReadOnlyCollection<AssignmentGoal> GetAllByAccountingPeriod(AccountingPeriodId accountingPeriodId);

    /// <summary>
    /// Attempts to get the Assignment Goal for the specified Fund and Accounting Period.
    /// </summary>
    AssignmentGoal? GetByFundAndAccountingPeriod(FundId fundId, AccountingPeriodId? accountingPeriodId);

    /// <summary>
    /// Adds the provided Assignment Goal to the repository
    /// </summary>
    void Add(AssignmentGoal assignmentGoal);

    /// <summary>
    /// Deletes the provided Assignment Goal from the repository
    /// </summary>
    void Delete(AssignmentGoal assignmentGoal);
}