using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Transactions;

/// <summary>
/// EF Core entity configuration for <see cref="Transaction"/> and its subtypes.
/// </summary>
internal sealed class TransactionConfiguration :
    IEntityTypeConfiguration<Transaction>,
    IEntityTypeConfiguration<SpendingTransaction>,
    IEntityTypeConfiguration<IncomeTransaction>,
    IEntityTypeConfiguration<AccountTransaction>,
    IEntityTypeConfiguration<FundTransaction>
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
            .HasValue<IncomeTransaction>(TransactionType.Income)
            .HasValue<AccountTransaction>(TransactionType.Account)
            .HasValue<FundTransaction>(TransactionType.Fund);
    }

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<SpendingTransaction> builder)
    {
        builder.Property(t => t.DebitAccountId)
            .HasColumnName("SpendingTransaction_DebitAccountId")
            .HasConversion(id => id.Value, value => new AccountId(value));
        builder.HasOne<Account>().WithMany().HasForeignKey(t => t.DebitAccountId);

        builder.Property(t => t.DebitPostedDate)
            .HasColumnName("SpendingTransaction_DebitPostedDate");

        builder.Property(t => t.CreditAccountId)
            .HasColumnName("SpendingTransaction_CreditAccountId")
            .HasConversion(id => id == null ? (Guid?)null : id.Value, value => value == null ? null : new AccountId(value.Value));
        builder.HasOne<Account>().WithMany().HasForeignKey(t => t.CreditAccountId);

        builder.Property(t => t.CreditPostedDate)
            .HasColumnName("SpendingTransaction_CreditPostedDate");

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
    public void Configure(EntityTypeBuilder<IncomeTransaction> builder)
    {
        builder.Property(t => t.CreditAccountId)
            .HasColumnName("IncomeTransaction_CreditAccountId")
            .HasConversion(id => id.Value, value => new AccountId(value));
        builder.HasOne<Account>().WithMany().HasForeignKey(t => t.CreditAccountId);

        builder.Property(t => t.CreditPostedDate)
            .HasColumnName("IncomeTransaction_CreditPostedDate");

        builder.Property(t => t.DebitAccountId)
            .HasColumnName("IncomeTransaction_DebitAccountId")
            .HasConversion(id => id == null ? (Guid?)null : id.Value, value => value == null ? null : new AccountId(value.Value));
        builder.HasOne<Account>().WithMany().HasForeignKey(t => t.DebitAccountId);

        builder.Property(t => t.DebitPostedDate)
            .HasColumnName("IncomeTransaction_DebitPostedDate");

        builder.OwnsMany(t => t.FundAssignments, fundAssignment =>
        {
            fundAssignment.ToTable("IncomeTransactionFundAssignments");
            fundAssignment.Property<int>("Id");
            fundAssignment.HasKey("Id");
            fundAssignment.Property(f => f.FundId)
                .HasConversion(fundId => fundId.Value, value => new FundId(value));
        });
    }

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<AccountTransaction> builder)
    {
        builder.Property(t => t.DebitAccountId)
            .HasColumnName("AccountTransaction_DebitAccountId")
            .HasConversion(id => id == null ? (Guid?)null : id.Value, value => value == null ? null : new AccountId(value.Value));
        builder.HasOne<Account>().WithMany().HasForeignKey(t => t.DebitAccountId);

        builder.Property(t => t.DebitPostedDate)
            .HasColumnName("AccountTransaction_DebitPostedDate");

        builder.Property(t => t.CreditAccountId)
            .HasColumnName("AccountTransaction_CreditAccountId")
            .HasConversion(id => id == null ? (Guid?)null : id.Value, value => value == null ? null : new AccountId(value.Value));
        builder.HasOne<Account>().WithMany().HasForeignKey(t => t.CreditAccountId);

        builder.Property(t => t.CreditPostedDate)
            .HasColumnName("AccountTransaction_CreditPostedDate");
    }

    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FundTransaction> builder)
    {
        builder.Property(t => t.DebitFundId)
            .HasColumnName("FundTransaction_DebitFundId")
            .HasConversion(id => id.Value, value => new FundId(value));
        builder.HasOne<Fund>().WithMany().HasForeignKey(t => t.DebitFundId);

        builder.Property(t => t.CreditFundId)
            .HasColumnName("FundTransaction_CreditFundId")
            .HasConversion(id => id.Value, value => new FundId(value));
        builder.HasOne<Fund>().WithMany().HasForeignKey(t => t.CreditFundId);
    }
}