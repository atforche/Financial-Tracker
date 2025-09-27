using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Tests.Old.AddTransaction.Scenarios;
using Tests.Old.Mocks;
using Tests.Old.Setups;

namespace Tests.Old.AddTransaction.Setups;

/// <summary>
/// Setup class for a <see cref="AccountScenarios"/> for adding a Transaction
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
        Fund = GetService<FundService>().Create("Test", "");
        GetService<IFundRepository>().Add(Fund);
        OtherFund = GetService<FundService>().Create("OtherTest", "");
        GetService<IFundRepository>().Add(OtherFund);
        GetService<TestUnitOfWork>().SaveChanges();

        AccountingPeriod = GetService<AccountingPeriodFactory>().Create(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(AccountingPeriod);
        GetService<TestUnitOfWork>().SaveChanges();

        if (debitAccountType != null)
        {
            DebitAccount = GetService<AccountFactory>().Create("TestOne", debitAccountType.Value, AccountingPeriod.Id, AccountingPeriod.PeriodStartDate,
                [
                    new FundAmount
                    {
                        FundId = Fund.Id,
                        Amount = 1500.00m,
                    },
                    new FundAmount
                    {
                        FundId = OtherFund.Id,
                        Amount = 1500.00m
                    }
                ]);
            GetService<IAccountRepository>().Add(DebitAccount);
            GetService<TestUnitOfWork>().SaveChanges();
        }
        if (creditAccountType != null && creditAccountType == debitAccountType && sameAccountTypeBehavior == SameAccountTypeBehavior.UseSameAccount)
        {
            CreditAccount = DebitAccount;
        }
        else if (creditAccountType != null)
        {
            CreditAccount = GetService<AccountFactory>().Create("TestTwo", creditAccountType.Value, AccountingPeriod.Id, AccountingPeriod.PeriodStartDate,
                [
                    new FundAmount
                    {
                        FundId = Fund.Id,
                        Amount = 1500.00m,
                    },
                    new FundAmount
                    {
                        FundId = OtherFund.Id,
                        Amount = 1500.00m
                    }
                ]);
            GetService<IAccountRepository>().Add(CreditAccount);
            GetService<TestUnitOfWork>().SaveChanges();
        }
    }
}