using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;
using Tests.GetAccountBalanceByDateTests.Scenarios;
using Tests.Setups;

namespace Tests.GetAccountBalanceByDateTests.Setups;

/// <summary>
/// Setup class for a <see cref="DateRangeScenarios"/> for getting an Account Balance by Date
/// </summary>
internal sealed class DateRangeScenarioSetup : ScenarioSetup
{
    /// <summary>
    /// Fund for the Setup
    /// </summary>
    public Fund Fund { get; }

    /// <summary>
    /// Account for the Setup
    /// </summary>
    public Account Account { get; }

    /// <summary>
    /// Date Range for the Setup
    /// </summary>
    public DateRange DateRange { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="scenario">Scenario for this test case</param>
    public DateRangeScenarioSetup(DateRangeScenario scenario)
    {
        Fund = GetService<IFundService>().CreateNewFund("Test");
        GetService<IFundRepository>().Add(Fund);

        AccountingPeriod firstAccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2024, 12);
        GetService<IAccountingPeriodRepository>().Add(firstAccountingPeriod);
        GetService<CloseAccountingPeriodAction>().Run(firstAccountingPeriod);

        AccountingPeriod secondAccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(secondAccountingPeriod);

        Account = GetService<AddAccountAction>().Run("Test", AccountType.Standard,
            [
                new FundAmount
                {
                    Fund = Fund,
                    Amount = 1500.00m,
                }
            ]);
        GetService<IAccountRepository>().Add(Account);

        AccountingPeriod thirdAccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2025, 2);
        GetService<IAccountingPeriodRepository>().Add(thirdAccountingPeriod);

        DateRange = scenario switch
        {
            DateRangeScenario.RangeExtendsIntoFirstAccountingPeriod => new DateRange(new DateOnly(2024, 11, 30), new DateOnly(2024, 12, 1)),
            DateRangeScenario.RangeExtendsAfterAccountWasAdded => new DateRange(new DateOnly(2024, 12, 31), new DateOnly(2025, 1, 1)),
            DateRangeScenario.RangeFallsAfterAccountWasAdded => new DateRange(new DateOnly(2025, 1, 15), new DateOnly(2025, 1, 15)),
            DateRangeScenario.RangeExtendsOutOfLastAccountingPeriod => new DateRange(new DateOnly(2025, 1, 31), new DateOnly(2025, 2, 1)),
            DateRangeScenario.RangeFallsAfterAllAccountingPeriods => new DateRange(new DateOnly(2025, 12, 1), new DateOnly(2025, 12, 1)),
            _ => throw new InvalidOperationException()
        };
    }
}