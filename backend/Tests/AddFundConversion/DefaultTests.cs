using Domain.FundConversions;
using Tests.Setups;
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
        using var setup = new DefaultScenarioSetup();
        FundConversion fundConversion = setup.GetService<FundConversionFactory>().Create(new CreateFundConversionRequest
        {
            AccountingPeriodId = setup.AccountingPeriod.Id,
            EventDate = new DateOnly(2025, 1, 15),
            AccountId = setup.Account.Id,
            FromFundId = setup.Fund.Id,
            ToFundId = setup.OtherFund.Id,
            Amount = 100.00m
        });
        new FundConversionValidator().Validate(fundConversion,
            new FundConversionState
            {
                AccountingPeriodId = setup.AccountingPeriod.Id,
                EventDate = new DateOnly(2025, 1, 15),
                EventSequence = 1,
                AccountId = setup.Account.Id,
                FromFundId = setup.Fund.Id,
                ToFundId = setup.OtherFund.Id,
                Amount = 100.00m,
            });
    }
}