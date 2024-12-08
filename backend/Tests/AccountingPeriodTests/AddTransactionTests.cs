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
    private readonly IAccountBalanceService _accountBalanceService;

    private readonly Fund _testFund;
    private readonly AccountingPeriod _testAccountingPeriod;
    private readonly Account _testAccount;


    /// <summary>
    /// Constructs a new instance of this class.
    /// This constructor is run again before each individual test in this test class.
    /// </summary>
    public AddTransactionTests()
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
    /// Tests that a Transaction can be added successfully
    /// </summary>
    [Fact]
    public void TestAddTransaction()
    {
        Transaction transaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod, new DateOnly(2024, 11, 26), _testAccount, null,
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
    public void TestWithClosedAccountingPeriod()
    {
        _accountingPeriodService.ClosePeriod(_testAccountingPeriod);
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.AddTransaction(_testAccountingPeriod, new DateOnly(2024, 11, 25),
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
    public void TestWithDateThatFallsInDifferentAccountingPeriod()
    {
        // Add an additional Accounting Period
        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        _accountingPeriodRepository.Add(secondAccountingPeriod);

        // Add a Transaction to the first Accounting Period that falls in the second period
        Transaction transaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod, new DateOnly(2024, 12, 25), _testAccount, null,
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
        transaction = _accountingPeriodService.AddTransaction(secondAccountingPeriod, new DateOnly(2024, 11, 25), _testAccount, null,
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

    /// <summary>
    /// Tests that adding a Transaction to an Accounting Period will fail when its date doesn't fall in an adjacent
    /// month
    /// </summary>
    [Fact]
    public void TestWithDateNotInAdjacentMonth()
    {
        // Test that adding a Transaction too far in the past fails
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.AddTransaction(_testAccountingPeriod, new DateOnly(2024, 9, 15),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 25.00m,
                }
            ]));

        // Test that adding a Transaction too far in the future fails
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.AddTransaction(_testAccountingPeriod, new DateOnly(2025, 1, 15),
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
    /// Tests that adding a Transaction with only a debit Account works as expected
    /// </summary>
    [Fact]
    public void TestDebitOnly()
    {
        Transaction transaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod, new DateOnly(2024, 12, 2),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 25.00m,
                }
            ]);
        new TransactionValidator(transaction).Validate(new TransactionState
        {
            TransactionDate = new DateOnly(2024, 12, 2),
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
                    EventDate = new DateOnly(2024, 12, 2),
                    EventSequence = 1,
                    TransactionEventType = TransactionBalanceEventType.Added,
                    TransactionAccountType = TransactionAccountType.Debit
                }
            ]
        });
    }

    /// <summary>
    /// Tests that adding a Transaction with only a credit Account works as expected
    /// </summary>
    [Fact]
    public void TestCreditOnly()
    {
        Transaction transaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod, new DateOnly(2024, 12, 2),
            null,
            _testAccount,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 25.00m
                }
            ]);
        new TransactionValidator(transaction).Validate(new TransactionState
        {
            TransactionDate = new DateOnly(2024, 12, 2),
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
                    EventDate = new DateOnly(2024, 12, 2),
                    EventSequence = 1,
                    TransactionEventType = TransactionBalanceEventType.Added,
                    TransactionAccountType = TransactionAccountType.Credit,
                }
            ]
        });
    }

    /// <summary>
    /// Tests that adding a Transaction with both a credit and debit Account works as expected
    /// </summary>
    [Fact]
    public void TestTransferTransaction()
    {
        // Add a second Account to be the credit account
        Account creditAccount = _accountService.CreateNewAccount("Credit", AccountType.Standard,
        [
            new FundAmount
            {
                Fund = _testFund,
                Amount = 1500.00m
            }
        ]);

        Transaction transaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod, new DateOnly(2024, 12, 2),
            _testAccount,
            creditAccount,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 25.00m,
                }
            ]);
        new TransactionValidator(transaction).Validate(new TransactionState
        {
            TransactionDate = new DateOnly(2024, 12, 2),
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
                    EventDate = new DateOnly(2024, 12, 2),
                    EventSequence = 1,
                    TransactionEventType = TransactionBalanceEventType.Added,
                    TransactionAccountType = TransactionAccountType.Debit
                },
                new TransactionBalanceEventState
                {
                    AccountName = creditAccount.Name,
                    EventDate = new DateOnly(2024, 12, 2),
                    EventSequence = 2,
                    TransactionEventType = TransactionBalanceEventType.Added,
                    TransactionAccountType = TransactionAccountType.Credit,
                }
            ]
        });
    }

    /// <summary>
    /// Tests that adding a Transaction with invalid Accounts will fail
    /// </summary>
    [Fact]
    public void TestWithInvalidAccounts()
    {
        // Test that having both the credit and debit account be null will fail
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.AddTransaction(_testAccountingPeriod, new DateOnly(2024, 12, 2),
            null,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 25.00m,
                }
            ]));

        // Test that having the same credit and debit account will fail
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.AddTransaction(_testAccountingPeriod, new DateOnly(2024, 12, 2),
            _testAccount,
            _testAccount,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 25.00m,
                }
            ]));
    }

    /// <summary>
    /// Tests that adding a Transaction with multiple accounting entries works as expected
    /// </summary>
    [Fact]
    public void TestWithMultipleAccountingEntries()
    {
        Fund secondFund = _fundService.CreateNewFund("Test2");
        Transaction transaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod, new DateOnly(2024, 12, 2),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 25.00m,
                },
                new FundAmount
                {
                    Fund = secondFund,
                    Amount = 50.00m,
                }
            ]);
        new TransactionValidator(transaction).Validate(new TransactionState
        {
            TransactionDate = new DateOnly(2024, 12, 2),
            AccountingEntries =
            [
                new FundAmountState
                {
                    FundName = _testFund.Name,
                    Amount = 25.00m,
                },
                new FundAmountState
                {
                    FundName = secondFund.Name,
                    Amount = 50.00m
                }
            ],
            TransactionBalanceEvents =
            [
                new TransactionBalanceEventState
                {
                    AccountName = _testAccount.Name,
                    EventDate = new DateOnly(2024, 12, 2),
                    EventSequence = 1,
                    TransactionEventType = TransactionBalanceEventType.Added,
                    TransactionAccountType = TransactionAccountType.Debit,
                }
            ]
        });
    }

    /// <summary>
    /// Tests that adding a Transaction with invalid accounting entries will fail
    /// </summary>
    [Fact]
    public void TestWithInvalidAccountingEntries()
    {
        // Test with no accounting entries
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.AddTransaction(_testAccountingPeriod, new DateOnly(2024, 12, 2),
            _testAccount,
            null,
            []));

        // Test with multiple accounting entries pointing to the same fund
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.AddTransaction(_testAccountingPeriod, new DateOnly(2024, 12, 2),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 25.00m
                },
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 50.00m
                }
            ]));

        // Test with an accounting entry with a value of zero
        Fund secondFund = _fundService.CreateNewFund("Test2");
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.AddTransaction(_testAccountingPeriod, new DateOnly(2024, 12, 2),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 25.00m
                },
                new FundAmount
                {
                    Fund = secondFund,
                    Amount = 0
                }
            ]));

        // Test with a negative accounting entry
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.AddTransaction(_testAccountingPeriod, new DateOnly(2024, 12, 2),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 25.00m
                },
                new FundAmount
                {
                    Fund = secondFund,
                    Amount = -50.00m,
                }
            ]));
    }

    /// <summary>
    /// Tests that adding a Transaction affects the Accounts' balances as expected
    /// </summary>
    [Fact]
    public void TestEffectOnAccountBalance()
    {
        Account creditAccount = _accountService.CreateNewAccount("Credit", AccountType.Standard,
        [
            new FundAmount
            {
                Fund = _testFund,
                Amount = 1500.00m,
            }
        ]);
        Transaction transaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod, new DateOnly(2024, 12, 2),
            _testAccount,
            creditAccount,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 50.00m,
                }
            ]);
        new AccountBalanceByEventValidator(_accountBalanceService.GetAccountBalancesByEvent(
                _testAccount,
                new DateRange(new DateOnly(2024, 12, 2), new DateOnly(2024, 12, 2))))
            .Validate(new AccountBalanceByEventState
            {
                AccountName = _testAccount.Name,
                AccountingPeriodYear = _testAccountingPeriod.Year,
                AccountingPeriodMonth = _testAccountingPeriod.Month,
                EventDate = new DateOnly(2024, 12, 2),
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
            });
        new AccountBalanceByEventValidator(_accountBalanceService.GetAccountBalancesByEvent(
                creditAccount,
                new DateRange(new DateOnly(2024, 12, 2), new DateOnly(2024, 12, 2))))
            .Validate(new AccountBalanceByEventState
            {
                AccountName = creditAccount.Name,
                AccountingPeriodYear = _testAccountingPeriod.Year,
                AccountingPeriodMonth = _testAccountingPeriod.Month,
                EventDate = new DateOnly(2024, 12, 2),
                EventSequence = 2,
                FundBalances =
                [
                    new FundAmountState
                    {
                        FundName = _testFund.Name,
                        Amount = 1500.00m,
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
            });
    }

    /// <summary>
    /// Tests that adding a Transaction affects a debt Account's balances as expected
    /// </summary>
    [Fact]
    public void TestEffectOnDebtAccountBalance()
    {
        Account firstDebtAccount = _accountService.CreateNewAccount("Debt1", AccountType.Debt,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 2500.00m,
                }
            ]);
        Account secondDebtAccount = _accountService.CreateNewAccount("Debt2", AccountType.Debt,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 1500.00m,
                }
            ]);
        Transaction transaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod, new DateOnly(2024, 12, 2),
            firstDebtAccount,
            secondDebtAccount,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 50.00m,
                }
            ]);
        new AccountBalanceByEventValidator(_accountBalanceService.GetAccountBalancesByEvent(
                firstDebtAccount,
                new DateRange(new DateOnly(2024, 12, 2), new DateOnly(2024, 12, 2))))
            .Validate(new AccountBalanceByEventState
            {
                AccountName = firstDebtAccount.Name,
                AccountingPeriodYear = _testAccountingPeriod.Year,
                AccountingPeriodMonth = _testAccountingPeriod.Month,
                EventDate = new DateOnly(2024, 12, 2),
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
            });
        new AccountBalanceByEventValidator(_accountBalanceService.GetAccountBalancesByEvent(
                secondDebtAccount,
                new DateRange(new DateOnly(2024, 12, 2), new DateOnly(2024, 12, 2))))
            .Validate(new AccountBalanceByEventState
            {
                AccountName = secondDebtAccount.Name,
                AccountingPeriodYear = _testAccountingPeriod.Year,
                AccountingPeriodMonth = _testAccountingPeriod.Month,
                EventDate = new DateOnly(2024, 12, 2),
                EventSequence = 2,
                FundBalances =
                [
                    new FundAmountState
                    {
                        FundName = _testFund.Name,
                        Amount = 1500.00m,
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
            });
    }

    /// <summary>
    /// Tests that adding a Transaction that would make one of the Fund balances in an Account negative
    /// works as expected
    /// </summary>
    [Fact]
    public void TestTransactionThatMakesAccountFundBalanceNegative()
    {
        Fund secondFund = _fundService.CreateNewFund("Test2");
        Transaction transaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod, new DateOnly(2024, 12, 2),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = secondFund,
                    Amount = 50.00m,
                }
            ]);
        new AccountBalanceByEventValidator(_accountBalanceService.GetAccountBalancesByEvent(
                _testAccount,
                new DateRange(new DateOnly(2024, 12, 2), new DateOnly(2024, 12, 2))))
            .Validate(new AccountBalanceByEventState
            {
                AccountName = _testAccount.Name,
                AccountingPeriodYear = _testAccountingPeriod.Year,
                AccountingPeriodMonth = _testAccountingPeriod.Month,
                EventDate = new DateOnly(2024, 12, 2),
                EventSequence = 1,
                FundBalances =
                [
                    new FundAmountState
                    {
                        FundName = _testFund.Name,
                        Amount = 2500.00m,
                    },
                ],
                PendingFundBalanceChanges =
                [
                    new FundAmountState
                    {
                        FundName = secondFund.Name,
                        Amount = -50.00m,
                    }
                ]
            });
    }

    /// <summary>
    /// Tests that adding a Transaction that would make the Accounts overall balance negative will fail
    /// </summary>
    [Fact]
    public void TestTransactionThatWouldMakeAccountBalanceNegative()
    {
        // Test for standard Account
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.AddTransaction(_testAccountingPeriod, new DateOnly(2024, 12, 2),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 3000.00m,
                }
            ]));

        // Test for debt Account
        Account debtAccount = _accountService.CreateNewAccount("Debt", AccountType.Debt,
        [
            new FundAmount
            {
                Fund = _testFund,
                Amount = 50.00m,
            }
        ]);
        _accountRepository.Add(debtAccount);
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.AddTransaction(_testAccountingPeriod, new DateOnly(2024, 12, 2),
            null,
            debtAccount,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 3000.00m,
                }
            ]));
    }

    /// <summary>
    /// Tests that adding a Transaction that would cause an existing Balance Event to make an Accounts overall balance
    /// negative will fail
    /// </summary>
    [Fact]
    public void TestTransactionThatInvalidateFutureBalanceEvents()
    {
        // Test for standard Accounts
        _accountingPeriodService.AddTransaction(_testAccountingPeriod,
            new DateOnly(2024, 11, 25),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 2450.00m
                }
            ]);
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.AddTransaction(_testAccountingPeriod,
            new DateOnly(2024, 11, 20),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 75.00m,
                }
            ]));

        // Test for debt Accounts
        Account debtAccount = _accountService.CreateNewAccount("Debt", AccountType.Debt,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 2500.00m,
                }
            ]);
        _accountingPeriodService.AddTransaction(_testAccountingPeriod,
            new DateOnly(2024, 11, 25),
            null,
            debtAccount,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 2450.00m
                }
            ]);
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.AddTransaction(_testAccountingPeriod,
            new DateOnly(2024, 11, 20),
            null,
            debtAccount,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 75.00m,
                }
            ]));
    }

    /// <summary>
    /// Tests that adding a Transaction that would cause an Account's balance to go negative within an Accounting
    /// Period will fail
    /// </summary>
    [Fact]
    public void TestTransactionThatMakesAccountNegativeWithinAccountingPeriod()
    {
        // Set up a second accounting period
        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);

        // Test for standard Accounts
        _accountingPeriodService.AddTransaction(_testAccountingPeriod,
            new DateOnly(2024, 11, 25),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 2450.00m,
                }
            ]);
        _accountingPeriodService.AddTransaction(secondAccountingPeriod,
            new DateOnly(2024, 11, 20),
            null,
            _testAccount,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 2450.00m
                }
            ]);
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.AddTransaction(_testAccountingPeriod,
            new DateOnly(2024, 11, 15),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 75.00m,
                }
            ]));

        // Test for debt Accounts
        Account debtAccount = _accountService.CreateNewAccount("Debt", AccountType.Debt,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 2500.00m,
                }
            ]);
        _accountingPeriodService.AddTransaction(_testAccountingPeriod,
            new DateOnly(2024, 11, 25),
            null,
            debtAccount,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 2450.00m,
                }
            ]);
        _accountingPeriodService.AddTransaction(secondAccountingPeriod,
            new DateOnly(2024, 11, 20),
            debtAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 2450.00m
                }
            ]);
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.AddTransaction(_testAccountingPeriod,
            new DateOnly(2024, 11, 15),
            null,
            debtAccount,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 75.00m,
                }
            ]));
    }
}