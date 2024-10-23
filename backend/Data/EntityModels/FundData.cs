using Domain.Entities;

namespace Data.EntityModels;

/// <summary>
/// Data model representing a Fund
/// </summary>
public class FundData : IEntityDataModel<FundData>
{
    /// <summary>
    /// Database primary key for this Fund
    /// </summary>
    public long PrimaryKey { get; set; }

    /// <inheritdoc cref="Fund.Id"/>
    public required Guid Id { get; set; }

    /// <inheritdoc cref="Fund.Name"/>
    public required string Name { get; set; }

    /// <inheritdoc/>
    public void Replace(FundData newModel)
    {
        Id = newModel.Id;
        Name = newModel.Name;
    }
}