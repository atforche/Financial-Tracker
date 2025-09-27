using Domain.Funds;

namespace Rest.Models.Funds;

/// <summary>
/// REST model representing a request to create or update a Fund
/// </summary>
public class CreateOrUpdateFundModel
{
    /// <inheritdoc cref="Fund.Name"/>
    public required string Name { get; init; }

    /// <inheritdoc cref="Fund.Description"/>
    public required string Description { get; init; }
}