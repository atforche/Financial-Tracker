namespace Domain.AccountGoals.Queries;

/// <summary>
/// Defines read-only persistence operations for Account Goals.
/// </summary>
public interface IAccountGoalQueryRepository
{
    /// <summary>
    /// Retrieves the Account Goal with the specified ID, or null when it does not exist.
    /// </summary>
    Task<AccountGoal?> GetByIdAsync(AccountGoalId id, CancellationToken cancellationToken = default);
}
