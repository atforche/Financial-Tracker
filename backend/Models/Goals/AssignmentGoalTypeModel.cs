namespace Models.Goals;

/// <summary>
/// Enum representing the supported assignment goal behaviors.
/// </summary>
public enum AssignmentGoalTypeModel
{
    /// <summary>
    /// Target a specific balance after accounting for the period opening balance.
    /// </summary>
    MonthlyTarget,

    /// <summary>
    /// Target a fixed contribution amount during the period regardless of the opening balance.
    /// </summary>
    RecurringContribution,
}