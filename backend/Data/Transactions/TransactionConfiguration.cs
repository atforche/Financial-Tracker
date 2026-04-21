using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Transactions;

/// <summary>
/// EF Core entity configuration for <see cref="Transaction"/> and its subtypes.
/// </summary>
internal sealed class TransactionConfiguration :
    IEntityTypeConfiguration<Transaction>,
    IEntityTypeConfiguration<SpendingTransaction>,
    IEntityTypeConfiguration<SpendingTransferTransaction>,
    IEntityTypeConfiguration<IncomeTransaction>,
    IEntityTypeConfiguration<IncomeTransferTransaction>,
    IEntityTypeConfiguration<TransferTransaction>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(transaction => transaction.Id);
        builder.Property(transaction => transaction.Id).HasConversion(transactionId => transactionId.Value, value => new TransactionId(value));

        builder.Property(transaction => transaction.AccountingPeriodId)
            .HasConversion(accountingPeriodId => accountingPeriodId.Value, value => new AccountingPeriodId(value));

        builder.HasDiscriminator(transaction => transaction.Type)
            .HasValue<SpendingTransaction>(TransactionType.Spending)
            .HasValue<SpendingTransferTransaction>(TransactionType.SpendingTransfer)
            .HasValue<IncomeTransaction>(TransactionType.Income)
            .HasValue<IncomeTransferTransaction>(TransactionType.IncomeTransfer)
            .HasValue<TransferTransaction>(TransactionType.Transfer);
        // .HasValue<RefundTransaction>(TransactionType.Refund);
    }

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<SpendingTransaction> builder)
    {
        builder.Property(t => t.AccountId)
            .HasColumnName("SpendingTransaction_AccountId")
            .HasConversion(id => id.Value, value => new AccountId(value));
        builder.HasOne<Account>().WithMany().HasForeignKey(t => t.AccountId);

        builder.Property(t => t.PostedDate)
            .HasColumnName("SpendingTransaction_PostedDate");

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
            .HasColumnName("SpendingTransferTransaction_CreditAccountId")
            .HasConversion(id => id.Value, value => new AccountId(value));
        builder.HasOne<Account>().WithMany().HasForeignKey(t => t.CreditAccountId);

        builder.Property(t => t.CreditPostedDate)
            .HasColumnName("SpendingTransferTransaction_CreditPostedDate");
    }

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<IncomeTransaction> builder)
    {
        builder.Property(t => t.AccountId)
            .HasColumnName("IncomeTransaction_AccountId")
            .HasConversion(id => id.Value, value => new AccountId(value));
        builder.HasOne<Account>().WithMany().HasForeignKey(t => t.AccountId);

        builder.Property(t => t.PostedDate)
            .HasColumnName("IncomeTransaction_PostedDate");

        builder.OwnsMany(t => t.FundAmounts, fundAmount =>
        {
            fundAmount.ToTable("IncomeTransactionFundAmounts");
            fundAmount.Property<int>("Id");
            fundAmount.HasKey("Id");
            fundAmount.Property(f => f.FundId)
                .HasConversion(fundId => fundId.Value, value => new FundId(value));
        });

        builder.Property(t => t.GeneratedByAccountId)
            .HasColumnName("IncomeTransaction_GeneratedByAccountId")
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value,
                value => value == null ? null : new AccountId(value.Value));
    }

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<IncomeTransferTransaction> builder)
    {
        builder.Property(t => t.DebitAccountId)
            .HasColumnName("IncomeTransferTransaction_DebitAccountId")
            .HasConversion(id => id.Value, value => new AccountId(value));
        builder.HasOne<Account>().WithMany().HasForeignKey(t => t.DebitAccountId);

        builder.Property(t => t.DebitPostedDate)
            .HasColumnName("IncomeTransferTransaction_DebitPostedDate");
    }

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<TransferTransaction> builder)
    {
        builder.Property(t => t.DebitAccountId)
            .HasColumnName("TransferTransaction_DebitAccountId")
            .HasConversion(id => id.Value, value => new AccountId(value));
        builder.HasOne<Account>().WithMany().HasForeignKey(t => t.DebitAccountId);

        builder.Property(t => t.DebitPostedDate)
            .HasColumnName("TransferTransaction_DebitPostedDate");

        builder.Property(t => t.CreditAccountId)
            .HasColumnName("TransferTransaction_CreditAccountId")
            .HasConversion(id => id.Value, value => new AccountId(value));
        builder.HasOne<Account>().WithMany().HasForeignKey(t => t.CreditAccountId);

        builder.Property(t => t.CreditPostedDate)
            .HasColumnName("TransferTransaction_CreditPostedDate");
    }
}