using System.Text.Json.Serialization;

namespace RestApi.Models.Fund;

/// <summary>
/// REST model representing a Fund
/// </summary>
public class FundModel
{
    /// <inheritdoc cref="Domain.Aggregates.EntityBase.Id"/>
    public Guid Id { get; init; }

    /// <inheritdoc cref="Domain.Aggregates.Funds.Fund.Name"/>
    public string Name { get; init; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    [JsonConstructor]
    public FundModel(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="fund">Fund entity to build this Fund REST model from</param>
    public FundModel(Domain.Aggregates.Funds.Fund fund)
    {
        Id = fund.Id.ExternalId;
        Name = fund.Name;
    }
}