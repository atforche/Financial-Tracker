using Domain.Actions;
using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Validators;

namespace Tests.AccountBalanceTests;

/// <summary>
/// Test class that tests getting an Account's balance by event
/// </summary>
public class GetAccountBalanceByEventTests : UnitTestBase
{
    private readonly IAccountingPeriodRepository _accountingPeriodRepository;
    private readonly AddAccountingPeriodAction _addAccountingPeriodAction;
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
    /// This constructor is run again before each individual test in this test class.
    /// </summary>
    public GetAccountBalanceByEventTests()
    {
        _accountingPeriodRepository = GetService<IAccountingPeriodRepository>();
        _addAccountingPeriodAction = GetService<AddAccountingPeriodAction>();
        _accountingPeriodService = GetService<IAccountingPeriodService>();
        _accountRepository = GetService<IAccountRepository>();
        _accountService = GetService<IAccountService>();
        _fundRepository = GetService<IFundRepository>();
        _fundService = GetService<IFundService>();
        _accountBalanceService = GetService<IAccountBalanceService>();

        // Setup shared by all tests
        _testFund = _fundService.CreateNewFund("Test");
        _fundRepository.Add(_testFund);
        _testAccountingPeriod = _addAccountingPeriodAction.Run(2024, 11);
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
    /// Tests that getting an Account Balance by Event works as expected
    /// </summary>
    [Fact]
    public void TestAccountBalanceByEvent()
    {
        Transaction debitTransaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod,
            new DateOnly(2024, 11, 10),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 50.00m
                }
            ]);
        Transaction creditTransaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod,
            new DateOnly(2024, 11, 15),
            null,
            _testAccount,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 100.00m
                }
            ]);
        _accountingPeriodService.PostTransaction(creditTransaction, _testAccount, new DateOnly(2024, 11, 20));
        _accountingPeriodService.PostTransaction(debitTransaction, _testAccount, new DateOnly(2024, 11, 25));
        new AccountBalanceByEventValidator().Validate(
            _accountBalanceService.GetAccountBalancesByEvent(_testAccount, new DateRange(new DateOnly(2024, 11, 1), new DateOnly(2024, 11, 30))),
            [
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = _testAccountingPeriod.Year,
                    AccountingPeriodMonth = _testAccountingPeriod.Month,
                    EventDate = new DateOnly(2024, 11, 10),
                    EventSequence = 1,
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
                            Amount = -50.00m,
                        }
                    ]
                },
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = _testAccountingPeriod.Year,
                    AccountingPeriodMonth = _testAccountingPeriod.Month,
                    EventDate = new DateOnly(2024, 11, 15),
                    EventSequence = 1,
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
                            Amount = 50.00m,
                        }
                    ]
                },
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = _testAccountingPeriod.Year,
                    AccountingPeriodMonth = _testAccountingPeriod.Month,
                    EventDate = new DateOnly(2024, 11, 20),
                    EventSequence = 1,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = _testFund.Name,
                            Amount = 2600.00m,
                        }
                    ],
                    PendingFundBalanceChanges =
                    [
                        new FundAmountState
                        {
                            FundName = _testFund.Name,
                            Amount = -50.00m,
                        }
                    ]
                },
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = _testAccountingPeriod.Year,
                    AccountingPeriodMonth = _testAccountingPeriod.Month,
                    EventDate = new DateOnly(2024, 11, 25),
                    EventSequence = 1,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = _testFund.Name,
                            Amount = 2550.00m,
                        }
                    ],
                    PendingFundBalanceChanges = []
                }
            ]);
    }

    /// <summary>
    /// Tests that getting an Account Balance by Event works when the Account Balance has multiple Funds
    /// </summary>
    [Fact]
    public void TestWithMultipleFunds()
    {
        Fund secondFund = _fundService.CreateNewFund("Test2");

        Transaction debitTransaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod,
            new DateOnly(2024, 11, 10),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = secondFund,
                    Amount = 50.00m
                }
            ]);
        Transaction creditTransaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod,
            new DateOnly(2024, 11, 15),
            null,
            _testAccount,
            [
                new FundAmount
                {
                    Fund = secondFund,
                    Amount = 100.00m
                }
            ]);
        _accountingPeriodService.PostTransaction(creditTransaction, _testAccount, new DateOnly(2024, 11, 20));
        _accountingPeriodService.PostTransaction(debitTransaction, _testAccount, new DateOnly(2024, 11, 25));
        new AccountBalanceByEventValidator().Validate(
            _accountBalanceService.GetAccountBalancesByEvent(_testAccount, new DateRange(new DateOnly(2024, 11, 1), new DateOnly(2024, 11, 30))),
            [
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = _testAccountingPeriod.Year,
                    AccountingPeriodMonth = _testAccountingPeriod.Month,
                    EventDate = new DateOnly(2024, 11, 10),
                    EventSequence = 1,
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
                            FundName = secondFund.Name,
                            Amount = -50.00m,
                        },
                    ]
                },
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = _testAccountingPeriod.Year,
                    AccountingPeriodMonth = _testAccountingPeriod.Month,
                    EventDate = new DateOnly(2024, 11, 15),
                    EventSequence = 1,
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
                            FundName = secondFund.Name,
                            Amount = 50.00m,
                        }
                    ]
                },
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = _testAccountingPeriod.Year,
                    AccountingPeriodMonth = _testAccountingPeriod.Month,
                    EventDate = new DateOnly(2024, 11, 20),
                    EventSequence = 1,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = _testFund.Name,
                            Amount = 2500.00m,
                        },
                        new FundAmountState
                        {
                            FundName = secondFund.Name,
                            Amount = 100.00m,
                        }
                    ],
                    PendingFundBalanceChanges =
                    [
                        new FundAmountState
                        {
                            FundName = secondFund.Name,
                            Amount = -50.00m,
                        }
                    ]
                },
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = _testAccountingPeriod.Year,
                    AccountingPeriodMonth = _testAccountingPeriod.Month,
                    EventDate = new DateOnly(2024, 11, 25),
                    EventSequence = 1,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = _testFund.Name,
                            Amount = 2500.00m,
                        },
                        new FundAmountState
                        {
                            FundName = secondFund.Name,
                            Amount = 50.00m,
                        }
                    ],
                    PendingFundBalanceChanges = []
                }
            ]);
    }

    /// <summary>
    /// Tests that getting an Account Balance by Event with an invalid Date Range will fail
    /// </summary>
    [Fact]
    public void TestWithInvalidDateRanges()
    {
        // Test with a date range that falls prior to any accounting periods
        Assert.Throws<InvalidOperationException>(() => _accountBalanceService.GetAccountBalancesByEvent(
            _testAccount,
            new DateRange(new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31))));

        // Test with a date range that falls prior to this account being added
        _accountingPeriodService.ClosePeriod(_testAccountingPeriod);
        AccountingPeriod secondAccountingPeriod = _addAccountingPeriodAction.Run(2024, 12);
        _accountingPeriodRepository.Add(secondAccountingPeriod);
        Account secondAccount = _accountService.CreateNewAccount("Test2", AccountType.Standard,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 2500.00m,
                }
            ]);
        Assert.Throws<InvalidOperationException>(() => _accountBalanceService.GetAccountBalancesByEvent(
            secondAccount,
            new DateRange(new DateOnly(2024, 11, 1), new DateOnly(2024, 11, 30))));

        // Test with a date range that falls after any accounting periods
        Assert.Throws<InvalidOperationException>(() => _accountBalanceService.GetAccountBalancesByEvent(
            _testAccount,
            new DateRange(new DateOnly(2025, 11, 1), new DateOnly(2025, 11, 30))));
    }

    /// <summary>
    /// Tests that getting an Account Balance by Event works when a certain date has balance
    /// events from multiple Accounting Periods
    /// </summary>
    [Fact]
    public void TestWithEventsFromMultiplePeriodsOnSameDate()
    {
        void ValidateBalances() => new AccountBalanceByEventValidator().Validate(
            _accountBalanceService.GetAccountBalancesByEvent(_testAccount, new DateRange(new DateOnly(2024, 12, 1), new DateOnly(2024, 12, 31))),
            [
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = 2024,
                    AccountingPeriodMonth = 11,
                    EventDate = new DateOnly(2024, 12, 15),
                    EventSequence = 1,
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
                            FundName = _testFund.Name,
                            Amount = -250.00m
                        }
                    ]
                },
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = 2024,
                    AccountingPeriodMonth = 12,
                    EventDate = new DateOnly(2024, 12, 15),
                    EventSequence = 1,
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
                            FundName = _testFund.Name,
                            Amount = -550.00m
                        }
                    ]
                },
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = 2025,
                    AccountingPeriodMonth = 1,
                    EventDate = new DateOnly(2024, 12, 15),
                    EventSequence = 1,
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
                            FundName = _testFund.Name,
                            Amount = -900.00m
                        }
                    ]
                },
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = 2024,
                    AccountingPeriodMonth = 11,
                    EventDate = new DateOnly(2024, 12, 20),
                    EventSequence = 1,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = _testFund.Name,
                            Amount = 2250.00m
                        }
                    ],
                    PendingFundBalanceChanges =
                    [
                        new FundAmountState
                        {
                            FundName = _testFund.Name,
                            Amount = -650.00m
                        }
                    ]
                },
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = 2024,
                    AccountingPeriodMonth = 12,
                    EventDate = new DateOnly(2024, 12, 20),
                    EventSequence = 1,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = _testFund.Name,
                            Amount = 1950.00m
                        }
                    ],
                    PendingFundBalanceChanges =
                    [
                        new FundAmountState
                        {
                            FundName = _testFund.Name,
                            Amount = -350.00m
                        }
                    ]
                },
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = 2025,
                    AccountingPeriodMonth = 1,
                    EventDate = new DateOnly(2024, 12, 20),
                    EventSequence = 1,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = _testFund.Name,
                            Amount = 1600.00m
                        }
                    ],
                    PendingFundBalanceChanges = []
                }
            ]);

        // Set up two additional accounting periods
        AccountingPeriod secondAccountingPeriod = _addAccountingPeriodAction.Run(2024, 12);
        _accountingPeriodRepository.Add(secondAccountingPeriod);
        AccountingPeriod thirdAccountingPeriod = _addAccountingPeriodAction.Run(2025, 1);
        _accountingPeriodRepository.Add(thirdAccountingPeriod);

        // Set up similar transactions in each accounting period
        Transaction firstTransaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod,
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
        Transaction secondTransaction = _accountingPeriodService.AddTransaction(secondAccountingPeriod,
            new DateOnly(2024, 12, 15),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 300.00m,
                }
            ]);
        Transaction thirdTransaction = _accountingPeriodService.AddTransaction(thirdAccountingPeriod,
            new DateOnly(2024, 12, 15),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 350.00m,
                }
            ]);
        _accountingPeriodService.PostTransaction(firstTransaction, _testAccount, new DateOnly(2024, 12, 20));
        _accountingPeriodService.PostTransaction(secondTransaction, _testAccount, new DateOnly(2024, 12, 20));
        _accountingPeriodService.PostTransaction(thirdTransaction, _testAccount, new DateOnly(2024, 12, 20));

        ValidateBalances();
        _accountingPeriodService.ClosePeriod(_testAccountingPeriod);
        ValidateBalances();
        _accountingPeriodService.ClosePeriod(secondAccountingPeriod);
        ValidateBalances();
        _accountingPeriodService.ClosePeriod(thirdAccountingPeriod);
        ValidateBalances();
    }

    /// <summary>
    /// Test that getting an Account Balance by Event works across an Accounting Period boundary
    /// </summary>
    [Fact]
    public void TestAcrossAccountingPeriodBoundary()
    {
        void ValidateBalances() => new AccountBalanceByEventValidator().Validate(
            _accountBalanceService.GetAccountBalancesByEvent(_testAccount, new DateRange(new DateOnly(2024, 11, 15), new DateOnly(2024, 12, 15))),
            [
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = 2024,
                    AccountingPeriodMonth = 11,
                    EventDate = new DateOnly(2024, 11, 30),
                    EventSequence = 1,
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
                            Amount = -50.00m,
                        }
                    ]
                },
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = 2024,
                    AccountingPeriodMonth = 11,
                    EventDate = new DateOnly(2024, 11, 30),
                    EventSequence = 2,
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
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = 2024,
                    AccountingPeriodMonth = 12,
                    EventDate = new DateOnly(2024, 12, 1),
                    EventSequence = 1,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = _testFund.Name,
                            Amount = 2450.00m,
                        }
                    ],
                    PendingFundBalanceChanges =
                    [
                        new FundAmountState
                        {
                            FundName = _testFund.Name,
                            Amount = -100.00m,
                        }
                    ]
                },
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = 2024,
                    AccountingPeriodMonth = 12,
                    EventDate = new DateOnly(2024, 12, 1),
                    EventSequence = 2,
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
        AccountingPeriod secondAccountingPeriod = _addAccountingPeriodAction.Run(2024, 12);
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
        _accountingPeriodService.PostTransaction(firstTransaction, _testAccount, new DateOnly(2024, 11, 30));
        _accountingPeriodService.PostTransaction(secondTransaction, _testAccount, new DateOnly(2024, 12, 1));

        ValidateBalances();
        _accountingPeriodService.ClosePeriod(_testAccountingPeriod);
        ValidateBalances();
        _accountingPeriodService.ClosePeriod(secondAccountingPeriod);
        ValidateBalances();
    }

    /// <summary>
    /// Tests that getting an Account Balance by Event works across an Accounting Period boundary where 
    /// transactions in the first Accounting Period fall after transactions in the second
    /// </summary>
    [Fact]
    public void TestWithMixedAccountingPeriodBoundary()
    {
        void ValidateBalances() => new AccountBalanceByEventValidator().Validate(
            _accountBalanceService.GetAccountBalancesByEvent(_testAccount, new DateRange(new DateOnly(2024, 11, 15), new DateOnly(2024, 12, 15))),
            [
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = 2024,
                    AccountingPeriodMonth = 12,
                    EventDate = new DateOnly(2024, 11, 30),
                    EventSequence = 1,
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
                            Amount = -50.00m,
                        }
                    ]
                },
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = 2024,
                    AccountingPeriodMonth = 12,
                    EventDate = new DateOnly(2024, 11, 30),
                    EventSequence = 2,
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
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = 2024,
                    AccountingPeriodMonth = 11,
                    EventDate = new DateOnly(2024, 12, 1),
                    EventSequence = 1,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = _testFund.Name,
                            Amount = 2450.00m,
                        }
                    ],
                    PendingFundBalanceChanges =
                    [
                        new FundAmountState
                        {
                            FundName = _testFund.Name,
                            Amount = -100.00m,
                        }
                    ]
                },
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = 2024,
                    AccountingPeriodMonth = 11,
                    EventDate = new DateOnly(2024, 12, 1),
                    EventSequence = 2,
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
        AccountingPeriod secondAccountingPeriod = _addAccountingPeriodAction.Run(2024, 12);
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

        _accountingPeriodService.PostTransaction(firstTransaction, _testAccount, new DateOnly(2024, 12, 1));
        _accountingPeriodService.PostTransaction(secondTransaction, _testAccount, new DateOnly(2024, 11, 30));

        ValidateBalances();
        _accountingPeriodService.ClosePeriod(_testAccountingPeriod);
        ValidateBalances();
        _accountingPeriodService.ClosePeriod(secondAccountingPeriod);
        ValidateBalances();
    }

    /// <summary>
    /// Tests that getting an Account Balance by Event works when a balance event from a past Accounting Period
    /// falls in the current Accounting Period before the provided date range
    /// </summary>
    [Fact]
    public void TestWithPastPeriodBalanceEventFallingBeforeDateRange()
    {
        void ValidateBalances() => new AccountBalanceByEventValidator().Validate(
            _accountBalanceService.GetAccountBalancesByEvent(_testAccount, new DateRange(new DateOnly(2024, 12, 20), new DateOnly(2024, 12, 31))),
            [
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = 2024,
                    AccountingPeriodMonth = 12,
                    EventDate = new DateOnly(2024, 12, 20),
                    EventSequence = 1,
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
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = 2024,
                    AccountingPeriodMonth = 12,
                    EventDate = new DateOnly(2024, 12, 25),
                    EventSequence = 1,
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

        AccountingPeriod secondAccountingPeriod = _addAccountingPeriodAction.Run(2024, 12);
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
        _accountingPeriodService.PostTransaction(firstTransaction, _testAccount, new DateOnly(2024, 12, 15));
        _accountingPeriodService.PostTransaction(secondTransaction, _testAccount, new DateOnly(2024, 12, 25));

        ValidateBalances();
        _accountingPeriodService.ClosePeriod(_testAccountingPeriod);
        ValidateBalances();
        _accountingPeriodService.ClosePeriod(secondAccountingPeriod);
        ValidateBalances();
    }

    /// <summary>
    /// Tests that getting an Account Balance by Event works when a balance event from a past Accounting Period
    /// falls in the current Accounting Period after the provided date range
    /// </summary>
    [Fact]
    public void TestWithPastPeriodBalanceEventFallingAfterDateRange()
    {
        void ValidateBalances() => new AccountBalanceByEventValidator().Validate(
            _accountBalanceService.GetAccountBalancesByEvent(_testAccount, new DateRange(new DateOnly(2024, 12, 10), new DateOnly(2024, 12, 19))),
            [
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = 2024,
                    AccountingPeriodMonth = 12,
                    EventDate = new DateOnly(2024, 12, 10),
                    EventSequence = 1,
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
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = 2024,
                    AccountingPeriodMonth = 12,
                    EventDate = new DateOnly(2024, 12, 15),
                    EventSequence = 1,
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

        AccountingPeriod secondAccountingPeriod = _addAccountingPeriodAction.Run(2024, 12);
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
        _accountingPeriodService.PostTransaction(firstTransaction, _testAccount, new DateOnly(2024, 12, 15));
        _accountingPeriodService.PostTransaction(secondTransaction, _testAccount, new DateOnly(2024, 12, 25));

        ValidateBalances();
        _accountingPeriodService.ClosePeriod(_testAccountingPeriod);
        ValidateBalances();
        _accountingPeriodService.ClosePeriod(secondAccountingPeriod);
        ValidateBalances();
    }

    /// <summary>
    /// Tests that getting an Account Balance by Event works when a balance event from a future Accounting Period
    /// falls in the current Accounting Period before the provided date range
    /// </summary>
    [Fact]
    public void TestWithFuturePeriodBalanceEventFallingBeforeDateRange()
    {
        void ValidateBalances() => new AccountBalanceByEventValidator().Validate(
            _accountBalanceService.GetAccountBalancesByEvent(_testAccount, new DateRange(new DateOnly(2024, 12, 20), new DateOnly(2024, 12, 31))),
            [
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = 2024,
                    AccountingPeriodMonth = 11,
                    EventDate = new DateOnly(2024, 12, 20),
                    EventSequence = 1,
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
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = 2024,
                    AccountingPeriodMonth = 11,
                    EventDate = new DateOnly(2024, 12, 25),
                    EventSequence = 1,
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

        AccountingPeriod secondAccountingPeriod = _addAccountingPeriodAction.Run(2024, 12);
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
        _accountingPeriodService.PostTransaction(firstTransaction, _testAccount, new DateOnly(2024, 12, 15));
        _accountingPeriodService.PostTransaction(secondTransaction, _testAccount, new DateOnly(2024, 12, 25));

        ValidateBalances();
        _accountingPeriodService.ClosePeriod(_testAccountingPeriod);
        ValidateBalances();
        _accountingPeriodService.ClosePeriod(secondAccountingPeriod);
        ValidateBalances();
    }

    /// <summary>
    /// Tests that getting an Account Balance by Event works when a balance event from a future Accounting Period
    /// falls in the current Accounting Period after the provided date range
    /// </summary>
    [Fact]
    public void TestWithFuturePeriodBalanceEventFallingAfterDateRange()
    {
        void ValidateBalances() => new AccountBalanceByEventValidator().Validate(
            _accountBalanceService.GetAccountBalancesByEvent(_testAccount, new DateRange(new DateOnly(2024, 12, 10), new DateOnly(2024, 12, 19))),
            [
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = 2024,
                    AccountingPeriodMonth = 11,
                    EventDate = new DateOnly(2024, 12, 10),
                    EventSequence = 1,
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
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = 2024,
                    AccountingPeriodMonth = 11,
                    EventDate = new DateOnly(2024, 12, 15),
                    EventSequence = 1,
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

        AccountingPeriod secondAccountingPeriod = _addAccountingPeriodAction.Run(2024, 12);
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
        _accountingPeriodService.PostTransaction(firstTransaction, _testAccount, new DateOnly(2024, 12, 15));
        _accountingPeriodService.PostTransaction(secondTransaction, _testAccount, new DateOnly(2024, 12, 25));

        ValidateBalances();
        _accountingPeriodService.ClosePeriod(_testAccountingPeriod);
        ValidateBalances();
        _accountingPeriodService.ClosePeriod(secondAccountingPeriod);
        ValidateBalances();
    }
}