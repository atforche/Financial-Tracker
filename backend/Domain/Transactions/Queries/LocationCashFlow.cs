namespace Domain.Transactions.Queries;

/// <summary>
/// Directional money totals for the selected Locations in a range.
/// </summary>
public sealed record LocationCashFlow(decimal Incoming, decimal Outgoing);