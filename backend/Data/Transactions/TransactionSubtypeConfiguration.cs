using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Transactions;

/// <summary>
/// EF Core entity configurations for <see cref="Transaction"/> subtypes
/// </summary>
internal sealed class TransactionSubtypeConfiguration :
    IEntityTypeConfiguration<SpendingTransaction>,
    IEntityTypeConfiguration<SpendingTransferTransaction>,
    IEntityTypeConfiguration<IncomeTransaction>,
    IEntityTypeConfiguration<IncomeTransferTransaction>,
    IEntityTypeConfiguration<TransferTransaction>,
    IEntityTypeConfiguration<RefundTransaction>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<SpendingTransaction> builder)
    {
        builder.Property(t => t.AccountId)
            .HasConversion(id => id.Value, value => new AccountId(value));
        builder.HasOne<Account>().WithMany().HasForeignKey(t => t.AccountId);

        builder.OwnsMany(t => t.FundAmounts, fundAmount =>
        {
            fundAmount.ToTable("SpendingTransactionFundAmounts");
            fundAmount.Property<int>("Id");
            fundAmount.HasKey("Id");
            fundAmount.Property(f => f.FundId)
                .HasConversion(fundId => fundId.Value, value => new FundId(value));
        });
        builder.Navigation(t => t.FundAmounts).AutoInclude();
    }

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<SpendingTransferTransaction> builder)
    {
        builder.Property(t => t.CreditAccountId)
            .HasConversion(id => id.Value, value => new AccountId(value));
        builder.HasOne<Account>().WithMany().HasForeignKey(t => t.CreditAccountId);
    }

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<IncomeTransaction> builder)
    {
        builder.Property(t => t.AccountId)
            .HasConversion(id => id.Value, value => new AccountId(value));
        builder.HasOne<Account>().WithMany().HasForeignKey(t => t.AccountId);

        builder.Property(t => t.GeneratedByAccountId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value,
                value => value == null ? null : new AccountId(value.Value));
    }

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<IncomeTransferTransaction> builder)
    {
        builder.Property(t => t.DebitAccountId)
            .HasConversion(id => id.Value, value => new AccountId(value));
        builder.HasOne<Account>().WithMany().HasForeignKey(t => t.DebitAccountId);
    }

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<TransferTransaction> builder)
    {
        builder.Property(t => t.DebitAccountId)
            .HasConversion(id => id.Value, value => new AccountId(value));
        builder.HasOne<Account>().WithMany().HasForeignKey(t => t.DebitAccountId);

        builder.Property(t => t.CreditAccountId)
            .HasConversion(id => id.Value, value => new AccountId(value));
        builder.HasOne<Account>().WithMany().HasForeignKey(t => t.CreditAccountId);
    }

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<RefundTransaction> builder)
    {
        builder.HasOne(t => t.Transaction).WithMany().HasForeignKey("RefundTransactionId");
        builder.Navigation(t => t.Transaction).AutoInclude();
    }
}
