using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.ChangeInValues;
using Domain.FundConversions;
using Domain.Funds;
using Domain.Transactions;
using Tests.CloseAccountingPeriod.Scenarios;
using Tests.Mocks;
using Tests.Setups;

namespace Tests.CloseAccountingPeriod.Setups;

/// <summary>
/// Setup class for a <see cref="BalanceEventScenarios"/> for closing an Accounting Period
/// </summary>
internal sealed class BalanceEventScenarioSetup : ScenarioSetup
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
    /// First Accounting Period for the Setup
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; }

    /// <summary>
    /// Change In Value for the setup
    /// </summary>
    public ChangeInValue? ChangeInValue { get; }

    /// <summary>
    /// Fund Conversion for the setup
    /// </summary>
    public FundConversion? FundConversion { get; }

    /// <summary>
    /// Transaction for the Setup
    /// </summary>
    public Transaction? Transaction { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="scenario">Scenario for this test case</param>
    public BalanceEventScenarioSetup(BalanceEventScenario scenario)
    {
        Fund = GetService<FundFactory>().Create("Test");
        GetService<IFundRepository>().Add(Fund);
        OtherFund = GetService<FundFactory>().Create("Test2");
        GetService<IFundRepository>().Add(OtherFund);
        GetService<TestUnitOfWork>().SaveChanges();

        AccountingPeriod = GetService<AccountingPeriodFactory>().Create(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(AccountingPeriod);
        GetService<TestUnitOfWork>().SaveChanges();

        Account = GetService<AccountFactory>().Create("Test", AccountType.Standard, AccountingPeriod.Id, AccountingPeriod.PeriodStartDate,
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

        if (scenario is BalanceEventScenario.UnpostedTransaction or BalanceEventScenario.PostedTransaction)
        {
            Transaction = GetService<TransactionFactory>().Create(AccountingPeriod.Id,
                new DateOnly(2025, 1, 15),
                Account.Id,
                null,
                [
                    new FundAmount
                    {
                        FundId = Fund.Id,
                        Amount = 250.00m
                    }
                ]);
            GetService<ITransactionRepository>().Add(Transaction);
            GetService<TestUnitOfWork>().SaveChanges();
        }
        if (scenario is BalanceEventScenario.PostedTransaction)
        {
            GetService<PostTransactionAction>().Run(Transaction ?? throw new InvalidOperationException(),
                TransactionAccountType.Debit,
                new DateOnly(2025, 1, 15));
            GetService<TestUnitOfWork>().SaveChanges();
        }
        if (scenario is BalanceEventScenario.ChangeInValue)
        {
            ChangeInValue = GetService<ChangeInValueFactory>().Create(new CreateChangeInValueRequest
            {
                AccountingPeriodId = AccountingPeriod.Id,
                EventDate = new DateOnly(2025, 1, 15),
                AccountId = Account.Id,
                FundAmount = new FundAmount
                {
                    FundId = Fund.Id,
                    Amount = 250.00m
                }
            });
            GetService<IChangeInValueRepository>().Add(ChangeInValue);
            GetService<TestUnitOfWork>().SaveChanges();
        }
        if (scenario is BalanceEventScenario.FundConversion)
        {
            FundConversion = GetService<FundConversionFactory>().Create(new CreateFundConversionRequest
            {
                AccountingPeriodId = AccountingPeriod.Id,
                EventDate = new DateOnly(2025, 1, 15),
                AccountId = Account.Id,
                FromFundId = Fund.Id,
                ToFundId = OtherFund.Id,
                Amount = 250.00m
            });
            GetService<IFundConversionRepository>().Add(FundConversion);
            GetService<TestUnitOfWork>().SaveChanges();
        }
    }
}