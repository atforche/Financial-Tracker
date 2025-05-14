using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Actions;
using Domain.Funds;
using Tests.CloseAccountingPeriod.Scenarios;
using Tests.Setups;

namespace Tests.CloseAccountingPeriod.Setups;

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
        Fund = GetService<AddFundAction>().Run("Test");
        GetService<IFundRepository>().Add(Fund);

        List<AccountingPeriod> accountingPeriods = [];
        List<DateOnly> accountingPeriodDates = [new DateOnly(2024, 12, 1), new DateOnly(2025, 1, 1), new DateOnly(2025, 2, 1)];
        foreach (DateOnly accountingPeriodDate in accountingPeriodDates)
        {
            AccountingPeriod accountingPeriod = GetService<AddAccountingPeriodAction>()
                .Run(accountingPeriodDate.Year, accountingPeriodDate.Month);
            GetService<IAccountingPeriodRepository>().Add(accountingPeriod);
            accountingPeriods.Add(accountingPeriod);

            if (Account == null)
            {
                Account = GetService<AddAccountAction>().Run("Test", AccountType.Standard, accountingPeriod, accountingPeriod.PeriodStartDate,
                [
                    new FundAmount
                    {
                        Fund = Fund,
                        Amount = 1500.00m,
                    }
                ]);
                GetService<IAccountRepository>().Add(Account);
            }
        }
        AccountingPeriods = accountingPeriods;
    }
}