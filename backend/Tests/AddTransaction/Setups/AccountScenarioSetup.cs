using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.ValueObjects;
using Tests.AddTransaction.Scenarios;
using Tests.Setups;

namespace Tests.AddTransaction.Setups;

/// <summary>
/// Setup class for <see cref="AccountScenarios"/> for adding a Transaction
/// </summary>
internal sealed class AccountScenarioSetup : ScenarioSetup
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
    public AccountScenarioSetup(
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

        if (debitAccountType != null)
        {
            DebitAccount = GetService<AddAccountAction>().Run("TestOne", debitAccountType.Value, AccountingPeriod, AccountingPeriod.PeriodStartDate,
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
            GetService<IAccountRepository>().Add(DebitAccount);
        }
        if (creditAccountType != null && creditAccountType == debitAccountType && sameAccountTypeBehavior == SameAccountTypeBehavior.UseSameAccount)
        {
            CreditAccount = DebitAccount;
        }
        else if (creditAccountType != null)
        {
            CreditAccount = GetService<AddAccountAction>().Run("TestTwo", creditAccountType.Value, AccountingPeriod, AccountingPeriod.PeriodStartDate,
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
            GetService<IAccountRepository>().Add(CreditAccount);
        }
    }
}