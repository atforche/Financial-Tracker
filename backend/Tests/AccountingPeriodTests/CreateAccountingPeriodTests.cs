using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Validators;

namespace Tests.AccountingPeriodTests;

/// <summary>
/// Test class that tests creating an AccountingPeriod
/// </summary>
public class CreateAccountingPeriodTests : UnitTestBase
{
    private readonly IAccountingPeriodRepository _accountingPeriodRepository;
    private readonly IAccountingPeriodService _accountingPeriodService;
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountService _accountService;
    private readonly IFundRepository _fundRepository;
    private readonly IFundService _fundService;

    /// <summary>
    /// Constructs a new instance of this class.
    /// This constructor is run again before each individual test in this test class.
    /// </summary>
    public CreateAccountingPeriodTests()
        : base()
    {
        _accountingPeriodRepository = GetService<IAccountingPeriodRepository>();
        _accountingPeriodService = GetService<IAccountingPeriodService>();
        _accountRepository = GetService<IAccountRepository>();
        _accountService = GetService<IAccountService>();
        _fundRepository = GetService<IFundRepository>();
        _fundService = GetService<IFundService>();
    }

    /// <summary>
    /// Tests a new Accounting Period can be added successfully
    /// </summary>
    [Fact]
    public void TestCreateAccountingPeriod()
    {
        AccountingPeriod newAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 11);
        new AccountingPeriodValidator(newAccountingPeriod).Validate(new AccountingPeriodState
        {
            Year = 2024,
            Month = 11,
            IsOpen = true,
            AccountBalanceCheckpoints = [],
            Transactions = [],
        });
    }

    /// <summary>
    /// Tests that creating an Accounting Period with an invalid year will fail
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(3000)]
    public void TestWithInvalidYear(int value)
    {
        const int month = 11;

        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.CreateNewAccountingPeriod(value, month));
    }

    /// <summary>
    /// Tests that creating an Accounting Period with an invalid month will fail
    /// </summary>
    /// <param name="value">Value to test</param>
    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void TestWithInvalidMonth(int value)
    {
        const int year = 2023;

        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.CreateNewAccountingPeriod(year, value));
    }

    /// <summary>
    /// Tests that multiple Accounting Periods can be created
    /// </summary>
    [Fact]
    public void TestWithMultipleAccountingPeriods()
    {
        // Add a first Accounting Period and mark it closed
        AccountingPeriod firstAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 11);
        _accountingPeriodRepository.Add(firstAccountingPeriod);
        _accountingPeriodService.ClosePeriod(firstAccountingPeriod);

        // Add a second accounting period
        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        new AccountingPeriodValidator(secondAccountingPeriod).Validate(new AccountingPeriodState
        {
            Year = 2024,
            Month = 12,
            IsOpen = true,
            AccountBalanceCheckpoints = [],
            Transactions = [],
        });
    }

    /// <summary>
    /// Tests that a new Accounting Period can be created if the previous Accounting Period is still open
    /// </summary>
    [Fact]
    public void TestWithMultipleOpenAccountingPeriods()
    {
        // Add a first Accounting Period and leave it open
        AccountingPeriod firstAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 11);
        _accountingPeriodRepository.Add(firstAccountingPeriod);

        // Add a second accounting period
        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        new AccountingPeriodValidator(secondAccountingPeriod).Validate(new AccountingPeriodState
        {
            Year = 2024,
            Month = 12,
            IsOpen = true,
            AccountBalanceCheckpoints = [],
            Transactions = [],
        });
    }

    /// <summary>
    /// Tests that creating an invalid Accounting Period will fail
    /// </summary>
    [Fact]
    public void TestWithInvalidAccountingPeriod()
    {
        // Add a first Accounting Period
        AccountingPeriod firstAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 11);
        _accountingPeriodRepository.Add(firstAccountingPeriod);

        // Attempt to add a second Accounting Period with a gap 
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.CreateNewAccountingPeriod(2025, 1));

        // Attempt to add a second Accounting Period in the past
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.CreateNewAccountingPeriod(2024, 10));

        // Attempt to add a duplicate Accounting Period
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.CreateNewAccountingPeriod(2024, 11));
    }

    /// <summary>
    /// Tests that creating an Accounting Period when there's a previous open Accounting Period with an Account
    /// won't create any Account Balance Checkpoints
    /// </summary>
    [Fact]
    public void TestWithNoBalanceCheckpoints()
    {
        // Add a test fund
        Fund fund = _fundService.CreateNewFund("Test");
        _fundRepository.Add(fund);

        // Add a first Accounting Period
        AccountingPeriod firstAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 11);
        _accountingPeriodRepository.Add(firstAccountingPeriod);

        // Add an Account with an initial period of the first Accounting Period
        Account account = _accountService.CreateNewAccount("Test", AccountType.Standard,
            [
                new FundAmount
                {
                    Fund = fund,
                    Amount = 2500.0m,
                }
            ]);
        _accountRepository.Add(account);

        // Add a new accounting period, leaving the first accounting period open
        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        new AccountingPeriodValidator(secondAccountingPeriod).Validate(new AccountingPeriodState
        {
            Year = 2024,
            Month = 12,
            IsOpen = true,
            AccountBalanceCheckpoints = [],
            Transactions = [],
        });
    }

    /// <summary>
    /// Tests that creating an Accounting Period when there's a previous closed Accounting Period with an Account
    /// will create Account Balance Checkpoints
    /// </summary>
    [Fact]
    public void TestWithBalanceCheckpoints()
    {
        // Add a test fund
        Fund fund = _fundService.CreateNewFund("Test");
        _fundRepository.Add(fund);

        // Add a first Accounting Period
        AccountingPeriod firstAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 11);
        _accountingPeriodRepository.Add(firstAccountingPeriod);

        // Add an Account with an initial period of the first Accounting Period
        Account account = _accountService.CreateNewAccount("Test", AccountType.Standard,
            [
                new FundAmount
                {
                    Fund = fund,
                    Amount = 2500.00m,
                }
            ]);
        _accountRepository.Add(account);

        // Close the first accounting period
        _accountingPeriodService.ClosePeriod(firstAccountingPeriod);

        // Add a new accounting period
        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        new AccountingPeriodValidator(secondAccountingPeriod).Validate(new AccountingPeriodState
        {
            Year = 2024,
            Month = 12,
            IsOpen = true,
            AccountBalanceCheckpoints =
            [
                new AccountBalanceCheckpointState
                {
                    AccountName = "Test",
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = "Test",
                            Amount = 2500.00m,
                        }
                    ]
                },
            ],
            Transactions = [],
        });
    }

    /// <summary>
    /// Tests that creating an Accounting Period when there's a Transaction that falls within the calendar 
    /// month for the Accounting Period being created works as expected
    /// </summary>
    [Fact]
    public void TestWithExistingTransactionInCurrentPeriod()
    {
        // Add a test fund
        Fund fund = _fundService.CreateNewFund("Test");
        _fundRepository.Add(fund);

        // Add a first Accounting Period
        AccountingPeriod firstAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 11);
        _accountingPeriodRepository.Add(firstAccountingPeriod);

        // Add an Account with an initial period of the first Accounting Period
        Account account = _accountService.CreateNewAccount("Test", AccountType.Standard,
            [
                new FundAmount
                {
                    Fund = fund,
                    Amount = 2500.0m,
                }
            ]);
        _accountRepository.Add(account);

        // Add a Transaction with a Balance Event that falls in the future month
        Transaction transaction = _accountingPeriodService.AddTransaction(firstAccountingPeriod, new DateOnly(2024, 11, 25), account, null,
            [
                new FundAmount
                {
                    Fund = fund,
                    Amount = 25.00m
                }
            ]);
        transaction.Post(account, new DateOnly(2024, 12, 5));

        // Close the first accounting period
        _accountingPeriodService.ClosePeriod(firstAccountingPeriod);

        // Add a new accounting period
        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        new AccountingPeriodValidator(secondAccountingPeriod).Validate(new AccountingPeriodState
        {
            Year = 2024,
            Month = 12,
            IsOpen = true,
            AccountBalanceCheckpoints =
            [
                new AccountBalanceCheckpointState
                {
                    AccountName = "Test",
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = "Test",
                            Amount = 2475.00m,
                        }
                    ]
                },
            ],
            Transactions = [],
        });
    }
}