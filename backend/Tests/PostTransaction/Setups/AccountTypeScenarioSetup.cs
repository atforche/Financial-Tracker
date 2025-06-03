using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
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
        Fund = GetService<FundFactory>().Create("Test");
        GetService<IFundRepository>().Add(Fund);

        AccountingPeriod = GetService<AccountingPeriodFactory>().Create(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(AccountingPeriod);

        if (scenario is AccountTypeScenario.Debit or AccountTypeScenario.MissingCredit)
        {
            DebitAccount = GetService<AccountFactory>().Create("TestOne", AccountType.Standard, AccountingPeriod.Id, AccountingPeriod.PeriodStartDate,
                [
                    new FundAmount
                    {
                        FundId = Fund.Id,
                        Amount = 1500.00m,
                    }
                ]);
            GetService<IAccountRepository>().Add(DebitAccount);
        }
        else
        {
            CreditAccount = GetService<AccountFactory>().Create("TestOne", AccountType.Standard, AccountingPeriod.Id, AccountingPeriod.PeriodStartDate,
                [
                    new FundAmount
                    {
                        FundId = Fund.Id,
                        Amount = 1500.00m,
                    }
                ]);
            GetService<IAccountRepository>().Add(CreditAccount);
        }

        Transaction = GetService<TransactionFactory>().Create(AccountingPeriod.Id,
            new DateOnly(2025, 1, 15),
            DebitAccount?.Id,
            CreditAccount?.Id,
            [
                new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 500.00m
                }
            ]);
        GetService<ITransactionRepository>().Add(Transaction);
    }
}