using System.Collections;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Tests.Setups;

namespace Tests.Scenarios;

/// <summary>
/// Collection class that contains all the unique Balance Event Date scenarios that should be tested
/// </summary>
public sealed class BalanceEventDateScenarios : IEnumerable<TheoryDataRow<DateOnly>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<DateOnly>> GetEnumerator() => new List<TheoryDataRow<DateOnly>>
        {
            new(new DateOnly(2024, 11, 1)),
            new(new DateOnly(2024, 12, 1)),
            new(new DateOnly(2025, 1, 1)),
            new(new DateOnly(2025, 2, 1)),
            new(new DateOnly(2025, 3, 1)),
        }.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Setup class for a Balance Event Date scenario
/// </summary>
internal sealed class BalanceEventDateScenarioSetup : ScenarioSetup
{
    private readonly AccountingPeriod? _pastAccountingPeriod;
    private readonly AccountingPeriod? _futureAccountingPeriod;

    /// <summary>
    /// Event Date for the Setup
    /// </summary>
    public DateOnly EventDate { get; }

    /// <summary>
    /// Fund for the Setup
    /// </summary>
    public Fund Fund { get; }

    /// <summary>
    /// Other Fund for the Setup
    /// </summary>
    public Fund OtherFund { get; }

    /// <summary>
    /// Account for the Setup
    /// </summary>
    public Account Account { get; }

    /// <summary>
    /// Current Accounting Period for the Setup
    /// </summary>
    public AccountingPeriod CurrentAccountingPeriod { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="eventDate">Event Date for the Setup</param>
    public BalanceEventDateScenarioSetup(DateOnly eventDate)
    {
        EventDate = eventDate;

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
    /// Calculates the number of months between the current Accounting Period and the event date
    /// </summary>
    /// <returns>The number of months between the current Accounting Period and the event date</returns>
    public int CalculateMonthDifference() =>
        (Math.Abs(CurrentAccountingPeriod.Year - EventDate.Year) * 12) + Math.Abs(CurrentAccountingPeriod.Month - EventDate.Month);
}