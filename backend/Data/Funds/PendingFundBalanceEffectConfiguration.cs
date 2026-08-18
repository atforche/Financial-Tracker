using Domain.Funds;
using Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Funds;

/// <summary>
/// EF Core configuration for <see cref="PendingFundBalanceEffect"/>.
/// </summary>
internal sealed class PendingFundBalanceEffectConfiguration : IEntityTypeConfiguration<PendingFundBalanceEffect>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<PendingFundBalanceEffect> builder)
    {
        builder.HasKey(effect => effect.Id);
        builder.Property(effect => effect.Id).HasConversion(id => id.Value, value => new PendingFundBalanceEffectId(value));

        builder.Property<FundId>("FundId").IsRequired().HasConversion(id => id.Value, value => new FundId(value));
        builder.HasOne(effect => effect.Fund).WithMany().HasForeignKey("FundId");
        builder.Navigation(effect => effect.Fund).AutoInclude();

        builder.Property(effect => effect.TransactionId).HasConversion(id => id.Value, value => new TransactionId(value));

        builder.HasIndex("FundId");
        builder.HasIndex(nameof(PendingFundBalanceEffect.TransactionId), "FundId").IsUnique();
    }
}
