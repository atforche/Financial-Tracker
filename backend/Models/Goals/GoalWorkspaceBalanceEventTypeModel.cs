namespace Models.Goals;

/// <summary>
/// Type of balance event in a Goal workspace.
/// </summary>
public enum GoalWorkspaceBalanceEventTypeModel
{
    /// <summary>
    /// An amount assigned to the Fund.
    /// </summary>
    Assignment,

    /// <summary>
    /// An amount spent from the Fund.
    /// </summary>
    Spending,
}