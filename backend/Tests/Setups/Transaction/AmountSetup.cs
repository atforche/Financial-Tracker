using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;

namespace Tests.Setups.Transaction;

/// <summary>
/// Setup class for a Transaction Amount test case
/// </summary>
internal sealed class AmountSetup : TestCaseSetup
{
    private static readonly List<EventAmountScenario> accountIndependentScenarios =
    [
        EventAmountScenario.Positive,
        EventAmountScenario.Zero,
        EventAmountScenario.Negative
    ];

    /// <summary>
    /// Fund for this Amount Setup
    /// </summary>
    public Fund Fund { get; }

    /// <summary>
    /// Other Fund for this Amount Setup
    /// </summary>
    public Fund OtherFund { get; }

    /// <summary>
    /// Accounting Period for this Amount Setup
    /// </summary>
    public AccountingPeriod AccountingPeriod { get; }

    /// <summary>
    /// Standard Account for this Amount Setup
    /// </summary>
    public Account StandardAccount { get; }

    /// <summary>
    /// Debt Account for this Amount Setup
    /// </summary>
    public Account DebtAccount { get; }

    /// <summary>
    /// Amount for this Amount Setup
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="scenario">Scenario for this Amount Setup</param>
    /// <param name="accountType">Account on the Transaction this scenario should be applied to</param>
    public AmountSetup(EventAmountScenario scenario, TransactionAccountType? accountType)
    {
        if (!accountIndependentScenarios.Contains(scenario) && accountType == null)
        {
            throw new InvalidOperationException();
        }

        IFundService fundService = GetService<IFundService>();
        IFundRepository fundRepository = GetService<IFundRepository>();
        Fund = fundService.CreateNewFund("Test");
        fundRepository.Add(Fund);
        OtherFund = fundService.CreateNewFund("Test2");
        fundRepository.Add(OtherFund);

        AccountingPeriod = GetService<IAccountingPeriodService>().CreateNewAccountingPeriod(2025, 1);
        GetService<IAccountingPeriodRepository>().Add(AccountingPeriod);

        IAccountService accountService = GetService<IAccountService>();
        IAccountRepository accountRepository = GetService<IAccountRepository>();
        StandardAccount = accountService.CreateNewAccount("TestOne", AccountType.Standard,
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
        accountRepository.Add(StandardAccount);
        DebtAccount = accountService.CreateNewAccount("TestTwo", AccountType.Debt,
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
        accountRepository.Add(DebtAccount);

        Amount = new EventAmountSetup(scenario).Amount;
        if (!accountIndependentScenarios.Contains(scenario))
        {
            Amount = Math.Abs(Amount);
        }
    }

    /// <summary>
    /// Gets a collection of Transaction Amount scenarios
    /// </summary>
    public static IEnumerable<TheoryDataRow<EventAmountScenario, TransactionAccountType?>> GetCollection()
    {
        foreach (EventAmountScenario scenario in EventAmountSetup.GetCollection().Select(row => row.Data))
        {
            if (accountIndependentScenarios.Contains(scenario))
            {
                yield return new TheoryDataRow<EventAmountScenario, TransactionAccountType?>(scenario, null);
            }
            else
            {
                foreach (TransactionAccountType accountType in Enum.GetValues<TransactionAccountType>())
                {
                    yield return new TheoryDataRow<EventAmountScenario, TransactionAccountType?>(scenario, accountType);
                }
            }
        }
    }
}