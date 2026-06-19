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

        builder.Property(t => t.DestinationLocation)
            .HasColumnName("SpendingTransaction_DestinationLocation");

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
        builder.Property(t => t.SourceAccountId)
            .HasColumnName("IncomeTransaction_SourceAccountId")
            .HasConversion(id => id == null ? (Guid?)null : id.Value, value => value == null ? null : new AccountId(value.Value));
        builder.HasOne<Account>().WithMany().HasForeignKey(t => t.SourceAccountId);

        builder.Property(t => t.SourcePostedDate)
            .HasColumnName("IncomeTransaction_SourcePostedDate");

        builder.Property(t => t.SourceLocation)
            .HasColumnName("IncomeTransaction_SourceLocation");

        builder.OwnsMany(t => t.IncomeLines, incomeLine =>
        {
            incomeLine.ToTable("IncomeTransactionIncomeLines");
            incomeLine.WithOwner().HasForeignKey("IncomeTransactionId");
            incomeLine.Property<int>("Id");
            incomeLine.HasKey("Id");
        });
        builder.Navigation(t => t.IncomeLines).AutoInclude();

        builder.OwnsMany(t => t.IncomeDeductions, incomeDeduction =>
        {
            incomeDeduction.ToTable("IncomeTransactionIncomeDeductions");
            incomeDeduction.WithOwner().HasForeignKey("IncomeTransactionId");
            incomeDeduction.Property<int>("Id");
            incomeDeduction.HasKey("Id");
        });
        builder.Navigation(t => t.IncomeDeductions).AutoInclude();

        builder.OwnsMany(t => t.IncomeDestinations, incomeDestinationBuilder =>
        {
            incomeDestinationBuilder.ToTable("IncomeTransactionIncomeDestinations");
            incomeDestinationBuilder.WithOwner().HasForeignKey("IncomeTransactionId");
            incomeDestinationBuilder.Property<int>("Id");
            incomeDestinationBuilder.HasKey("Id");
            incomeDestinationBuilder.HasOne(d => d.Account).WithMany().HasForeignKey("AccountId");

            incomeDestinationBuilder.OwnsMany(d => d.FundAssignments, fundAssignmentBuilder =>
            {
                fundAssignmentBuilder.ToTable("IncomeTransactionIncomeDestinationFundAssignments");
                fundAssignmentBuilder.WithOwner().HasForeignKey("IncomeDestinationId");
                fundAssignmentBuilder.Property<int>("Id");
                fundAssignmentBuilder.HasKey("Id");
                fundAssignmentBuilder.Property(f => f.FundId)
                    .HasConversion(fundId => fundId.Value, value => new FundId(value));
            });
            incomeDestinationBuilder.Navigation(d => d.FundAssignments).AutoInclude();
        });
        builder.Navigation(t => t.IncomeDestinations).AutoInclude();
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