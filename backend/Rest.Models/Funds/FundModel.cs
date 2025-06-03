using System.Text.Json.Serialization;
using Domain.Funds;

namespace Rest.Models.Funds;

/// <summary>
/// REST model representing a <see cref="Fund"/>
/// </summary>
public class FundModel
{
    /// <inheritdoc cref="FundId"/>
    public Guid Id { get; init; }

    /// <inheritdoc cref="Fund.Name"/>
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
    public FundModel(Fund fund)
    {
        Id = fund.Id.Value;
        Name = fund.Name;
    }
}