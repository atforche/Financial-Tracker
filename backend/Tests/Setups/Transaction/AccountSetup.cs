using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Scenarios;

namespace Tests.Setups.Transaction;

/// <summary>
/// Setup class for an Account test case
/// </summary>
internal sealed class AccountSetup : TestCaseSetup
{
    /// <summary>
    /// Fund for this Account Setup
    /// </summary>
    public Fund Fund { get; }

    /// <summary>
    /// Other Fund for this Account Setup
    /// </summary>
    public Fund OtherFund { get; }

    /// <summary>
    /// Accounting Period for this Account Setup
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; }

    /// <summary>
    /// Debit Account for this Account Setup
    /// </summary>
    public Account? DebitAccount { get; }

    /// <summary>
    /// Credit Account for this Account Setup
    /// </summary>
    public Account? CreditAccount { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="debitAccountType">Account Type for the Debit Account</param>
    /// <param name="creditAccountType">Account Type for the Credit Account</param>
    /// <param name="sameAccountTypeBehavior">Behavior to use if the same Account type is provided for both the debit and credit Accounts</param>
    public AccountSetup(
        AccountType? debitAccountType,
        AccountType? creditAccountType,
        SameAccountTypeBehavior sameAccountTypeBehavior = SameAccountTypeBehavior.UseDifferentAccounts)
    {
        IFundService fundService = GetService<IFundService>();
        IFundRepository fundRepository = GetService<IFundRepository>();
        Fund = fundService.CreateNewFund("Test");
        fundRepository.Add(Fund);
        OtherFund = fundService.CreateNewFund("OtherTest");
        fundRepository.Add(OtherFund);

        AccountingPeriod = GetService<IAccountingPeriodService>().CreateNewAccountingPeriod(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(AccountingPeriod);

        IAccountService accountService = GetService<IAccountService>();
        IAccountRepository accountRepository = GetService<IAccountRepository>();
        if (debitAccountType != null)
        {
            DebitAccount = accountService.CreateNewAccount("TestOne", debitAccountType.Value,
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
            CreditAccount = accountService.CreateNewAccount("TestTwo", creditAccountType.Value,
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

    /// <summary>
    /// Gets the collection of Transaction Account scenarios
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<TheoryDataRow<AccountType?, AccountType?, SameAccountTypeBehavior>> GetCollection()
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