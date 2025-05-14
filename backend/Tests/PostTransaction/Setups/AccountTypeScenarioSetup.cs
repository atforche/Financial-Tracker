using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Actions;
using Domain.Funds;
using Tests.PostTransaction.Scenarios;
using Tests.Setups;

namespace Tests.PostTransaction.Setups;

/// <summary>
/// Setup class for <see cref="AccountTypeScenarios"/> for posting a Transaction
/// </summary>
internal sealed class AccountTypeScenarioSetup : ScenarioSetup
{
    /// <summary>
    /// Accounting Period for this Setup
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; }

    /// <summary>
    /// Fund for this Setup
    /// </summary>
    public Fund Fund { get; }

    /// <summary>
    /// Debit Account for this Setup
    /// </summary>
    public Account? DebitAccount { get; }

    /// <summary>
    /// Credit Account for this Setup
    /// </summary>
    public Account? CreditAccount { get; }

    /// <summary>
    /// Transaction for this Setup
    /// </summary>
    public Transaction Transaction { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="scenario">Scenario for this test case</param>
    public AccountTypeScenarioSetup(AccountTypeScenario scenario)
    {
        Fund = GetService<AddFundAction>().Run("Test");
        GetService<IFundRepository>().Add(Fund);

        AccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(AccountingPeriod);

        if (scenario is AccountTypeScenario.Debit or AccountTypeScenario.MissingCredit)
        {
            DebitAccount = GetService<AddAccountAction>().Run("TestOne", AccountType.Standard, AccountingPeriod, AccountingPeriod.PeriodStartDate,
                [
                    new FundAmount
                    {
                        Fund = Fund,
                        Amount = 1500.00m,
                    }
                ]);
            GetService<IAccountRepository>().Add(DebitAccount);
        }
        else
        {
            CreditAccount = GetService<AddAccountAction>().Run("TestOne", AccountType.Standard, AccountingPeriod, AccountingPeriod.PeriodStartDate,
                [
                    new FundAmount
                    {
                        Fund = Fund,
                        Amount = 1500.00m,
                    }
                ]);
            GetService<IAccountRepository>().Add(CreditAccount);
        }

        Transaction = GetService<AddTransactionAction>().Run(AccountingPeriod,
            new DateOnly(2025, 1, 15),
            DebitAccount,
            CreditAccount,
            [
                new FundAmount
                {
                    Fund = Fund,
                    Amount = 500.00m
                }
            ]);
    }
}