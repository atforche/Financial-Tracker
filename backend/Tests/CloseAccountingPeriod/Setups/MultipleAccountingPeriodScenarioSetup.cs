using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;
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
        Fund = GetService<IFundService>().CreateNewFund("Test");
        GetService<IFundRepository>().Add(Fund);

        // Grab the valid setup for adding multiple Accounting Periods
        (DateOnly firstPeriod, DateOnly secondPeriod, DateOnly thirdPeriod, bool shouldClosePeriods) =
            new AddAccountingPeriod.Scenarios.MultipleAccountingPeriodScenarios()
                .DistinctBy(row => (row.Data.Item1, row.Data.Item2, row.Data.Item3))
                .Single(row => AddAccountingPeriod.Scenarios.MultipleAccountingPeriodScenarios.IsValid(row.Data.Item1, row.Data.Item2, row.Data.Item3))
                .Data;
        List<DateOnly> accountingPeriodDates = [firstPeriod, secondPeriod, thirdPeriod];

        List<AccountingPeriod> accountingPeriods = [];
        foreach (DateOnly accountingPeriodDate in accountingPeriodDates)
        {
            AccountingPeriod accountingPeriod = GetService<AddAccountingPeriodAction>()
                .Run(accountingPeriodDate.Year, accountingPeriodDate.Month);
            GetService<IAccountingPeriodRepository>().Add(accountingPeriod);
            accountingPeriods.Add(accountingPeriod);

            if (Account == null)
            {
                Account = GetService<AddAccountAction>().Run("Test", AccountType.Standard,
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