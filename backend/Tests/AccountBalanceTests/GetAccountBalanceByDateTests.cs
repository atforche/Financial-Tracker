using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Validators;

namespace Tests.AccountBalanceTests;

/// <summary>
/// Test class that tests getting an Account's balance by date
/// </summary>
public class GetAccountBalanceByDateTests : UnitTestBase
{
    private readonly IAccountBalanceService _accountBalanceService;
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
    /// This constructor is run again before each individual test in this test class
    /// </summary>
    public GetAccountBalanceByDateTests()
    {
        _accountBalanceService = GetService<IAccountBalanceService>();
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
    /// Tests that getting an Account Balance by Date works as expected
    /// </summary>
    [Fact]
    public void TestAccountBalanceByDate()
    {
        IEnumerable<AccountBalanceByDate> accountBalanceByDates = _accountBalanceService.GetAccountBalancesByDate(
            _testAccount,
            new DateRange(new DateOnly(2024, 11, 25), new DateOnly(2024, 11, 27)));
        new AccountBalanceByDateValidator(accountBalanceByDates).Validate(
            [
                new AccountBalanceByDateState
                {
                    Date = new DateOnly(2024, 11, 25),
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = _testFund.Name,
                            Amount = 2500.00m
                        }
                    ],
                    PendingFundBalanceChanges = [],
                },
                new AccountBalanceByDateState
                {
                    Date = new DateOnly(2024, 11, 26),
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = _testFund.Name,
                            Amount = 2500.00m
                        }
                    ],
                    PendingFundBalanceChanges = [],
                },
                new AccountBalanceByDateState
                {
                    Date = new DateOnly(2024, 11, 27),
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = _testFund.Name,
                            Amount = 2500.00m
                        }
                    ],
                    PendingFundBalanceChanges = [],
                }
            ]);
    }

    /// <summary>
    /// Tests that getting an Account Balance by Date works when the Account Balance has multiple Funds
    /// </summary>
    [Fact]
    public void TestWithMultipleFunds()
    {
        // Add a second fund
        Fund secondFund = _fundService.CreateNewFund("Test2");
        _fundRepository.Add(secondFund);

        // Add a transaction and post it
        Transaction transaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod, new DateOnly(2024, 11, 26),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = secondFund,
                    Amount = 100.00m
                }
            ]);
        transaction.Post(_testAccount, new DateOnly(2024, 11, 27));

        // Validate the account balances in the test account
        IEnumerable<AccountBalanceByDate> accountBalanceByDates = _accountBalanceService.GetAccountBalancesByDate(
            _testAccount,
            new DateRange(new DateOnly(2024, 11, 25), new DateOnly(2024, 11, 27)));
        new AccountBalanceByDateValidator(accountBalanceByDates).Validate(
            [
                new AccountBalanceByDateState
                {
                    Date = new DateOnly(2024, 11, 25),
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = _testFund.Name,
                            Amount = 2500.00m
                        }
                    ],
                    PendingFundBalanceChanges = [],
                },
                new AccountBalanceByDateState
                {
                    Date = new DateOnly(2024, 11, 26),
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = _testFund.Name,
                            Amount = 2500.00m
                        }
                    ],
                    PendingFundBalanceChanges =
                    [
                        new FundAmountState
                        {
                            FundName = secondFund.Name,
                            Amount = -100.00m,
                        }
                    ],
                },
                new AccountBalanceByDateState
                {
                    Date = new DateOnly(2024, 11, 27),
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = _testFund.Name,
                            Amount = 2500.00m
                        },
                        new FundAmountState
                        {
                            FundName = secondFund.Name,
                            Amount = -100.00m
                        },
                    ],
                    PendingFundBalanceChanges = [],
                },
            ]);
    }

    /// <summary>
    /// Tests that getting an Account Balance by Date with an invalid Date Range will fail
    /// </summary>
    [Fact]
    public void TestWithInvalidDateRanges()
    {
        // Test with a date range that falls prior to any accounting periods
        Assert.Throws<InvalidOperationException>(() => _accountBalanceService.GetAccountBalancesByDate(
            _testAccount,
            new DateRange(new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31))));

        // Test with a date range that falls prior to this account being added
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
        Assert.Throws<InvalidOperationException>(() => _accountBalanceService.GetAccountBalancesByDate(
            secondAccount,
            new DateRange(new DateOnly(2024, 11, 1), new DateOnly(2024, 11, 30))));

        // Test with a date range that falls after any accounting periods
        Assert.Throws<InvalidOperationException>(() => _accountBalanceService.GetAccountBalancesByDate(
            _testAccount,
            new DateRange(new DateOnly(2025, 11, 1), new DateOnly(2025, 11, 30))));
    }

    /// <summary>
    /// Tests that getting an Account Balance by Date works across an Accounting Period boundary
    /// </summary>
    [Fact]
    public void TestAcrossAccountingPeriodBoundary()
    {
        void ValidateBalances() => new AccountBalanceByDateValidator(
            _accountBalanceService.GetAccountBalancesByDate(
                _testAccount,
                new DateRange(new DateOnly(2024, 11, 30), new DateOnly(2024, 12, 1))))
            .Validate(
                [
                    new AccountBalanceByDateState
                    {
                        Date = new DateOnly(2024, 11, 30),
                        FundBalances =
                        [
                            new FundAmountState
                            {
                                FundName = _testFund.Name,
                                Amount = 2450.00m,
                            }
                        ],
                        PendingFundBalanceChanges = []
                    },
                    new AccountBalanceByDateState
                    {
                        Date = new DateOnly(2024, 12, 1),
                        FundBalances =
                        [
                            new FundAmountState
                            {
                                FundName = _testFund.Name,
                                Amount = 2350.00m,
                            }
                        ],
                        PendingFundBalanceChanges = []
                    }
                ]);

        // Add a second accounting period
        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        _accountingPeriodRepository.Add(secondAccountingPeriod);

        Transaction firstTransaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod,
            new DateOnly(2024, 11, 30),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 50.00m,
                }
            ]);
        Transaction secondTransaction = _accountingPeriodService.AddTransaction(secondAccountingPeriod,
            new DateOnly(2024, 12, 1),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 100.00m,
                }
            ]);
        firstTransaction.Post(_testAccount, new DateOnly(2024, 11, 30));
        secondTransaction.Post(_testAccount, new DateOnly(2024, 12, 1));

        ValidateBalances();
        _accountingPeriodService.ClosePeriod(_testAccountingPeriod);
        ValidateBalances();
        _accountingPeriodService.ClosePeriod(secondAccountingPeriod);
        ValidateBalances();
    }

    /// <summary>
    /// Tests that getting an Account Balance by Date works across an Accounting Period boundary where 
    /// transactions in the first Accounting Period fall after transactions in the second
    /// </summary>
    [Fact]
    public void TestWithMixedAccountingPeriodBoundary()
    {
        void ValidateBalances() => new AccountBalanceByDateValidator(
            _accountBalanceService.GetAccountBalancesByDate(
                _testAccount,
                new DateRange(new DateOnly(2024, 11, 30), new DateOnly(2024, 12, 1))))
            .Validate(
                [
                    new AccountBalanceByDateState
                    {
                        Date = new DateOnly(2024, 11, 30),
                        FundBalances =
                        [
                            new FundAmountState
                            {
                                FundName = _testFund.Name,
                                Amount = 2450.00m,
                            }
                        ],
                        PendingFundBalanceChanges = []
                    },
                    new AccountBalanceByDateState
                    {
                        Date = new DateOnly(2024, 12, 1),
                        FundBalances =
                        [
                            new FundAmountState
                            {
                                FundName = _testFund.Name,
                                Amount = 2350.00m,
                            }
                        ],
                        PendingFundBalanceChanges = []
                    }
                ]);

        // Add a second accounting period
        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        _accountingPeriodRepository.Add(secondAccountingPeriod);

        Transaction firstTransaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod,
            new DateOnly(2024, 12, 1),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 100.00m,
                }
            ]);
        Transaction secondTransaction = _accountingPeriodService.AddTransaction(secondAccountingPeriod,
            new DateOnly(2024, 11, 30),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 50.00m,
                }
            ]);

        firstTransaction.Post(_testAccount, new DateOnly(2024, 12, 1));
        secondTransaction.Post(_testAccount, new DateOnly(2024, 11, 30));

        ValidateBalances();
        _accountingPeriodService.ClosePeriod(_testAccountingPeriod);
        ValidateBalances();
        _accountingPeriodService.ClosePeriod(secondAccountingPeriod);
        ValidateBalances();
    }

    /// <summary>
    /// Tests that getting an Account Balance by Date works when a balance event from a past Accounting Period
    /// falls in the current Accounting Period before the provided date range
    /// </summary>
    [Fact]
    public void TestWithPastPeriodBalanceEventFallingBeforeDateRange()
    {
        void ValidateBalances() => new AccountBalanceByDateValidator(
            _accountBalanceService.GetAccountBalancesByDate(
                _testAccount,
                new DateRange(new DateOnly(2024, 12, 20), new DateOnly(2024, 12, 21))))
            .Validate(
                [
                    new AccountBalanceByDateState
                    {
                        Date = new DateOnly(2024, 12, 20),
                        FundBalances =
                        [
                            new FundAmountState
                            {
                                FundName = _testFund.Name,
                                Amount = 2250.00m,
                            }
                        ],
                        PendingFundBalanceChanges =
                        [
                            new FundAmountState
                            {
                                FundName = _testFund.Name,
                                Amount = -300.00m,
                            }
                        ]
                    },
                    new AccountBalanceByDateState
                    {
                        Date = new DateOnly(2024, 12, 21),
                        FundBalances =
                        [
                            new FundAmountState
                            {
                                FundName = _testFund.Name,
                                Amount = 1950.00m,
                            }
                        ],
                        PendingFundBalanceChanges = []
                    }
                ]);

        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        _accountingPeriodRepository.Add(secondAccountingPeriod);

        Transaction firstTransaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod,
            new DateOnly(2024, 12, 10),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 250.00m,
                }
            ]);
        Transaction secondTransaction = _accountingPeriodService.AddTransaction(secondAccountingPeriod,
            new DateOnly(2024, 12, 20),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 300.00m,
                }
            ]);
        firstTransaction.Post(_testAccount, new DateOnly(2024, 12, 11));
        secondTransaction.Post(_testAccount, new DateOnly(2024, 12, 21));

        ValidateBalances();
        _accountingPeriodService.ClosePeriod(_testAccountingPeriod);
        ValidateBalances();
        _accountingPeriodService.ClosePeriod(secondAccountingPeriod);
        ValidateBalances();
    }

    /// <summary>
    /// Tests that getting an Account Balance by Date works when a balance event from a past Accounting Period
    /// falls in the current Accounting Period during the provided date range
    /// </summary>
    [Fact]
    public void TestWithPastPeriodBalanceEventFallingInDateRange()
    {
        void ValidateBalances() => new AccountBalanceByDateValidator(
            _accountBalanceService.GetAccountBalancesByDate(
                _testAccount,
                new DateRange(new DateOnly(2024, 12, 10), new DateOnly(2024, 12, 11))))
            .Validate(
                [
                    new AccountBalanceByDateState
                    {
                        Date = new DateOnly(2024, 12, 10),
                        FundBalances =
                        [
                            new FundAmountState
                            {
                                FundName = _testFund.Name,
                                Amount = 2500.00m,
                            }
                        ],
                        PendingFundBalanceChanges =
                        [
                            new FundAmountState
                            {
                                FundName = _testFund.Name,
                                Amount = -550.00m,
                            }
                        ]
                    },
                    new AccountBalanceByDateState
                    {
                        Date = new DateOnly(2024, 12, 11),
                        FundBalances =
                        [
                            new FundAmountState
                            {
                                FundName = _testFund.Name,
                                Amount = 1950.00m,
                            }
                        ],
                        PendingFundBalanceChanges = []
                    }
                ]);

        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        _accountingPeriodRepository.Add(secondAccountingPeriod);

        Transaction firstTransaction = _accountingPeriodService.AddTransaction(secondAccountingPeriod,
            new DateOnly(2024, 12, 10),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 250.00m,
                }
            ]);
        Transaction secondTransaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod,
            new DateOnly(2024, 12, 10),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 300.00m,
                }
            ]);
        firstTransaction.Post(_testAccount, new DateOnly(2024, 12, 11));
        secondTransaction.Post(_testAccount, new DateOnly(2024, 12, 11));

        ValidateBalances();
        _accountingPeriodService.ClosePeriod(_testAccountingPeriod);
        ValidateBalances();
        _accountingPeriodService.ClosePeriod(secondAccountingPeriod);
        ValidateBalances();
    }

    /// <summary>
    /// Tests that getting an Account Balance by Date works when a balance event from a past Accounting Period
    /// falls in the current Accounting Period after the provided date range
    /// </summary>
    [Fact]
    public void TestWithPastPeriodBalanceEventFallingAfterDateRange()
    {
        void ValidateBalances() => new AccountBalanceByDateValidator(
            _accountBalanceService.GetAccountBalancesByDate(
                _testAccount,
                new DateRange(new DateOnly(2024, 12, 10), new DateOnly(2024, 12, 11))))
            .Validate(
                [
                    new AccountBalanceByDateState
                    {
                        Date = new DateOnly(2024, 12, 10),
                        FundBalances =
                        [
                            new FundAmountState
                            {
                                FundName = _testFund.Name,
                                Amount = 2500.00m,
                            }
                        ],
                        PendingFundBalanceChanges =
                        [
                            new FundAmountState
                            {
                                FundName = _testFund.Name,
                                Amount = -250.00m,
                            }
                        ]
                    },
                    new AccountBalanceByDateState
                    {
                        Date = new DateOnly(2024, 12, 11),
                        FundBalances =
                        [
                            new FundAmountState
                            {
                                FundName = _testFund.Name,
                                Amount = 2250.00m,
                            }
                        ],
                        PendingFundBalanceChanges = []
                    }
                ]);

        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        _accountingPeriodRepository.Add(secondAccountingPeriod);

        Transaction firstTransaction = _accountingPeriodService.AddTransaction(secondAccountingPeriod,
            new DateOnly(2024, 12, 10),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 250.00m,
                }
            ]);
        Transaction secondTransaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod,
            new DateOnly(2024, 12, 20),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 300.00m,
                }
            ]);
        firstTransaction.Post(_testAccount, new DateOnly(2024, 12, 11));
        secondTransaction.Post(_testAccount, new DateOnly(2024, 12, 21));

        ValidateBalances();
        _accountingPeriodService.ClosePeriod(_testAccountingPeriod);
        ValidateBalances();
        _accountingPeriodService.ClosePeriod(secondAccountingPeriod);
        ValidateBalances();
    }

    /// <summary>
    /// Tests that getting an Account Balance by Date works when a balance event from a future Accounting Period
    /// falls in the current Accounting Period before the provided date range
    /// </summary>
    [Fact]
    public void TestWithFuturePeriodBalanceEventFallingBeforeDateRange()
    {
        void ValidateBalances() => new AccountBalanceByDateValidator(
            _accountBalanceService.GetAccountBalancesByDate(
                _testAccount,
                new DateRange(new DateOnly(2024, 12, 20), new DateOnly(2024, 12, 21))))
            .Validate(
                [
                    new AccountBalanceByDateState
                    {
                        Date = new DateOnly(2024, 12, 20),
                        FundBalances =
                        [
                            new FundAmountState
                            {
                                FundName = _testFund.Name,
                                Amount = 2250.00m,
                            }
                        ],
                        PendingFundBalanceChanges =
                        [
                            new FundAmountState
                            {
                                FundName = _testFund.Name,
                                Amount = -300.00m,
                            }
                        ]
                    },
                    new AccountBalanceByDateState
                    {
                        Date = new DateOnly(2024, 12, 21),
                        FundBalances =
                        [
                            new FundAmountState
                            {
                                FundName = _testFund.Name,
                                Amount = 1950.00m,
                            }
                        ],
                        PendingFundBalanceChanges = []
                    }
                ]);

        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        _accountingPeriodRepository.Add(secondAccountingPeriod);

        Transaction firstTransaction = _accountingPeriodService.AddTransaction(secondAccountingPeriod,
            new DateOnly(2024, 12, 10),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 250.00m,
                }
            ]);
        Transaction secondTransaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod,
            new DateOnly(2024, 12, 20),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 300.00m,
                }
            ]);
        firstTransaction.Post(_testAccount, new DateOnly(2024, 12, 11));
        secondTransaction.Post(_testAccount, new DateOnly(2024, 12, 21));

        ValidateBalances();
        _accountingPeriodService.ClosePeriod(_testAccountingPeriod);
        ValidateBalances();
        _accountingPeriodService.ClosePeriod(secondAccountingPeriod);
        ValidateBalances();
    }

    /// <summary>
    /// Tests that getting an Account Balance by Date works when a balance event from a future Accounting Period
    /// falls in the current Accounting Period after the provided date range
    /// </summary>
    [Fact]
    public void TestWithFuturePeriodBalanceEventFallingAfterDateRange()
    {
        void ValidateBalances() => new AccountBalanceByDateValidator(
            _accountBalanceService.GetAccountBalancesByDate(
                _testAccount,
                new DateRange(new DateOnly(2024, 12, 10), new DateOnly(2024, 12, 11))))
            .Validate(
                [
                    new AccountBalanceByDateState
                    {
                        Date = new DateOnly(2024, 12, 10),
                        FundBalances =
                        [
                            new FundAmountState
                            {
                                FundName = _testFund.Name,
                                Amount = 2500.00m,
                            }
                        ],
                        PendingFundBalanceChanges =
                        [
                            new FundAmountState
                            {
                                FundName = _testFund.Name,
                                Amount = -250.00m,
                            }
                        ]
                    },
                    new AccountBalanceByDateState
                    {
                        Date = new DateOnly(2024, 12, 11),
                        FundBalances =
                        [
                            new FundAmountState
                            {
                                FundName = _testFund.Name,
                                Amount = 2250.00m,
                            }
                        ],
                        PendingFundBalanceChanges = []
                    }
                ]);

        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        _accountingPeriodRepository.Add(secondAccountingPeriod);

        Transaction firstTransaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod,
            new DateOnly(2024, 12, 10),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 250.00m,
                }
            ]);
        Transaction secondTransaction = _accountingPeriodService.AddTransaction(secondAccountingPeriod,
            new DateOnly(2024, 12, 20),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 300.00m,
                }
            ]);
        firstTransaction.Post(_testAccount, new DateOnly(2024, 12, 11));
        secondTransaction.Post(_testAccount, new DateOnly(2024, 12, 21));

        ValidateBalances();
        _accountingPeriodService.ClosePeriod(_testAccountingPeriod);
        ValidateBalances();
        _accountingPeriodService.ClosePeriod(secondAccountingPeriod);
        ValidateBalances();
    }
}