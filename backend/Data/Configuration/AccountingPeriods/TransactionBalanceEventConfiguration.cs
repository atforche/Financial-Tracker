using Domain.AccountingPeriods;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuration.AccountingPeriods;

/// <summary>
/// EF Core entity configuration for a <see cref="TransactionBalanceEvent"/>
/// </summary>
internal sealed class TransactionBalanceEventConfiguration : EntityConfiguration<TransactionBalanceEvent>
{
    /// <inheritdoc/>
    protected override void ConfigurePrivate(EntityTypeBuilder<TransactionBalanceEvent> builder)
    {
        builder.Property(transactionBalanceEvent => transactionBalanceEvent.AccountingPeriodId)
            .HasConversion(accountingPeriodId => accountingPeriodId.Value, value => new AccountingPeriodId(value));
        builder.HasOne<AccountingPeriod>()
            .WithMany()
            .HasForeignKey(transactionBalanceEvent => transactionBalanceEvent.AccountingPeriodId);

        builder.HasIndex(transactionBalanceEvent => new { transactionBalanceEvent.EventDate, transactionBalanceEvent.EventSequence }).IsUnique();

        builder.HasOne(transactionBalanceEvent => transactionBalanceEvent.Account).WithMany().HasForeignKey("AccountId");
        builder.Navigation(transactionBalanceEvent => transactionBalanceEvent.Account).IsRequired().AutoInclude();
    }
}