using Domain;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Tests.Old.Mocks;
using Tests.Old.Scenarios;

namespace Tests.Old.Setups;

/// <summary>
/// Setup class for a <see cref="GetAccountBalanceDateRangeScenario"/> for getting an Account Balance
/// </summary>
internal sealed class GetAccountBalanceDateRangeScenarioSetup : ScenarioSetup
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
    public GetAccountBalanceDateRangeScenarioSetup(GetAccountBalanceDateRangeScenario scenario)
    {
        Fund = GetService<FundService>().Create("Test", "");
        GetService<IFundRepository>().Add(Fund);
        GetService<TestUnitOfWork>().SaveChanges();

        AccountingPeriod firstAccountingPeriod = GetService<AccountingPeriodFactory>().Create(2024, 12);
        GetService<IAccountingPeriodRepository>().Add(firstAccountingPeriod);
        GetService<TestUnitOfWork>().SaveChanges();

        GetService<CloseAccountingPeriodAction>().Run(firstAccountingPeriod);
        GetService<TestUnitOfWork>().SaveChanges();

        AccountingPeriod secondAccountingPeriod = GetService<AccountingPeriodFactory>().Create(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(secondAccountingPeriod);
        GetService<TestUnitOfWork>().SaveChanges();

        Account = GetService<AccountFactory>().Create("Test", AccountType.Standard, secondAccountingPeriod.Id, secondAccountingPeriod.PeriodStartDate,
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 1500.00m,
                }
            ]);
        GetService<IAccountRepository>().Add(Account);
        GetService<TestUnitOfWork>().SaveChanges();

        AccountingPeriod thirdAccountingPeriod = GetService<AccountingPeriodFactory>().Create(2025, 2);
        GetService<IAccountingPeriodRepository>().Add(thirdAccountingPeriod);
        GetService<TestUnitOfWork>().SaveChanges();

        DateRange = scenario switch
        {
            GetAccountBalanceDateRangeScenario.RangeExtendsIntoFirstAccountingPeriod => new DateRange(new DateOnly(2024, 11, 30), new DateOnly(2024, 12, 1)),
            GetAccountBalanceDateRangeScenario.RangeExtendsAfterAccountWasAdded => new DateRange(new DateOnly(2024, 12, 31), new DateOnly(2025, 1, 1)),
            GetAccountBalanceDateRangeScenario.RangeFallsAfterAccountWasAdded => new DateRange(new DateOnly(2025, 1, 15), new DateOnly(2025, 1, 15)),
            GetAccountBalanceDateRangeScenario.RangeExtendsOutOfLastAccountingPeriod => new DateRange(new DateOnly(2025, 1, 31), new DateOnly(2025, 2, 1)),
            GetAccountBalanceDateRangeScenario.RangeFallsAfterAllAccountingPeriods => new DateRange(new DateOnly(2025, 12, 1), new DateOnly(2025, 12, 1)),
            _ => throw new InvalidOperationException()
        };
    }
}