namespace Tests.FundGoals;

/// <summary>
/// Fund goal availability values exposed by the application.
/// </summary>
internal sealed record FundGoalAvailabilitySnapshot(decimal Posted, decimal IncludingPending);