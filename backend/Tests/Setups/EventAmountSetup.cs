using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;

namespace Tests.Setups;

/// <summary>
/// Setup class for an Event Amount test case
/// </summary>
internal sealed class EventAmountSetup : TestCaseSetup
{
    private readonly AccountingPeriod _futureAccountingPeriod;

    /// <summary>
    /// Fund for this Event Amount Setup
    /// </summary>
    public Fund Fund { get; }

    /// <summary>
    /// Other Fund for this Event Amount Setup
    /// </summary>
    public Fund OtherFund { get; }

    /// <summary>
    /// Account for this Event Amount Setup
    /// </summary>
    public Account Account { get; }

    /// <summary>
    /// Current Accounting Period for this Event Amount Setup
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; }

    /// <summary>
    /// Amount for this Event Amount Setup
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="scenario">Scenario for this Event Amount Setup</param>
    public EventAmountSetup(EventAmountScenario scenario)
    {
        IFundService fundService = GetService<IFundService>();
        IFundRepository fundRepository = GetService<IFundRepository>();
        Fund = fundService.CreateNewFund("Test");
        fundRepository.Add(Fund);
        OtherFund = fundService.CreateNewFund("Test2");
        fundRepository.Add(OtherFund);

        IAccountingPeriodService accountingPeriodService = GetService<IAccountingPeriodService>();
        IAccountingPeriodRepository accountingPeriodRepository = GetService<IAccountingPeriodRepository>();
        AccountingPeriod = accountingPeriodService.CreateNewAccountingPeriod(2025, 1);
        accountingPeriodRepository.Add(AccountingPeriod);
        _futureAccountingPeriod = accountingPeriodService.CreateNewAccountingPeriod(2025, 2);
        accountingPeriodRepository.Add(_futureAccountingPeriod);

        Account = GetService<IAccountService>().CreateNewAccount("Test", AccountType.Standard,
            [
                new FundAmount
                {
                    Fund = Fund,
                    Amount = 1500.00m,
                },
                new FundAmount
                {
                    Fund = OtherFund,
                    Amount = 1500.00m,
                }
            ]);
        GetService<IAccountRepository>().Add(Account);

        if (scenario == EventAmountScenario.Positive)
        {
            Amount = 1000.00m;
        }
        else if (scenario == EventAmountScenario.Zero)
        {
            Amount = 0.00m;
        }
        else if (scenario == EventAmountScenario.Negative)
        {
            Amount = -1000.00m;
        }
        else if (scenario == EventAmountScenario.ForcesFundBalanceNegative)
        {
            Amount = -2000.00m;
        }
        else if (scenario == EventAmountScenario.ForcesAccountBalanceToZero)
        {
            Amount = -3000.00m;
        }
        else if (scenario == EventAmountScenario.ForcesAccountBalanceNegative)
        {
            Amount = -4000.00m;
        }
        else if (scenario == EventAmountScenario.ForcesFutureEventToMakeAccountBalanceNegative)
        {
            GetService<IAccountingPeriodService>().AddChangeInValue(_futureAccountingPeriod,
                new DateOnly(2025, 2, 28),
                Account,
                new FundAmount
                {
                    Fund = OtherFund,
                    Amount = -2999.99m,
                });
            Amount = -100.00m;
        }
        else if (scenario == EventAmountScenario.ForcesAccountBalancesAtEndOfPeriodToBeNegative)
        {
            GetService<IAccountingPeriodService>().AddChangeInValue(_futureAccountingPeriod,
                new DateOnly(2025, 1, 1),
                Account,
                new FundAmount
                {
                    Fund = Fund,
                    Amount = 3000.00m,
                });
            Amount = -4000.00m;
        }
    }

    /// <summary>
    /// Gets a collection of Event Amount Scenarios
    /// </summary>
    public static IEnumerable<TheoryDataRow<EventAmountScenario>> GetCollection() => Enum.GetValues<EventAmountScenario>()
        .Select(scenario => new TheoryDataRow<EventAmountScenario>(scenario));
}

/// <summary>
/// Enum representing the different Event Amount Scenarios
/// </summary>
public enum EventAmountScenario
{
    /// <summary>
    /// Positive Amount
    /// </summary>
    Positive,

    /// <summary>
    /// Zero Amount
    /// </summary>
    Zero,

    /// <summary>
    /// Negative Amount
    /// </summary>
    Negative,

    /// <summary>
    /// Amount that forces the balance of the Fund to be negative
    /// </summary>
    ForcesFundBalanceNegative,

    /// <summary>
    /// Amount that forces the balance of the Account to be zero
    /// </summary>
    ForcesAccountBalanceToZero,

    /// <summary>
    /// Amount that forces the balance of the Account to be negative
    /// </summary>
    ForcesAccountBalanceNegative,

    /// <summary>
    /// Amount that forces a future Balance Event to force the Account balance to be negative
    /// </summary>
    ForcesFutureEventToMakeAccountBalanceNegative,

    /// <summary>
    /// Amount that forces the Account Balance at the end of the Accounting Period to be negative
    /// </summary>
    ForcesAccountBalancesAtEndOfPeriodToBeNegative,
}