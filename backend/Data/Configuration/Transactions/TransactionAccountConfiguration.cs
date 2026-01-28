using Domain.Accounts;
using Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configuration.Transactions;

/// <summary>
/// EF Core entity configuration for a <see cref="TransactionAccount"/>
/// </summary>
internal sealed class TransactionAccountConfiguration : IEntityTypeConfiguration<TransactionAccount>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<TransactionAccount> builder)
    {
        // Use an auto-incrementing shadow property key since a Transaction Account is a value object
        builder.Property<int>("Id");
        builder.HasKey("Id");

        builder.HasOne<Account>().WithMany().HasForeignKey(transactionAccount => transactionAccount.Account);

        builder.HasMany(transactionAccount => transactionAccount.FundAmounts).WithOne().HasForeignKey("TransactionAccountId");
        builder.Navigation(transactionAccount => transactionAccount.FundAmounts).AutoInclude();
    }
}