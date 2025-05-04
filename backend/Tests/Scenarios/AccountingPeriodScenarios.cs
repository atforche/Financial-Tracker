using System.Collections;
using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.ValueObjects;
using Tests.Setups;

namespace Tests.Scenarios;

/// <summary>
/// Collection class that contains all the unique Accounting Period scenarios that should be tested
/// </summary>
public sealed class AddAccountingPeriodScenarios :
    IEnumerable<TheoryDataRow<AccountingPeriodStatus?, AccountingPeriodStatus, AccountingPeriodStatus?>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<AccountingPeriodStatus?, AccountingPeriodStatus, AccountingPeriodStatus?>> GetEnumerator()
    {
        List<AccountingPeriodStatus?> validOtherTermValues = [null, AccountingPeriodStatus.Open, AccountingPeriodStatus.Closed];
        foreach (AccountingPeriodStatus? pastPeriodValue in validOtherTermValues)
        {
            foreach (AccountingPeriodStatus currentPeriodValue in Enum.GetValues<AccountingPeriodStatus>())
            {
                foreach (AccountingPeriodStatus? futurePeriodValue in validOtherTermValues)
                {
                    yield return new TheoryDataRow<AccountingPeriodStatus?, AccountingPeriodStatus, AccountingPeriodStatus?>(
                        pastPeriodValue,
                        currentPeriodValue,
                        futurePeriodValue);
                }
            }
        }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Collection class that contains all the valid Accounting Period scenarios that should be tested
/// </summary>
public sealed class AccountingPeriodScenarios :
    IEnumerable<TheoryDataRow<AccountingPeriodStatus?, AccountingPeriodStatus, AccountingPeriodStatus?>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<AccountingPeriodStatus?, AccountingPeriodStatus, AccountingPeriodStatus?>> GetEnumerator() =>
        new AddAccountingPeriodScenarios()
            .Where(row => row.Data.Item3 <= row.Data.Item2 && row.Data.Item3 <= row.Data.Item1 && row.Data.Item2 <= row.Data.Item1)
            .GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
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

/// <summary>
/// Setup class for an Accounting Period scenario
/// </summary>
internal sealed class AccountingPeriodScenarioSetup : ScenarioSetup
{
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
    /// Past Accounting Period for the Setup
    /// </summary>
    public AccountingPeriod? PastAccountingPeriod { get; }

    /// <summary>
    /// Current Accounting Period for the Setup
    /// </summary>
    public AccountingPeriod CurrentAccountingPeriod { get; }

    /// <summary>
    /// Future Accounting Period for the Setup
    /// </summary>
    public AccountingPeriod? FutureAccountingPeriod { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="pastPeriodStatus">Status of the past Accounting Period</param>
    /// <param name="currentPeriodStatus">Status of the current Accounting Period</param>
    /// <param name="futurePeriodStatus">Status of the future Accounting Period</param>
    public AccountingPeriodScenarioSetup(
        AccountingPeriodStatus? pastPeriodStatus,
        AccountingPeriodStatus currentPeriodStatus,
        AccountingPeriodStatus? futurePeriodStatus)
    {
        Fund = GetService<AddFundAction>().Run("Test");
        GetService<IFundRepository>().Add(Fund);
        OtherFund = GetService<AddFundAction>().Run("OtherTest");
        GetService<IFundRepository>().Add(OtherFund);

        AddAccountingPeriodAction addAccountingPeriodAction = GetService<AddAccountingPeriodAction>();
        CloseAccountingPeriodAction closeAccountingPeriodAction = GetService<CloseAccountingPeriodAction>();
        IAccountingPeriodRepository accountingPeriodRepository = GetService<IAccountingPeriodRepository>();
        // Create the past Accounting Period if needed
        if (pastPeriodStatus != null)
        {
            PastAccountingPeriod = addAccountingPeriodAction.Run(2024, 12);
            accountingPeriodRepository.Add(PastAccountingPeriod);
            Account = CreateAccount(PastAccountingPeriod);
            if (pastPeriodStatus == AccountingPeriodStatus.Closed)
            {
                closeAccountingPeriodAction.Run(PastAccountingPeriod);
            }
        }
        // Create the current Accounting Period
        CurrentAccountingPeriod = addAccountingPeriodAction.Run(2025, 1);
        accountingPeriodRepository.Add(CurrentAccountingPeriod);
        Account ??= CreateAccount(CurrentAccountingPeriod);
        if (currentPeriodStatus == AccountingPeriodStatus.Closed)
        {
            closeAccountingPeriodAction.Run(CurrentAccountingPeriod);
        }
        // Create the future Accounting Period if needed
        if (futurePeriodStatus != null)
        {
            FutureAccountingPeriod = addAccountingPeriodAction.Run(2025, 2);
            accountingPeriodRepository.Add(FutureAccountingPeriod);
            if (futurePeriodStatus == AccountingPeriodStatus.Closed)
            {
                closeAccountingPeriodAction.Run(FutureAccountingPeriod);
            }
        }
    }

    /// <summary>
    /// Creates the Account for this test case
    /// </summary>
    /// <param name="accountingPeriod">Accounting Period to add the Account to</param>
    /// <returns>The Account for this test case</returns>
    private Account CreateAccount(AccountingPeriod accountingPeriod)
    {
        Account account = GetService<AddAccountAction>().Run("Test", AccountType.Standard, accountingPeriod, accountingPeriod.PeriodStartDate,
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
        GetService<IAccountRepository>().Add(account);
        return account;
    }
}