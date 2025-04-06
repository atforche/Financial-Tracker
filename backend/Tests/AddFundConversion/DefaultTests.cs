using Domain.Aggregates.AccountingPeriods;
using Domain.Services;
using Tests.Scenarios;
using Tests.Validators;

namespace Tests.AddFundConversion;

/// <summary>
/// Test class that tests adding a Fund Conversion with a default scenario
/// </summary>
public class DefaultTests
{
    /// <summary>
    /// Runs the test for this test class
    /// </summary>
    [Fact]
    public void RunTest()
    {
        var setup = new DefaultScenarioSetup();
        FundConversion fundConversion = setup.GetService<IAccountingPeriodService>().AddFundConversion(setup.AccountingPeriod,
            new DateOnly(2025, 1, 15),
            setup.Account,
            setup.Fund,
            setup.OtherFund,
            100.00m);
        new FundConversionValidator().Validate(fundConversion,
            new FundConversionState
            {
                AccountName = setup.Account.Name,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = 1,
                FromFundName = setup.Fund.Name,
                ToFundName = setup.OtherFund.Name,
                Amount = 100.00m,
            });
    }
}