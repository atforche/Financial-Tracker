using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;

namespace Tests.Setups;

/// <summary>
/// Setup class for an Accounting Period test case
/// </summary>
internal sealed class AccountingPeriodSetup : TestCaseSetup
{
    /// <summary>
    /// Fund for the Accounting Period Setup
    /// </summary>
    public Fund Fund { get; }

    /// <summary>
    /// Account for the Accounting Period Setup
    /// </summary>
    public Account Account { get; }

    /// <summary>
    /// Past Accounting Period for the Accounting Period Setup
    /// </summary>
    public AccountingPeriod? PastAccountingPeriod { get; }

    /// <summary>
    /// Current Accounting Period for the Accounting Period Setup
    /// </summary>
    public AccountingPeriod CurrentAccountingPeriod { get; }

    /// <summary>
    /// Future Accounting Period for the Accounting Period Setup
    /// </summary>
    public AccountingPeriod? FutureAccountingPeriod { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="pastPeriodStatus">Status of the past Accounting Period</param>
    /// <param name="currentPeriodStatus">Status of the current Accounting Period</param>
    /// <param name="futurePeriodStatus">Status of the future Accounting Period</param>
    public AccountingPeriodSetup(
        AccountingPeriodStatus? pastPeriodStatus,
        AccountingPeriodStatus currentPeriodStatus,
        AccountingPeriodStatus? futurePeriodStatus)
    {
        Fund = GetService<IFundService>().CreateNewFund("Test");
        GetService<IFundRepository>().Add(Fund);

        IAccountingPeriodService accountingPeriodService = GetService<IAccountingPeriodService>();
        IAccountingPeriodRepository accountingPeriodRepository = GetService<IAccountingPeriodRepository>();
        // Create the past Accounting Period if needed
        if (pastPeriodStatus != null)
        {
            PastAccountingPeriod = accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
            accountingPeriodRepository.Add(PastAccountingPeriod);
            if (pastPeriodStatus == AccountingPeriodStatus.Closed)
            {
                accountingPeriodService.ClosePeriod(PastAccountingPeriod);
            }
        }
        // Create the current Accounting Period
        CurrentAccountingPeriod = accountingPeriodService.CreateNewAccountingPeriod(2025, 1);
        accountingPeriodRepository.Add(CurrentAccountingPeriod);
        Account = GetService<IAccountService>().CreateNewAccount("Test", AccountType.Standard,
            [
                new FundAmount
                {
                    Fund = Fund,
                    Amount = 2500.00m,
                }
            ]);
        GetService<IAccountRepository>().Add(Account);
        if (currentPeriodStatus == AccountingPeriodStatus.Closed)
        {
            accountingPeriodService.ClosePeriod(CurrentAccountingPeriod);
        }
        // Create the future Accounting Period if needed
        if (futurePeriodStatus != null)
        {
            FutureAccountingPeriod = accountingPeriodService.CreateNewAccountingPeriod(2025, 2);
            accountingPeriodRepository.Add(FutureAccountingPeriod);
            if (futurePeriodStatus == AccountingPeriodStatus.Closed)
            {
                accountingPeriodService.ClosePeriod(FutureAccountingPeriod);
            }
        }
    }

    /// <summary>
    /// Gets the collection of Accounting Period scenarios 
    /// </summary>
    public static IEnumerable<TheoryDataRow<AccountingPeriodStatus?, AccountingPeriodStatus, AccountingPeriodStatus?>> GetCollection()
    {
        List<AccountingPeriodStatus?> validOtherTermValues = [null, AccountingPeriodStatus.Open, AccountingPeriodStatus.Closed];
        foreach (AccountingPeriodStatus? pastPeriodValue in validOtherTermValues)
        {
            foreach (AccountingPeriodStatus currentPeriodValue in Enum.GetValues<AccountingPeriodStatus>())
            {
                foreach (AccountingPeriodStatus? futurePeriodValue in validOtherTermValues)
                {
                    if (futurePeriodValue > currentPeriodValue || futurePeriodValue > pastPeriodValue || currentPeriodValue > pastPeriodValue)
                    {
                        continue;
                    }
                    yield return new TheoryDataRow<AccountingPeriodStatus?, AccountingPeriodStatus, AccountingPeriodStatus?>(pastPeriodValue, currentPeriodValue, futurePeriodValue);
                }
            }
        }
    }
}

/// <summary>
/// Enum representing the status of an Accounting Period
/// </summary>
public enum AccountingPeriodStatus
{
    /// <summary>
    /// Accounting Period is open
    /// </summary>
    Open,

    /// <summary>
    /// Accounting Period is closed
    /// </summary>
    Closed,
}