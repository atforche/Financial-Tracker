namespace RestApi.Models.Fund;

/// <summary>
/// REST model representing a request to create a Fund
/// </summary>
public class CreateFundModel
{
    /// <inheritdoc cref="Domain.Entities.Fund.Name"/>
    public required string Name { get; init; }
}