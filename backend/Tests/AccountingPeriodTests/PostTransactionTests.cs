using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Validators;

namespace Tests.AccountingPeriodTests;

/// <summary>
/// Test class that tests posting a Transaction
/// </summary>
public class PostTransactionTests : UnitTestBase
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
    public PostTransactionTests()
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
    /// Tests that a Transaction can be posted successfully
    /// </summary>
    [Fact]
    public void TestPostTransaction()
    {
        Transaction transaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod,
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
        _accountingPeriodService.PostTransaction(transaction, _testAccount, new DateOnly(2024, 11, 15));
        new TransactionValidator().Validate(transaction,
            new TransactionState
            {
                TransactionDate = new DateOnly(2024, 11, 15),
                AccountingEntries =
                [
                    new FundAmountState
                    {
                        FundName = _testFund.Name,
                        Amount = 250.00m,
                    }
                ],
                TransactionBalanceEvents =
                [
                    new TransactionBalanceEventState
                    {
                        AccountName = _testAccount.Name,
                        EventDate = new DateOnly(2024, 11, 15),
                        EventSequence = 1,
                        TransactionEventType = TransactionBalanceEventType.Added,
                        TransactionAccountType = TransactionAccountType.Debit,
                    },
                    new TransactionBalanceEventState
                    {
                        AccountName = _testAccount.Name,
                        EventDate = new DateOnly(2024, 11, 15),
                        EventSequence = 2,
                        TransactionEventType = TransactionBalanceEventType.Posted,
                        TransactionAccountType = TransactionAccountType.Debit,
                    },
                ]
            });
    }

    /// <summary>
    /// Tests that posting a Transaction that is already posted will fail
    /// </summary>
    [Fact]
    public void TestAlreadyPostedTransaction()
    {
        Transaction transaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod,
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
        _accountingPeriodService.PostTransaction(transaction, _testAccount, new DateOnly(2024, 11, 15));
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.PostTransaction(transaction,
             _testAccount,
             new DateOnly(2024, 11, 15)));
    }

    /// <summary>
    /// Tests that posting a Transaction with an invalid Account will fail
    /// </summary>
    [Fact]
    public void TestWithInvalidAccount()
    {
        Account creditAccount = _accountService.CreateNewAccount("Credit",
            AccountType.Standard,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 2500.00m,
                }
            ]);
        Account extraAccount = _accountService.CreateNewAccount("Extra",
            AccountType.Standard,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 2500.00m,
                }
            ]);

        Transaction transaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod,
            new DateOnly(2024, 11, 15),
            _testAccount,
            creditAccount,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 250.00m,
                }
            ]);
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.PostTransaction(transaction,
            extraAccount,
            new DateOnly(2024, 11, 15)));
    }

    /// <summary>
    /// Tests that a Transaction can be posted with a date in a different Accounting Period
    /// </summary>
    [Fact]
    public void TestWithDateThatFallsInDifferentAccountingPeriod()
    {
        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        _accountingPeriodRepository.Add(secondAccountingPeriod);
        AccountingPeriod thirdAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2025, 1);
        _accountingPeriodRepository.Add(thirdAccountingPeriod);

        // Test a posting date in a past accounting period
        Transaction pastTransaction = _accountingPeriodService.AddTransaction(secondAccountingPeriod,
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
        _accountingPeriodService.PostTransaction(pastTransaction,
            _testAccount,
            new DateOnly(2024, 11, 15));
        new TransactionValidator().Validate(pastTransaction,
            new TransactionState
            {
                TransactionDate = new DateOnly(2024, 11, 15),
                AccountingEntries =
                [
                    new FundAmountState
                    {
                        FundName = _testFund.Name,
                        Amount = 250.00m,
                    }
                ],
                TransactionBalanceEvents =
                [
                    new TransactionBalanceEventState
                    {
                        AccountName = _testAccount.Name,
                        EventDate = new DateOnly(2024, 11, 15),
                        EventSequence = 1,
                        TransactionEventType = TransactionBalanceEventType.Added,
                        TransactionAccountType = TransactionAccountType.Debit,
                    },
                    new TransactionBalanceEventState
                    {
                        AccountName = _testAccount.Name,
                        EventDate = new DateOnly(2024, 11, 15),
                        EventSequence = 2,
                        TransactionEventType = TransactionBalanceEventType.Posted,
                        TransactionAccountType = TransactionAccountType.Debit,
                    },
                ]
            });

        // Test a posting date in a future accounting period
        Transaction futureTransaction = _accountingPeriodService.AddTransaction(secondAccountingPeriod,
            new DateOnly(2025, 1, 15),
            _testAccount,
            null,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 250.00m,
                }
            ]);
        _accountingPeriodService.PostTransaction(futureTransaction,
            _testAccount,
            new DateOnly(2025, 1, 15));
        new TransactionValidator().Validate(futureTransaction,
            new TransactionState
            {
                TransactionDate = new DateOnly(2025, 1, 15),
                AccountingEntries =
                [
                    new FundAmountState
                    {
                        FundName = _testFund.Name,
                        Amount = 250.00m,
                    }
                ],
                TransactionBalanceEvents =
                [
                    new TransactionBalanceEventState
                    {
                        AccountName = _testAccount.Name,
                        EventDate = new DateOnly(2025, 1, 15),
                        EventSequence = 1,
                        TransactionEventType = TransactionBalanceEventType.Added,
                        TransactionAccountType = TransactionAccountType.Debit,
                    },
                    new TransactionBalanceEventState
                    {
                        AccountName = _testAccount.Name,
                        EventDate = new DateOnly(2025, 1, 15),
                        EventSequence = 2,
                        TransactionEventType = TransactionBalanceEventType.Posted,
                        TransactionAccountType = TransactionAccountType.Debit,
                    },
                ]
            });
    }

    /// <summary>
    /// Tests that posting a Transaction with an invalid date will fail
    /// </summary>
    [Fact]
    public void TestWithInvalidDate()
    {
        // Tests that the posting date cannot fall before the add date
        Transaction transaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod,
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
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.PostTransaction(transaction,
            _testAccount,
            new DateOnly(2024, 11, 14)));

        // Tests that the posting date cannot fall in a non-adjacent accounting period
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.PostTransaction(transaction,
            _testAccount,
            new DateOnly(2025, 1, 15)));
    }

    /// <summary>
    /// Tests that posting a Transaction affects the Accounts' balances as expected
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
        Transaction transaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod, new DateOnly(2024, 11, 15),
            _testAccount,
            creditAccount,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 50.00m,
                }
            ]);
        _accountingPeriodService.PostTransaction(transaction, _testAccount, new DateOnly(2024, 11, 16));
        _accountingPeriodService.PostTransaction(transaction, creditAccount, new DateOnly(2024, 11, 17));
        new AccountBalanceByEventValidator().Validate(
            _accountBalanceService.GetAccountBalancesByEvent(_testAccount, new DateRange(new DateOnly(2024, 11, 1), new DateOnly(2024, 11, 30))),
            [
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
                            Amount = -50.00m,
                        }
                    ]
                },
                new AccountBalanceByEventState
                {
                    AccountName = _testAccount.Name,
                    AccountingPeriodYear = _testAccountingPeriod.Year,
                    AccountingPeriodMonth = _testAccountingPeriod.Month,
                    EventDate = new DateOnly(2024, 11, 16),
                    EventSequence = 1,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = _testFund.Name,
                            Amount = 2450.00m,
                        }
                    ],
                    PendingFundBalanceChanges = []
                }
            ]);
        new AccountBalanceByEventValidator().Validate(
            _accountBalanceService.GetAccountBalancesByEvent(creditAccount, new DateRange(new DateOnly(2024, 11, 1), new DateOnly(2024, 11, 30))),
            [
                new AccountBalanceByEventState
                {
                    AccountName = creditAccount.Name,
                    AccountingPeriodYear = _testAccountingPeriod.Year,
                    AccountingPeriodMonth = _testAccountingPeriod.Month,
                    EventDate = new DateOnly(2024, 11, 15),
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
                },
                new AccountBalanceByEventState
                {
                    AccountName = creditAccount.Name,
                    AccountingPeriodYear = _testAccountingPeriod.Year,
                    AccountingPeriodMonth = _testAccountingPeriod.Month,
                    EventDate = new DateOnly(2024, 11, 17),
                    EventSequence = 1,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = _testFund.Name,
                            Amount = 1550.00m,
                        }
                    ],
                    PendingFundBalanceChanges = []
                }
            ]);
    }

    /// <summary>
    /// Tests that posting a Transaction affects a debt Account's balances as expected
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
        Transaction transaction = _accountingPeriodService.AddTransaction(_testAccountingPeriod, new DateOnly(2024, 11, 15),
            firstDebtAccount,
            secondDebtAccount,
            [
                new FundAmount
                {
                    Fund = _testFund,
                    Amount = 50.00m,
                }
            ]);
        _accountingPeriodService.PostTransaction(transaction, firstDebtAccount, new DateOnly(2024, 11, 16));
        _accountingPeriodService.PostTransaction(transaction, secondDebtAccount, new DateOnly(2024, 11, 17));
        new AccountBalanceByEventValidator().Validate(
            _accountBalanceService.GetAccountBalancesByEvent(firstDebtAccount, new DateRange(new DateOnly(2024, 11, 1), new DateOnly(2024, 11, 30))),
            [
                new AccountBalanceByEventState
                {
                    AccountName = firstDebtAccount.Name,
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
                    AccountName = firstDebtAccount.Name,
                    AccountingPeriodYear = _testAccountingPeriod.Year,
                    AccountingPeriodMonth = _testAccountingPeriod.Month,
                    EventDate = new DateOnly(2024, 11, 16),
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
        new AccountBalanceByEventValidator().Validate(
            _accountBalanceService.GetAccountBalancesByEvent(secondDebtAccount, new DateRange(new DateOnly(2024, 11, 1), new DateOnly(2024, 11, 30))),
            [
                new AccountBalanceByEventState
                {
                    AccountName = secondDebtAccount.Name,
                    AccountingPeriodYear = _testAccountingPeriod.Year,
                    AccountingPeriodMonth = _testAccountingPeriod.Month,
                    EventDate = new DateOnly(2024, 11, 15),
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
                },
                new AccountBalanceByEventState
                {
                    AccountName = secondDebtAccount.Name,
                    AccountingPeriodYear = _testAccountingPeriod.Year,
                    AccountingPeriodMonth = _testAccountingPeriod.Month,
                    EventDate = new DateOnly(2024, 11, 17),
                    EventSequence = 1,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = _testFund.Name,
                            Amount = 1450.00m,
                        }
                    ],
                    PendingFundBalanceChanges = []
                }
            ]);
    }
}