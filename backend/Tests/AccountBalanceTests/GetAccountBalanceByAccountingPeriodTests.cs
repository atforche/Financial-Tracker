using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Validators;

namespace Tests.AccountBalanceTests;

/// <summary>
/// Test class that tests getting an Account's balance by Accounting Period
/// </summary>
public class GetAccountBalanceByAccountingPeriodTests : UnitTestBase
{
    private readonly IAccountingPeriodRepository _accountingPeriodRepository;
    private readonly IAccountingPeriodService _accountingPeriodService;
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountService _accountService;
    private readonly IFundRepository _fundRepository;
    private readonly IFundService _fundService;
    private readonly IAccountBalanceService _accountBalanceService;

    private readonly Fund _testFund;
    private readonly AccountingPeriod _testAccountingPeriod;
    private readonly Account _testAccount;

    /// <summary>
    /// Constructs a new instance of this class.
    /// This constructor is run against before each individual test in this test class
    /// </summary>
    public GetAccountBalanceByAccountingPeriodTests()
    {
        _accountingPeriodRepository = GetService<IAccountingPeriodRepository>();
        _accountingPeriodService = GetService<IAccountingPeriodService>();
        _accountRepository = GetService<IAccountRepository>();
        _accountService = GetService<IAccountService>();
        _fundRepository = GetService<IFundRepository>();
        _fundService = GetService<IFundService>();
        _accountBalanceService = GetService<IAccountBalanceService>();

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
    /// Tests that getting an Account Balance by Accounting Period works as expected
    /// </summary>
    [Fact]
    public void TestAccountBalanceByAccountingPeriod()
    {
        _accountingPeriodService.AddTransaction(_testAccountingPeriod,
            new DateOnly(2024, 11, 5),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 250.00m,
                }
            ]);
        new AccountBalanceByAccountingPeriodValidator(
            _accountBalanceService.GetAccountBalancesByAccountingPeriod(_testAccount, _testAccountingPeriod))
            .Validate(new AccountBalanceByAccountingPeriodState
            {
                AccountingPeriodYear = 2024,
                AccountingPeriodMonth = 11,
                StartingFundBalances =
                [
                    new FundAmountState
                    {
                        FundName = _testFund.Name,
                        Amount = 2500.00m,
                    }
                ],
                EndingFundBalances =
                [
                    new FundAmountState
                    {
                        FundName = _testFund.Name,
                        Amount = 2500.00m,
                    }
                ],
                EndingPendingFundBalanceChanges =
                [
                    new FundAmountState
                    {
                        FundName = _testFund.Name,
                        Amount = -250.00m,
                    }
                ]
            });
    }

    /// <summary>
    /// Tests that getting an Account Balance by Accounting Period works when the Account Balance
    /// has multiple Funds
    /// </summary>
    [Fact]
    public void TestWithMultipleFunds()
    {
        Fund secondFund = _fundService.CreateNewFund("Test2");
        Transaction transaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod,
            new DateOnly(2024, 11, 5),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = secondFund,
                    Amount = 250.00m,
                }
            ]);
        _accountingPeriodService.PostTransaction(transaction, _testAccount, new DateOnly(2024, 11, 5));
        new AccountBalanceByAccountingPeriodValidator(
            _accountBalanceService.GetAccountBalancesByAccountingPeriod(_testAccount, _testAccountingPeriod))
            .Validate(new AccountBalanceByAccountingPeriodState
            {
                AccountingPeriodYear = 2024,
                AccountingPeriodMonth = 11,
                StartingFundBalances =
                [
                    new FundAmountState
                    {
                        FundName = _testFund.Name,
                        Amount = 2500.00m,
                    }
                ],
                EndingFundBalances =
                [
                    new FundAmountState
                    {
                        FundName = _testFund.Name,
                        Amount = 2500.00m,
                    },
                    new FundAmountState
                    {
                        FundName = secondFund.Name,
                        Amount = -250.00m,
                    }
                ],
                EndingPendingFundBalanceChanges = []
            });
    }

    /// <summary>
    /// Tests that getting an Account Balance by Accounting Period with an invalid Accounting Period
    /// will fail
    /// </summary>
    [Fact]
    public void TestWithInvalidAccountingPeriod()
    {
        // Test with an Accounting Period prior to when the account was added
        _accountingPeriodService.ClosePeriod(_testAccountingPeriod);
        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        _accountingPeriodRepository.Add(secondAccountingPeriod);
        Account secondAccount = _accountService.CreateNewAccount("Test2", AccountType.Standard,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 2500.00m,
                }
            ]);
        Assert.Throws<InvalidOperationException>(() => _accountBalanceService.GetAccountBalancesByAccountingPeriod(
            secondAccount,
            _testAccountingPeriod));
    }

    /// <summary>
    /// Tests that getting an Account Balance by Accounting Period works when the Accounting Period has 
    /// balance events that fall in a previous Accounting Period
    /// </summary>
    [Fact]
    public void TestWithBalanceEventsInPastPeriod()
    {
        void ValidateBalances(AccountingPeriod secondAccountingPeriod) => new AccountBalanceByAccountingPeriodValidator(
            _accountBalanceService.GetAccountBalancesByAccountingPeriod(_testAccount, secondAccountingPeriod))
            .Validate(new AccountBalanceByAccountingPeriodState
            {
                AccountingPeriodYear = 2024,
                AccountingPeriodMonth = 12,
                StartingFundBalances =
                [
                    new FundAmountState
                    {
                        FundName = _testFund.Name,
                        Amount = 2500.00m,
                    }
                ],
                EndingFundBalances =
                [
                    new FundAmountState
                    {
                        FundName = _testFund.Name,
                        Amount = 2250.00m,
                    },
                ],
                EndingPendingFundBalanceChanges = []
            });

        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        Transaction transaction = _accountingPeriodService.AddTransaction(secondAccountingPeriod,
            new DateOnly(2024, 11, 15),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 250.00m,
                }
            ]);
        _accountingPeriodService.PostTransaction(transaction, _testAccount, new DateOnly(2024, 11, 25));

        ValidateBalances(secondAccountingPeriod);
        _accountingPeriodService.ClosePeriod(_testAccountingPeriod);
        ValidateBalances(secondAccountingPeriod);
        _accountingPeriodService.ClosePeriod(secondAccountingPeriod);
        ValidateBalances(secondAccountingPeriod);
    }

    /// <summary>
    /// Tests that getting an Account Balance by Accounting Period works when the Accounting Period has
    /// balance events that fall in a future Accounting Period
    /// </summary>
    [Fact]
    public void TestWithBalanceEventsInFuturePeriod()
    {
        void ValidateBalances() => new AccountBalanceByAccountingPeriodValidator(
            _accountBalanceService.GetAccountBalancesByAccountingPeriod(_testAccount, _testAccountingPeriod))
            .Validate(new AccountBalanceByAccountingPeriodState
            {
                AccountingPeriodYear = 2024,
                AccountingPeriodMonth = 11,
                StartingFundBalances =
                [
                    new FundAmountState
                    {
                        FundName = _testFund.Name,
                        Amount = 2500.00m,
                    }
                ],
                EndingFundBalances =
                [
                    new FundAmountState
                    {
                        FundName = _testFund.Name,
                        Amount = 2250.00m,
                    },
                ],
                EndingPendingFundBalanceChanges = []
            });

        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        Transaction transaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod,
            new DateOnly(2024, 12, 15),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 250.00m,
                }
            ]);
        _accountingPeriodService.PostTransaction(transaction, _testAccount, new DateOnly(2024, 12, 25));

        ValidateBalances();
        _accountingPeriodService.ClosePeriod(_testAccountingPeriod);
        ValidateBalances();
        _accountingPeriodService.ClosePeriod(secondAccountingPeriod);
        ValidateBalances();
    }

    /// <summary>
    /// Tests that getting an Account Balance by Accounting Period works if there's balance events from a 
    /// past Accounting Period that fall within the current Accounting Period
    /// </summary>
    [Fact]
    public void TestWithBalanceEventsFromPreviousPeriodInMonth()
    {
        void ValidateBalances(AccountingPeriod secondAccountingPeriod) => new AccountBalanceByAccountingPeriodValidator(
            _accountBalanceService.GetAccountBalancesByAccountingPeriod(_testAccount, secondAccountingPeriod))
            .Validate(new AccountBalanceByAccountingPeriodState
            {
                AccountingPeriodYear = 2024,
                AccountingPeriodMonth = 12,
                StartingFundBalances =
                [
                    new FundAmountState
                    {
                        FundName = _testFund.Name,
                        Amount = 2250.00m,
                    }
                ],
                EndingFundBalances =
                [
                    new FundAmountState
                    {
                        FundName = _testFund.Name,
                        Amount = 2250.00m,
                    },
                ],
                EndingPendingFundBalanceChanges = []
            });

        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        Transaction transaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod,
            new DateOnly(2024, 12, 15),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 250.00m,
                }
            ]);
        _accountingPeriodService.PostTransaction(transaction, _testAccount, new DateOnly(2024, 12, 25));

        ValidateBalances(secondAccountingPeriod);
        _accountingPeriodService.ClosePeriod(_testAccountingPeriod);
        ValidateBalances(secondAccountingPeriod);
        _accountingPeriodService.ClosePeriod(secondAccountingPeriod);
        ValidateBalances(secondAccountingPeriod);
    }

    /// <summary>
    /// Tests that getting an Account Balance by Accounting Period works if there's balance events from a 
    /// future Accounting Period that fall within the current Accounting Period
    /// </summary>
    [Fact]
    public void TestWithBalanceEventsFromFuturePeriodInMonth()
    {
        void ValidateBalances() => new AccountBalanceByAccountingPeriodValidator(
            _accountBalanceService.GetAccountBalancesByAccountingPeriod(_testAccount, _testAccountingPeriod))
            .Validate(new AccountBalanceByAccountingPeriodState
            {
                AccountingPeriodYear = 2024,
                AccountingPeriodMonth = 11,
                StartingFundBalances =
                [
                    new FundAmountState
                    {
                        FundName = _testFund.Name,
                        Amount = 2500.00m,
                    }
                ],
                EndingFundBalances =
                [
                    new FundAmountState
                    {
                        FundName = _testFund.Name,
                        Amount = 2500.00m,
                    },
                ],
                EndingPendingFundBalanceChanges = []
            });

        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        Transaction transaction = _accountingPeriodService.AddTransaction(secondAccountingPeriod,
            new DateOnly(2024, 11, 15),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 250.00m,
                }
            ]);
        _accountingPeriodService.PostTransaction(transaction, _testAccount, new DateOnly(2024, 11, 25));

        ValidateBalances();
        _accountingPeriodService.ClosePeriod(_testAccountingPeriod);
        ValidateBalances();
        _accountingPeriodService.ClosePeriod(secondAccountingPeriod);
        ValidateBalances();
    }
}