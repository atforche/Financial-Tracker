using Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuration.Transactions;

/// <summary>
/// EF Core entity configuration for a <see cref="TransactionBalanceEventPart"/>
/// </summary>
internal sealed class TransactionBalanceEventPartConfiguration : IEntityTypeConfiguration<TransactionBalanceEventPart>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<TransactionBalanceEventPart> builder)
    {
        builder.HasKey(transactionBalanceEventPart => transactionBalanceEventPart.Id);
        builder.Property(transactionBalanceEventPart => transactionBalanceEventPart.Id)
            .HasConversion(transactionBalanceEventPartId => transactionBalanceEventPartId.Value, value => new TransactionBalanceEventPartId(value));

        builder.Navigation(transactionBalanceEventPart => transactionBalanceEventPart.TransactionBalanceEvent).AutoInclude();

        builder.Property(transactionBalanceEventPart => transactionBalanceEventPart.Type).HasConversion<string>();
    }
}