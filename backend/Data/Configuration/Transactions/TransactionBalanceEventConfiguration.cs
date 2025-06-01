using Domain.Accounts;
using Domain.Transactions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuration.Transactions;

/// <summary>
/// EF Core entity configuration for a <see cref="TransactionBalanceEvent"/>
/// </summary>
internal sealed class TransactionBalanceEventConfiguration : EntityConfiguration<TransactionBalanceEvent>
{
    /// <inheritdoc/>
    protected override void ConfigurePrivate(EntityTypeBuilder<TransactionBalanceEvent> builder)
    {
        builder.Navigation(transactionBalanceEvent => transactionBalanceEvent.Transaction).AutoInclude();

        builder.HasIndex(transactionBalanceEvent => new { transactionBalanceEvent.EventDate, transactionBalanceEvent.EventSequence }).IsUnique();

        builder.HasOne<Account>().WithMany().HasForeignKey(transactionBalanceEvent => transactionBalanceEvent.AccountId);
    }
}