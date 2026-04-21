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
    IEntityTypeConfiguration<AccountTransferTransaction>,
    IEntityTypeConfiguration<FundTransferTransaction>
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
            .HasValue<AccountTransferTransaction>(TransactionType.AccountTransfer)
            .HasValue<FundTransferTransaction>(TransactionType.FundTransfer);
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

        builder.OwnsMany(t => t.FundAssignments, fundAssignment =>
        {
            fundAssignment.ToTable("SpendingTransactionFundAssignments");
            fundAssignment.Property<int>("Id");
            fundAssignment.HasKey("Id");
            fundAssignment.Property(f => f.FundId)
                .HasConversion(fundId => fundId.Value, value => new FundId(value));
        });
        builder.Navigation(t => t.FundAssignments).AutoInclude();
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

        builder.OwnsMany(t => t.FundAssignments, fundAssignment =>
        {
            fundAssignment.ToTable("IncomeTransactionFundAssignments");
            fundAssignment.Property<int>("Id");
            fundAssignment.HasKey("Id");
            fundAssignment.Property(f => f.FundId)
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
    public void Configure(EntityTypeBuilder<AccountTransferTransaction> builder)
    {
        builder.Property(t => t.DebitAccountId)
            .HasColumnName("AccountTransferTransaction_DebitAccountId")
            .HasConversion(id => id.Value, value => new AccountId(value));
        builder.HasOne<Account>().WithMany().HasForeignKey(t => t.DebitAccountId);

        builder.Property(t => t.DebitPostedDate)
            .HasColumnName("AccountTransferTransaction_DebitPostedDate");

        builder.Property(t => t.CreditAccountId)
            .HasColumnName("AccountTransferTransaction_CreditAccountId")
            .HasConversion(id => id.Value, value => new AccountId(value));
        builder.HasOne<Account>().WithMany().HasForeignKey(t => t.CreditAccountId);

        builder.Property(t => t.CreditPostedDate)
            .HasColumnName("AccountTransferTransaction_CreditPostedDate");
    }

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FundTransferTransaction> builder)
    {
        builder.Property(t => t.DebitFundId)
            .HasColumnName("FundTransferTransaction_DebitFundId")
            .HasConversion(id => id.Value, value => new FundId(value));
        builder.HasOne<Fund>().WithMany().HasForeignKey(t => t.DebitFundId);

        builder.Property(t => t.CreditFundId)
            .HasColumnName("FundTransferTransaction_CreditFundId")
            .HasConversion(id => id.Value, value => new FundId(value));
        builder.HasOne<Fund>().WithMany().HasForeignKey(t => t.CreditFundId);
    }
}