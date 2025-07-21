using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Tests.Old.CloseAccountingPeriod.Scenarios;
using Tests.Old.Mocks;
using Tests.Old.Setups;

namespace Tests.Old.CloseAccountingPeriod.Setups;

/// <summary>
/// Setup class for a <see cref="MultipleAccountingPeriodScenarios"/> for closing an Accounting Period
/// </summary>
internal sealed class MultipleAccountingPeriodScenarioSetup : ScenarioSetup
{
    /// <summary>
    /// Fund for the Setup
    /// </summary>
    public Fund Fund { get; }

    /// <summary>
    /// Account for the Setup
    /// </summary>
    public Account Account { get; } = null!;

    /// <summary>
    /// Accounting Periods for the Setup
    /// </summary>
    public IReadOnlyCollection<AccountingPeriod> AccountingPeriods { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public MultipleAccountingPeriodScenarioSetup()
    {
        Fund = GetService<FundFactory>().Create("Test", "");
        GetService<IFundRepository>().Add(Fund);
        GetService<TestUnitOfWork>().SaveChanges();

        List<AccountingPeriod> accountingPeriods = [];
        List<DateOnly> accountingPeriodDates = [new DateOnly(2024, 12, 1), new DateOnly(2025, 1, 1), new DateOnly(2025, 2, 1)];
        foreach (DateOnly accountingPeriodDate in accountingPeriodDates)
        {
            AccountingPeriod accountingPeriod = GetService<AccountingPeriodFactory>()
                .Create(accountingPeriodDate.Year, accountingPeriodDate.Month);
            GetService<IAccountingPeriodRepository>().Add(accountingPeriod);
            GetService<TestUnitOfWork>().SaveChanges();
            accountingPeriods.Add(accountingPeriod);

            if (Account == null)
            {
                Account = GetService<AccountFactory>().Create("Test", AccountType.Standard, accountingPeriod.Id, accountingPeriod.PeriodStartDate,
                [
                    new FundAmount
                    {
                        FundId = Fund.Id,
                        Amount = 1500.00m,
                    }
                ]);
                GetService<IAccountRepository>().Add(Account);
                GetService<TestUnitOfWork>().SaveChanges();
            }
        }
        AccountingPeriods = accountingPeriods;
    }
}