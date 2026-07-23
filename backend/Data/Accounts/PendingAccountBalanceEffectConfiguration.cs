using Domain.Accounts;
using Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Accounts;

/// <summary>
/// EF Core entity configuration for a <see cref="PendingAccountBalanceEffect"/>.
/// </summary>
internal sealed class PendingAccountBalanceEffectConfiguration : IEntityTypeConfiguration<PendingAccountBalanceEffect>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<PendingAccountBalanceEffect> builder)
    {
        builder.HasKey(effect => effect.Id);
        builder.Property(effect => effect.Id).HasConversion(id => id.Value, value => new PendingAccountBalanceEffectId(value));

        builder.Property<AccountId>("AccountId")
            .IsRequired()
            .HasConversion(id => id.Value, value => new AccountId(value));
        builder.HasOne(effect => effect.Account).WithMany().HasForeignKey("AccountId");
        builder.Navigation(effect => effect.Account).AutoInclude();

        builder.Property(effect => effect.TransactionId)
            .HasConversion(id => id.Value, value => new TransactionId(value));
        builder.HasIndex("AccountId");
        builder.HasIndex(nameof(PendingAccountBalanceEffect.TransactionId), "AccountId").IsUnique();
    }
}