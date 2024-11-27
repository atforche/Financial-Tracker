using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Validators;

namespace Tests.AccountingPeriodTests;

/// <summary>
/// Test class that tests adding a Transaction to an Accounting Period
/// </summary>
public class AddTransactionTests : UnitTestBase
{
    private readonly IAccountingPeriodRepository _accountingPeriodRepository;
    private readonly IAccountingPeriodService _accountingPeriodService;
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountService _accountService;
    private readonly IFundRepository _fundRepository;
    private readonly IFundService _fundService;

    private readonly Fund _testFund;
    private readonly AccountingPeriod _testAccountingPeriod;
    private readonly Account _testAccount;


    /// <summary>
    /// Constructs a new instance of this class.
    /// This constructor is run against before each individual test in this test class.
    /// </summary>
    public AddTransactionTests()
    {
        _accountingPeriodRepository = GetService<IAccountingPeriodRepository>();
        _accountingPeriodService = GetService<IAccountingPeriodService>();
        _accountRepository = GetService<IAccountRepository>();
        _accountService = GetService<IAccountService>();
        _fundRepository = GetService<IFundRepository>();
        _fundService = GetService<IFundService>();

        // Setup shared by all tests
        _testFund = _fundService.CreateNewFund("Test");
        _fundRepository.Add(_testFund);
        _testAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 11);
        _accountingPeriodRepository.Add(_testAccountingPeriod);
        _testAccount = _accountService.CreateNewAccount("Test", AccountType.Standard,
            [
                new FundAmount()
                {
                    Fund = _testFund,
                    Amount = 2500.00m
                }
            ]);
        _accountRepository.Add(_testAccount);
    }

    /// <summary>
    /// Tests that a Transaction can be added successfully
    /// </summary>
    [Fact]
    public void AddTransaction()
    {
        Transaction transaction = _testAccountingPeriod.AddTransaction(new DateOnly(2024, 11, 26), _testAccount, null,
            [
                new FundAmount()
                {
                    Fund = _testFund,
                    Amount = 25.00m,
                }
            ]);
        new TransactionValidator(transaction).Validate(new TransactionState
        {
            TransactionDate = new DateOnly(2024, 11, 26),
            AccountingEntries =
            [
                new FundAmountState
                {
                    FundName = _testFund.Name,
                    Amount = 25.00m,
                }
            ],
            TransactionBalanceEvents =
            [
                new TransactionBalanceEventState
                {
                    AccountName = _testAccount.Name,
                    EventDate = new DateOnly(2024, 11, 26),
                    EventSequence = 1,
                    TransactionEventType = TransactionBalanceEventType.Added,
                    TransactionAccountType = TransactionAccountType.Debit,
                }
            ]
        });
    }

    /// <summary>
    /// Tests that adding a Transaction to a closed Accounting Period fails
    /// </summary>
    [Fact]
    public void AddTransactionToClosedAccountingPeriod()
    {
        _accountingPeriodService.ClosePeriod(_testAccountingPeriod);
        Assert.Throws<InvalidOperationException>(() => _testAccountingPeriod.AddTransaction(new DateOnly(2024, 11, 25),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 25.00m,
                }
            ]));
    }

    /// <summary>
    /// Tests that a Transaction can be added to an Accounting Period when its transaction date falls in a different Accounting Period
    /// </summary>
    [Fact]
    public void AddTransactionThatFallsInDifferentAccountingPeriod()
    {
        // Add an additional Accounting Period
        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        _accountingPeriodRepository.Add(secondAccountingPeriod);

        // Add a Transaction to the first Accounting Period that falls in the second period
        Transaction transaction = _testAccountingPeriod.AddTransaction(new DateOnly(2024, 12, 25), _testAccount, null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 25.00m,
                }
            ]);
        new TransactionValidator(transaction).Validate(new TransactionState
        {
            TransactionDate = new DateOnly(2024, 12, 25),
            AccountingEntries =
            [
                new FundAmountState
                {
                    FundName = _testFund.Name,
                    Amount = 25.00m
                }
            ],
            TransactionBalanceEvents =
            [
                new TransactionBalanceEventState
                {
                    AccountName = _testAccount.Name,
                    EventDate = new DateOnly(2024, 12, 25),
                    EventSequence = 1,
                    TransactionEventType = TransactionBalanceEventType.Added,
                    TransactionAccountType = TransactionAccountType.Debit,
                }
            ]
        });

        // Add a Transaction to the second Accounting Period that falls in the first period
        transaction = secondAccountingPeriod.AddTransaction(new DateOnly(2024, 11, 25), _testAccount, null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 25.00m,
                }
            ]);
        new TransactionValidator(transaction).Validate(new TransactionState
        {
            TransactionDate = new DateOnly(2024, 11, 25),
            AccountingEntries =
            [
                new FundAmountState
                {
                    FundName = _testFund.Name,
                    Amount = 25.00m
                }
            ],
            TransactionBalanceEvents =
            [
                new TransactionBalanceEventState
                {
                    AccountName = _testAccount.Name,
                    EventDate = new DateOnly(2024, 11, 25),
                    EventSequence = 1,
                    TransactionEventType = TransactionBalanceEventType.Added,
                    TransactionAccountType = TransactionAccountType.Debit,
                }
            ]
        });
    }
}