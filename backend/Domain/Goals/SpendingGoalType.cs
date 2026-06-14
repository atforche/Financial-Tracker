namespace Domain.Goals;

/// <summary>
/// Enum representing the supported spending goal behaviors.
/// </summary>
public enum SpendingGoalType
{
    /// <summary>
    /// Spending is on track so long as the fund balance remains non-negative.
    /// </summary>
    Standard,

    /// <summary>
    /// Spending is on track only when all available funds have been applied to the debt by period end.
    /// </summary>
    Debt,
}