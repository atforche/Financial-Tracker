using Domain.Aggregates.Accounts;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuration;

/// <summary>
/// EF Core configuration for the Account entity
/// </summary>
internal sealed class AccountEntityConfiguration : EntityConfigurationBase<Account>
{
    /// <inheritdoc/>
    protected override void ConfigurePrivate(EntityTypeBuilder<Account> builder)
    {
        builder.HasIndex(account => account.Name).IsUnique();
        builder.Property(account => account.Type).HasConversion<string>();
    }
}