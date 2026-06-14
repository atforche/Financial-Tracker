using Domain.AccountingPeriods;
using Domain.Funds;

namespace Domain.Goals;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="SpendingGoal"/>
/// </summary>
public interface ISpendingGoalRepository
{
    /// <summary>
    /// Gets all Spending Goals.
    /// </summary>
    IReadOnlyCollection<SpendingGoal> GetAll();

    /// <summary>
    /// Gets the Spending Goal with the specified ID.
    /// </summary>
    SpendingGoal GetById(SpendingGoalId id);

    /// <summary>
    /// Gets all Spending Goals associated with the specified Fund.
    /// </summary>
    IReadOnlyCollection<SpendingGoal> GetAllByFund(FundId fundId);

    /// <summary>
    /// Gets all Spending Goals associated with the specified Accounting Period.
    /// </summary>
    IReadOnlyCollection<SpendingGoal> GetAllByAccountingPeriod(AccountingPeriodId accountingPeriodId);

    /// <summary>
    /// Attempts to get the Spending Goal for the specified Fund and Accounting Period.
    /// </summary>
    SpendingGoal? GetByFundAndAccountingPeriod(FundId fundId, AccountingPeriodId? accountingPeriodId);

    /// <summary>
    /// Adds the provided Spending Goal to the repository
    /// </summary>
    void Add(SpendingGoal spendingGoal);

    /// <summary>
    /// Deletes the provided Spending Goal from the repository
    /// </summary>
    void Delete(SpendingGoal spendingGoal);
}