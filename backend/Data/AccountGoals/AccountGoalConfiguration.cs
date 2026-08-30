using Domain.AccountGoals;
using Domain.AccountingPeriods;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.AccountGoals;

/// <summary>
/// EF Core entity configuration for an <see cref="AccountGoal"/>.
/// </summary>
internal sealed class AccountGoalConfiguration : IEntityTypeConfiguration<AccountGoal>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<AccountGoal> builder)
    {
        builder.HasKey(accountGoal => accountGoal.Id);
        builder.Property(accountGoal => accountGoal.Id)
            .HasConversion(id => id.Value, value => new AccountGoalId(value));
        builder.HasOne(accountGoal => accountGoal.Account).WithMany().HasForeignKey("AccountId");
        builder.Navigation(accountGoal => accountGoal.Account).AutoInclude();
        builder.Property<AccountingPeriodId?>("AccountingPeriodId").HasConversion(
            accountingPeriodId => accountingPeriodId == null ? (Guid?)null : accountingPeriodId.Value,
            value => value == null ? null : new AccountingPeriodId(value.Value));
        builder.HasOne(accountGoal => accountGoal.AccountingPeriod).WithMany();
        builder.Navigation(accountGoal => accountGoal.AccountingPeriod).AutoInclude();
        builder.HasIndex("AccountId", "AccountingPeriodId").IsUnique();
        builder.HasIndex("AccountId").IsUnique().HasFilter("\"AccountingPeriodId\" IS NULL");
    }
}
