using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.ValueObjects;
using Tests.Scenarios;
using Tests.Setups;

namespace Tests.AddTransaction.Setups;

/// <summary>
/// Setup class for a <see cref="AddBalanceEventMultipleBalanceEventScenarios"/> for adding a Transaction
/// </summary>
internal sealed class MultipleBalanceEventScenarioSetup : ScenarioSetup
{
    private readonly AccountingPeriod _futureAccountingPeriod;

    /// <summary>
    /// Accounting Period for the Setup
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; }

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
    /// Debt Account for the Setup
    /// </summary>
    public Account DebtAccount { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="scenario">Scenario for this test case</param>
    public MultipleBalanceEventScenarioSetup(AddBalanceEventMultipleBalanceEventScenario scenario)
    {
        Fund = GetService<AddFundAction>().Run("Test");
        GetService<IFundRepository>().Add(Fund);
        OtherFund = GetService<AddFundAction>().Run("Test2");
        GetService<IFundRepository>().Add(OtherFund);

        AccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(AccountingPeriod);

        _futureAccountingPeriod = GetService<AddAccountingPeriodAction>().Run(2025, 2);
        GetService<IAccountingPeriodRepository>().Add(_futureAccountingPeriod);

        Account = GetService<AddAccountAction>().Run("Test", AccountType.Standard, AccountingPeriod, AccountingPeriod.PeriodStartDate,
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
        GetService<IAccountRepository>().Add(Account);

        DebtAccount = GetService<AddAccountAction>().Run("TestDebt", AccountType.Debt, AccountingPeriod, AccountingPeriod.PeriodStartDate,
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
        GetService<IAccountRepository>().Add(DebtAccount);

        DoScenarioSpecificSetup(scenario);
    }

    /// <summary>
    /// Performs scenario specific setup for the provided scenario
    /// </summary>
    /// <param name="scenario">Scenario for this test case</param>
    private void DoScenarioSpecificSetup(AddBalanceEventMultipleBalanceEventScenario scenario)
    {
        if (scenario == AddBalanceEventMultipleBalanceEventScenario.MultipleEventsSameDay)
        {
            GetService<AddTransactionAction>().Run(AccountingPeriod,
                new DateOnly(2025, 1, 15),
                Account,
                DebtAccount,
                [
                    new FundAmount
                    {
                        Fund = Fund,
                        Amount = 500.00m
                    }
                ]);
        }
        if (scenario == AddBalanceEventMultipleBalanceEventScenario.ForcesFundBalanceNegative)
        {
            GetService<AddTransactionAction>().Run(AccountingPeriod,
                new DateOnly(2025, 1, 10),
                Account,
                DebtAccount,
                [
                    new FundAmount
                    {
                        Fund = Fund,
                        Amount = 1250.00m
                    }
                ]);
        }
        if (scenario == AddBalanceEventMultipleBalanceEventScenario.ForcesAccountBalanceToZero)
        {
            GetService<AddTransactionAction>().Run(AccountingPeriod,
                new DateOnly(2025, 1, 10),
                Account,
                DebtAccount,
                [
                    new FundAmount
                    {
                        Fund = Fund,
                        Amount = 2500.00m
                    }
                ]);
        }
        if (scenario == AddBalanceEventMultipleBalanceEventScenario.ForcesAccountBalanceNegative)
        {
            GetService<AddTransactionAction>().Run(AccountingPeriod,
                new DateOnly(2025, 1, 10),
                Account,
                DebtAccount,
                [
                    new FundAmount
                    {
                        Fund = Fund,
                        Amount = 2750.00m
                    }
                ]);
        }
        if (scenario == AddBalanceEventMultipleBalanceEventScenario.ForcesFutureEventToMakeAccountBalanceNegative)
        {
            GetService<AddTransactionAction>().Run(AccountingPeriod,
                new DateOnly(2025, 1, 20),
                Account,
                DebtAccount,
                [
                    new FundAmount
                    {
                        Fund = Fund,
                        Amount = 2750.00m
                    }
                ]);
        }
        if (scenario == AddBalanceEventMultipleBalanceEventScenario.ForcesAccountBalancesAtEndOfPeriodToBeNegative)
        {
            GetService<AddTransactionAction>().Run(AccountingPeriod,
                new DateOnly(2025, 1, 10),
                Account,
                DebtAccount,
                [
                    new FundAmount
                    {
                        Fund = Fund,
                        Amount = 2750.00m
                    }
                ]);
            GetService<AddTransactionAction>().Run(_futureAccountingPeriod,
                new DateOnly(2025, 1, 10),
                DebtAccount,
                Account,
                [
                    new FundAmount
                    {
                        Fund = Fund,
                        Amount = 2750.00m
                    }
                ]);
        }
    }
}