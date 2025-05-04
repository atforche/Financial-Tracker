using System.Collections;
using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.ValueObjects;
using Tests.Setups;

namespace Tests.Scenarios.Transaction;

/// <summary>
/// Collection class that contains all the unique Transaction Account scenarios that should be tested
/// </summary>
public sealed class AddTransactionAccountScenarios :
    IEnumerable<TheoryDataRow<AccountType?, AccountType?, SameAccountTypeBehavior>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<AccountType?, AccountType?, SameAccountTypeBehavior>> GetEnumerator()
    {
        var accountTypes = new AccountScenarios().Select(row => (AccountType?)row.Data).ToList();
        accountTypes.Add(null);
        foreach (AccountType? debitAccountType in accountTypes)
        {
            foreach (AccountType? creditAccountType in accountTypes)
            {
                yield return new TheoryDataRow<AccountType?, AccountType?, SameAccountTypeBehavior>(
                    debitAccountType,
                    creditAccountType,
                    SameAccountTypeBehavior.UseDifferentAccounts);
                if (debitAccountType != null && creditAccountType != null && debitAccountType == creditAccountType)
                {
                    yield return new TheoryDataRow<AccountType?, AccountType?, SameAccountTypeBehavior>(
                        debitAccountType,
                        creditAccountType,
                        SameAccountTypeBehavior.UseSameAccount);
                }
            }
        }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Collection class that contains all the valid Transaction Account scenarios that should be tested
/// </summary>
public sealed class TransactionAccountScenarios :
    IEnumerable<TheoryDataRow<AccountType?, AccountType?, SameAccountTypeBehavior>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<AccountType?, AccountType?, SameAccountTypeBehavior>> GetEnumerator() =>
        new AddTransactionAccountScenarios().Where(row => row.Data.Item1 != row.Data.Item2).GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Enum representing the different behaviors when the same Account type is used for the debit and credit Accounts
/// </summary>
public enum SameAccountTypeBehavior
{
    /// <summary>
    /// Use the same Account as the debit and credit Accounts
    /// </summary>
    UseSameAccount,

    /// <summary>
    /// Use two different Accounts of the same type for the debit and credit Accounts
    /// </summary>
    UseDifferentAccounts,
}

/// <summary>
/// Setup class for a Transaction Account scenario
/// </summary>
internal sealed class TransactionAccountScenarioSetup : ScenarioSetup
{
    /// <summary>
    /// Fund for this Setup
    /// </summary>
    public Fund Fund { get; }

    /// <summary>
    /// Other Fund for this Setup
    /// </summary>
    public Fund OtherFund { get; }

    /// <summary>
    /// Accounting Period for this Setup
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; }

    /// <summary>
    /// Debit Account for this Setup
    /// </summary>
    public Account? DebitAccount { get; }

    /// <summary>
    /// Credit Account for this Setup
    /// </summary>
    public Account? CreditAccount { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="debitAccountType">Account Type for the Debit Account</param>
    /// <param name="creditAccountType">Account Type for the Credit Account</param>
    /// <param name="sameAccountTypeBehavior">Behavior to use if the same Account type is provided for both the debit and credit Accounts</param>
    public TransactionAccountScenarioSetup(
        AccountType? debitAccountType,
        AccountType? creditAccountType,
        SameAccountTypeBehavior sameAccountTypeBehavior)
    {
        Fund = GetService<AddFundAction>().Run("Test");
        GetService<IFundRepository>().Add(Fund);
        OtherFund = GetService<AddFundAction>().Run("OtherTest");
        GetService<IFundRepository>().Add(OtherFund);

        AccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(AccountingPeriod);

        AddAccountAction addAccountAction = GetService<AddAccountAction>();
        IAccountRepository accountRepository = GetService<IAccountRepository>();
        if (debitAccountType != null)
        {
            DebitAccount = addAccountAction.Run("TestOne", debitAccountType.Value, AccountingPeriod, AccountingPeriod.PeriodStartDate,
                [
                    new FundAmount
                    {
                        Fund = Fund,
                        Amount = 1500.00m,
                    },
                    new FundAmount
                    {
                        Fund = OtherFund,
                        Amount = 1500.00m
                    }
                ]);
            accountRepository.Add(DebitAccount);
        }
        if (creditAccountType != null && creditAccountType == debitAccountType && sameAccountTypeBehavior == SameAccountTypeBehavior.UseSameAccount)
        {
            CreditAccount = DebitAccount;
        }
        else if (creditAccountType != null)
        {
            CreditAccount = addAccountAction.Run("TestTwo", creditAccountType.Value, AccountingPeriod, AccountingPeriod.PeriodStartDate,
                [
                    new FundAmount
                    {
                        Fund = Fund,
                        Amount = 1500.00m,
                    },
                    new FundAmount
                    {
                        Fund = OtherFund,
                        Amount = 1500.00m
                    }
                ]);
            accountRepository.Add(CreditAccount);
        }
    }
}