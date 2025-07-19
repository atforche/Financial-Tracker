using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Tests.Old.Mocks;
using Tests.Old.Scenarios;

namespace Tests.Old.Setups;

/// <summary>
/// Setup class for a <see cref="AddBalanceEventDateScenarios"/> for adding a Balance Event
/// </summary>
internal sealed class AddBalanceEventDateScenarioSetup : ScenarioSetup
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
    /// Other Account for the Setup
    /// </summary>
    public Account OtherAccount { get; }

    /// <summary>
    /// Current Accounting Period for the Setup
    /// </summary>
    public AccountingPeriod CurrentAccountingPeriod { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="eventDate">Event Date for the Setup</param>
    public AddBalanceEventDateScenarioSetup(DateOnly eventDate)
    {
        EventDate = eventDate;

        Fund = GetService<FundFactory>().Create("Test");
        GetService<IFundRepository>().Add(Fund);
        OtherFund = GetService<FundFactory>().Create("OtherTest");
        GetService<IFundRepository>().Add(OtherFund);
        GetService<TestUnitOfWork>().SaveChanges();

        _pastAccountingPeriod = GetService<AccountingPeriodFactory>().Create(2024, 12);
        GetService<IAccountingPeriodRepository>().Add(_pastAccountingPeriod);
        GetService<TestUnitOfWork>().SaveChanges();

        CurrentAccountingPeriod = GetService<AccountingPeriodFactory>().Create(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(CurrentAccountingPeriod);
        GetService<TestUnitOfWork>().SaveChanges();

        _futureAccountingPeriod = GetService<AccountingPeriodFactory>().Create(2025, 2);
        GetService<IAccountingPeriodRepository>().Add(_futureAccountingPeriod);
        GetService<TestUnitOfWork>().SaveChanges();

        Account = GetService<AccountFactory>().Create("Test", AccountType.Standard, CurrentAccountingPeriod.Id, _pastAccountingPeriod.PeriodStartDate.AddDays(14),
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 1500.00m,
                },
                new FundAmount
                {
                    FundId = OtherFund.Id,
                    Amount = 1500.00m,
                }
            ]);
        GetService<IAccountRepository>().Add(Account);
        GetService<TestUnitOfWork>().SaveChanges();

        OtherAccount = GetService<AccountFactory>().Create("OtherTest", AccountType.Standard, CurrentAccountingPeriod.Id, _pastAccountingPeriod.PeriodStartDate.AddDays(14),
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 1500.00m,
                },
                new FundAmount
                {
                    FundId = OtherFund.Id,
                    Amount = 1500.00m,
                }
            ]);
        GetService<IAccountRepository>().Add(OtherAccount);
        GetService<TestUnitOfWork>().SaveChanges();
    }
}