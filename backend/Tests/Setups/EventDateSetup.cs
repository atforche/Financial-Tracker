using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Tests.Scenarios;

namespace Tests.Setups;

/// <summary>
/// Setup class for an Event Date test case
/// </summary>
internal sealed class EventDateSetup : TestCaseSetup
{
    private readonly AccountingPeriod? _pastAccountingPeriod;
    private readonly AccountingPeriod? _futureAccountingPeriod;

    /// <summary>
    /// Fund for the Event Date Setup
    /// </summary>
    public Fund Fund { get; }

    /// <summary>
    /// Other Fund for the Event Date Setup
    /// </summary>
    public Fund OtherFund { get; }

    /// <summary>
    /// Account for the Event Date Setup
    /// </summary>
    public Account Account { get; }

    /// <summary>
    /// Current Accounting Period for the Accounting Period Setup
    /// </summary>
    public AccountingPeriod CurrentAccountingPeriod { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public EventDateSetup()
    {
        var accountingPeriodSetup = new AccountingPeriodScenarioSetup(AccountingPeriodStatus.Closed,
            AccountingPeriodStatus.Open,
            AccountingPeriodStatus.Open);
        Fund = accountingPeriodSetup.Fund;
        GetService<IFundRepository>().Add(Fund);
        OtherFund = accountingPeriodSetup.OtherFund;
        GetService<IFundRepository>().Add(OtherFund);
        Account = accountingPeriodSetup.Account;
        GetService<IAccountRepository>().Add(Account);

        IAccountingPeriodRepository accountingPeriodRepository = GetService<IAccountingPeriodRepository>();
        _pastAccountingPeriod = accountingPeriodSetup.PastAccountingPeriod;
        if (_pastAccountingPeriod != null)
        {
            accountingPeriodRepository.Add(_pastAccountingPeriod);
        }
        CurrentAccountingPeriod = accountingPeriodSetup.CurrentAccountingPeriod;
        accountingPeriodRepository.Add(CurrentAccountingPeriod);
        _futureAccountingPeriod = accountingPeriodSetup.FutureAccountingPeriod;
        if (_futureAccountingPeriod != null)
        {
            accountingPeriodRepository.Add(_futureAccountingPeriod);
        }
    }

    /// <summary>
    /// Calculates the number of months between the current Accounting Period and the provided event date
    /// </summary>
    /// <param name="eventDate">Event Date</param>
    /// <returns>The number of months between the current Accounting Period and the provided event date</returns>
    public int CalculateMonthDifference(DateOnly eventDate) =>
        (Math.Abs(CurrentAccountingPeriod.Year - eventDate.Year) * 12) + Math.Abs(CurrentAccountingPeriod.Month - eventDate.Month);

    /// <summary>
    /// Gets the collection of Event Date scenarios
    /// </summary>
    public static IEnumerable<TheoryDataRow<DateOnly>> GetCollection() =>
    [
        new TheoryDataRow<DateOnly>(new DateOnly(2024, 11, 1)),
        new TheoryDataRow<DateOnly>(new DateOnly(2024, 12, 1)),
        new TheoryDataRow<DateOnly>(new DateOnly(2025, 1, 1)),
        new TheoryDataRow<DateOnly>(new DateOnly(2025, 2, 1)),
        new TheoryDataRow<DateOnly>(new DateOnly(2025, 3, 1)),
    ];
}