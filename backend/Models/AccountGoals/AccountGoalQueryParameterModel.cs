namespace Models.AccountGoals;

/// <summary>
/// Query parameters for retrieving Account Goals.
/// </summary>
public sealed class AccountGoalQueryParameterModel : PaginationModel
{
    /// <summary>
    /// Gets the optional Account Goal filter.
    /// </summary>
    public AccountGoalFilterModel? Filter { get; init; }

    /// <summary>
    /// Gets the optional Account Goal ordering.
    /// </summary>
    public AccountGoalSortModel? Sort { get; init; }
}
