using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.ValueObjects;
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

        Fund = GetService<AddFundAction>().Run("Test");
        GetService<IFundRepository>().Add(Fund);
        OtherFund = GetService<AddFundAction>().Run("OtherTest");
        GetService<IFundRepository>().Add(OtherFund);

        _pastAccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2024, 12);
        GetService<IAccountingPeriodRepository>().Add(_pastAccountingPeriod);
        CurrentAccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(CurrentAccountingPeriod);
        _futureAccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2025, 2);
        GetService<IAccountingPeriodRepository>().Add(_futureAccountingPeriod);

        Account = GetService<AddAccountAction>().Run("Test", AccountType.Standard, CurrentAccountingPeriod, _pastAccountingPeriod.PeriodStartDate.AddDays(14),
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
    }
}