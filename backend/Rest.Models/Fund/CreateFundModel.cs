namespace Rest.Models.Fund;

/// <summary>
/// REST model representing a request to create a Fund
/// </summary>
public class CreateFundModel
{
    /// <inheritdoc cref="Domain.Aggregates.Funds.Fund.Name"/>
    public required string Name { get; init; }
}