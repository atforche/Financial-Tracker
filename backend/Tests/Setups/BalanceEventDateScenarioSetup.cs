using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Tests.Scenarios;

namespace Tests.Setups;

/// <summary>
/// Setup class for a <see cref="AddBalanceEventDateScenarios"/>
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
}