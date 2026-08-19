namespace Models.Locations;

/// <summary>
/// API request to consolidate a duplicate Location.
/// </summary>
public sealed class ConsolidateLocationModel
{
    /// <summary>
    /// ID of the Location that will survive consolidation.
    /// </summary>
    public required Guid TargetLocationId { get; init; }
}
