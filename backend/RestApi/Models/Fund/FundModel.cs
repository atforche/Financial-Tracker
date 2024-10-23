namespace RestApi.Models.Fund;

/// <summary>
/// REST model representing a Fund
/// </summary>
public class FundModel
{
    /// <inheritdoc cref="Domain.Entities.Fund.Id"/>
    public required Guid Id { get; init; }

    /// <inheritdoc cref="Domain.Entities.Fund.Name"/>
    public required string Name { get; init; }

    /// <summary>
    /// Converts the Fund domain entity into a Fund REST model
    /// </summary>
    /// <param name="fund">Fund domain entity to be converted</param>
    /// <returns>The converted Fund REST model</returns>
    internal static FundModel ConvertEntityToModel(Domain.Entities.Fund fund) =>
        new FundModel
        {
            Id = fund.Id,
            Name = fund.Name,
        };
}