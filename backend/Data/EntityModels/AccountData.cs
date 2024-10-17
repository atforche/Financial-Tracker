using Domain.Entities;

namespace Data.EntityModels;

/// <summary>
/// Data model representing an Account
/// </summary>
public class AccountData : IEntityDataModel<AccountData>
{
    /// <summary>
    /// Database primary key for this Account
    /// </summary>
    public long PrimaryKey { get; set; }

    /// <inheritdoc cref="Account.Id"/>
    public required Guid Id { get; set; }

    /// <inheritdoc cref="Account.Name"/>
    public required string Name { get; set; }

    /// <inheritdoc cref="Account.Type"/>
    public required AccountType Type { get; set; }

    /// <inheritdoc/>
    public void Replace(AccountData newModel)
    {
        Id = newModel.Id;
        Name = newModel.Name;
        Type = newModel.Type;
    }
}