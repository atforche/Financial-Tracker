using Domain.Accounts;
using Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuration.Transactions;

/// <summary>
/// EF Core entity configuration for a <see cref="TransactionBalanceEvent"/>
/// </summary>
internal sealed class TransactionBalanceEventConfiguration : IEntityTypeConfiguration<TransactionBalanceEvent>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<TransactionBalanceEvent> builder)
    {
        builder.HasKey(transactionBalanceEvent => transactionBalanceEvent.Id);
        builder.Property(transactionBalanceEvent => transactionBalanceEvent.Id)
            .HasConversion(transactionBalanceEventId => transactionBalanceEventId.Value, value => new TransactionBalanceEventId(value));

        builder.Navigation(transactionBalanceEvent => transactionBalanceEvent.Transaction).AutoInclude();

        builder.HasIndex(transactionBalanceEvent => new { transactionBalanceEvent.EventDate, transactionBalanceEvent.EventSequence }).IsUnique();

        builder.HasOne<Account>().WithMany().HasForeignKey(transactionBalanceEvent => transactionBalanceEvent.AccountId);
    }
}