namespace Tests.Funds;

/// <summary>
/// Stable reference to a fund and its goal created through the test context.
/// </summary>
internal sealed record FundHandle(Guid Id, string Name, FundGoalHandle Goal);
