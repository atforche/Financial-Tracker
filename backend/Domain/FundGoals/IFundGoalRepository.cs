using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Funds;

namespace Domain.FundGoals;

/// <summary>
/// Interface representing methods to interact with Fund Goals.
/// </summary>
public interface IFundGoalRepository
{
    /// <summary>
    /// Gets the Fund Goal with the specified ID.
    /// </summary>
    FundGoal GetById(FundGoalId id);

    /// <summary>
    /// Attempts to get the Fund Goal with the specified ID.
    /// </summary>
    bool TryGetById(Guid id, [NotNullWhen(true)] out FundGoal? fundGoal);

    /// <summary>
    /// Gets all Fund Goals associated with the specified Fund.
    /// </summary>
    IReadOnlyCollection<FundGoal> GetAllByFund(FundId fundId);

    /// <summary>
    /// Gets all Fund Goals associated with the specified Accounting Period.
    /// </summary>
    IReadOnlyCollection<FundGoal> GetAllByAccountingPeriod(AccountingPeriodId? accountingPeriodId);

    /// <summary>
    /// Attempts to get the Fund Goal associated with the specified Fund and Accounting Period.
    /// </summary>
    FundGoal? GetByFundAndAccountingPeriod(FundId fundId, AccountingPeriodId? accountingPeriodId);

    /// <summary>
    /// Atomically attempts to add the provided Fund Goal when no Fund Goal exists for its Fund and Accounting Period.
    /// </summary>
    bool TryAdd(FundGoal fundGoal);

    /// <summary>
    /// Deletes the provided Fund Goal from the repository.
    /// </summary>
    void Delete(FundGoal fundGoal);
}
