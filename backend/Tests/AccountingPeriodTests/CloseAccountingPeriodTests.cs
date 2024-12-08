using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Validators;

namespace Tests.AccountingPeriodTests;

/// <summary>
/// Test class that tests closing an Accounting Period
/// </summary>
public class CloseAccountingPeriodTests : UnitTestBase
{
    private readonly IAccountingPeriodRepository _accountingPeriodRepository;
    private readonly IAccountingPeriodService _accountingPeriodService;
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountService _accountService;
    private readonly IFundService _fundService;

    /// <summary>
    /// Constructs a new instance of this class.
    /// This constructor is run again before each individual test in this test class.
    /// </summary>
    public CloseAccountingPeriodTests()
    {
        _accountingPeriodRepository = GetService<IAccountingPeriodRepository>();
        _accountingPeriodService = GetService<IAccountingPeriodService>();
        _accountRepository = GetService<IAccountRepository>();
        _accountService = GetService<IAccountService>();
        _fundService = GetService<IFundService>();
    }

    /// <summary>
    /// Tests that an Account Period can be closed successfully
    /// </summary>
    [Fact]
    public void TestCloseAccountingPeriod()
    {
        AccountingPeriod accountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 11);
        _accountingPeriodService.ClosePeriod(accountingPeriod);
        new AccountingPeriodValidator(accountingPeriod).Validate(new AccountingPeriodState
        {
            Year = 2024,
            Month = 11,
            IsOpen = false,
            AccountBalanceCheckpoints = [],
            Transactions = [],
        });
    }

    /// <summary>
    /// Tests that closing an already closed Accounting Period will fail
    /// </summary>
    [Fact]
    public void TestWithAlreadyClosedAccountingPeriod()
    {
        AccountingPeriod accountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 11);
        _accountingPeriodService.ClosePeriod(accountingPeriod);
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.ClosePeriod(accountingPeriod));
    }

    /// <summary>
    /// Test that the earliest open Accounting Period can be closed
    /// </summary>
    [Fact]
    public void TestWithEarliestOpenPeriod()
    {
        // Add the first Accounting Period
        AccountingPeriod firstAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 11);
        _accountingPeriodRepository.Add(firstAccountingPeriod);

        // Add the second Accounting Period
        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        _accountingPeriodRepository.Add(secondAccountingPeriod);

        // Close the first Accounting Period
        _accountingPeriodService.ClosePeriod(firstAccountingPeriod);
        new AccountingPeriodValidator(firstAccountingPeriod).Validate(new AccountingPeriodState
        {
            Year = 2024,
            Month = 11,
            IsOpen = false,
            AccountBalanceCheckpoints = [],
            Transactions = [],
        });
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
    /// Tests that closing an Accounting Period while there's still an earlier open Accounting Period will fail
    /// </summary>
    [Fact]
    public void TestWithLatestOpenPeriod()
    {
        // Add the first Accounting Period
        AccountingPeriod firstAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 11);
        _accountingPeriodRepository.Add(firstAccountingPeriod);

        // Add the second Accounting Period
        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        _accountingPeriodRepository.Add(secondAccountingPeriod);

        // Attempt to close the second Accounting Period
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.ClosePeriod(secondAccountingPeriod));
    }

    /// <summary>
    /// Tests that an Accounting Period with fully posted Transactions can be closed successfully 
    /// </summary>
    [Fact]
    public void TestWithTransactions()
    {
        // Add a Fund
        Fund fund = _fundService.CreateNewFund("Test");

        // Add the Accounting Period
        AccountingPeriod accountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 11);
        _accountingPeriodRepository.Add(accountingPeriod);

        // Add two test Accounts
        Account firstAccount = _accountService.CreateNewAccount("Test", AccountType.Standard,
        [
            new FundAmount
            {
                Fund = fund,
                Amount = 50.00m,
            }
        ]);
        Account secondAccount = _accountService.CreateNewAccount("Test2", AccountType.Standard,
        [
            new FundAmount
            {
                Fund = fund,
                Amount = 50.00m,
            }
        ]);

        // Add a Transaction and fully post it
        Transaction transaction = _accountingPeriodService.AddTransaction(accountingPeriod, new DateOnly(2024, 11, 24), firstAccount, secondAccount,
            [
                new FundAmount
                {
                    Fund = fund,
                    Amount = 25.00m,
                }
            ]);
        transaction.Post(firstAccount, new DateOnly(2024, 11, 24));
        transaction.Post(secondAccount, new DateOnly(2024, 11, 24));

        // Close the Accounting Period
        _accountingPeriodService.ClosePeriod(accountingPeriod);
        new AccountingPeriodValidator(accountingPeriod).Validate(new AccountingPeriodState
        {
            Year = 2024,
            Month = 11,
            IsOpen = false,
            AccountBalanceCheckpoints =
            [
                new AccountBalanceCheckpointState
                {
                    AccountName = firstAccount.Name,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = fund.Name,
                            Amount = 50.00m,
                        }
                    ]
                },
                new AccountBalanceCheckpointState
                {
                    AccountName = secondAccount.Name,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = fund.Name,
                            Amount = 50.00m,
                        }
                    ]
                },
            ],
            Transactions =
            [
                new TransactionState
                {
                    TransactionDate = new DateOnly(2024, 11, 24),
                    AccountingEntries =
                    [
                        new FundAmountState
                        {
                            FundName = fund.Name,
                            Amount = 25.00m,
                        }
                    ],
                    TransactionBalanceEvents =
                    [
                        new TransactionBalanceEventState
                        {
                            AccountName = firstAccount.Name,
                            EventDate = new DateOnly(2024, 11, 24),
                            EventSequence = 1,
                            TransactionEventType = TransactionBalanceEventType.Added,
                            TransactionAccountType = TransactionAccountType.Debit,
                        },
                        new TransactionBalanceEventState
                        {
                            AccountName = secondAccount.Name,
                            EventDate = new DateOnly(2024, 11, 24),
                            EventSequence = 2,
                            TransactionEventType = TransactionBalanceEventType.Added,
                            TransactionAccountType = TransactionAccountType.Credit,
                        },
                        new TransactionBalanceEventState
                        {
                            AccountName = firstAccount.Name,
                            EventDate = new DateOnly(2024, 11, 24),
                            EventSequence = 3,
                            TransactionEventType = TransactionBalanceEventType.Posted,
                            TransactionAccountType = TransactionAccountType.Debit,
                        },
                        new TransactionBalanceEventState
                        {
                            AccountName = secondAccount.Name,
                            EventDate = new DateOnly(2024, 11, 24),
                            EventSequence = 4,
                            TransactionEventType = TransactionBalanceEventType.Posted,
                            TransactionAccountType = TransactionAccountType.Credit,
                        },
                    ]
                }
            ]
        });
    }

    /// <summary>
    /// Tests that closing an Accounting Period with pending balance changes will fail
    /// </summary>
    [Fact]
    public void TestWithPendingBalanceChanges()
    {
        // Add a Fund
        Fund fund = _fundService.CreateNewFund("Test");

        // Add the Accounting Period
        AccountingPeriod accountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 11);
        _accountingPeriodRepository.Add(accountingPeriod);

        // Add two test Accounts
        Account firstAccount = _accountService.CreateNewAccount("Test", AccountType.Standard,
        [
            new FundAmount
            {
                Fund = fund,
                Amount = 50.00m,
            }
        ]);
        _accountRepository.Add(firstAccount);
        Account secondAccount = _accountService.CreateNewAccount("Test2", AccountType.Standard,
        [
            new FundAmount
            {
                Fund = fund,
                Amount = 50.00m,
            }
        ]);
        _accountRepository.Add(secondAccount);

        // Add a Transaction and partially post it
        Transaction transaction = _accountingPeriodService.AddTransaction(accountingPeriod, new DateOnly(2024, 11, 24), firstAccount, secondAccount,
            [
                new FundAmount
                {
                    Fund = fund,
                    Amount = 25.00m,
                }
            ]);
        transaction.Post(firstAccount, new DateOnly(2024, 11, 24));

        // Close the Accounting Period
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.ClosePeriod(accountingPeriod));
    }

    /// <summary>
    /// Tests that closing an Accounting Period will add Balance Checkpoints to a future open Accounting Period
    /// </summary>
    [Fact]
    public void TestWithBalanceCheckpoints()
    {
        // Add a Fund
        Fund fund = _fundService.CreateNewFund("Test");

        // Add two Accounting Periods
        AccountingPeriod firstAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 11);
        _accountingPeriodRepository.Add(firstAccountingPeriod);
        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        _accountingPeriodRepository.Add(secondAccountingPeriod);

        // Add an Account
        Account account = _accountService.CreateNewAccount("Test", AccountType.Standard,
            [
                new FundAmount
                {
                    Fund = fund,
                    Amount = 50.00m,
                }
            ]);
        _accountRepository.Add(account);

        // Add a Transaction and fully post it
        Transaction transaction = _accountingPeriodService.AddTransaction(firstAccountingPeriod, new DateOnly(2024, 11, 24), account, null,
            [
                new FundAmount
                {
                    Fund = fund,
                    Amount = 25.00m,
                }
            ]);
        transaction.Post(account, new DateOnly(2024, 11, 24));

        // Close the first Accounting Period and expect that Balance Checkpoints are added to the second Accounting Period
        _accountingPeriodService.ClosePeriod(firstAccountingPeriod);
        new AccountingPeriodValidator(firstAccountingPeriod).Validate(new AccountingPeriodState
        {
            Year = 2024,
            Month = 11,
            IsOpen = false,
            AccountBalanceCheckpoints =
            [
                new AccountBalanceCheckpointState
                {
                    AccountName = account.Name,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = fund.Name,
                            Amount = 50.00m,
                        }
                    ]
                },
            ],
            Transactions =
            [
                new TransactionState
                {
                    TransactionDate = new DateOnly(2024, 11, 24),
                    AccountingEntries =
                    [
                        new FundAmountState
                        {
                            FundName = fund.Name,
                            Amount = 25.00m,
                        }
                    ],
                    TransactionBalanceEvents =
                    [
                        new TransactionBalanceEventState
                        {
                            AccountName = account.Name,
                            EventDate = new DateOnly(2024, 11, 24),
                            EventSequence = 1,
                            TransactionEventType = TransactionBalanceEventType.Added,
                            TransactionAccountType = TransactionAccountType.Debit,
                        },
                        new TransactionBalanceEventState
                        {
                            AccountName = account.Name,
                            EventDate = new DateOnly(2024, 11, 24),
                            EventSequence = 2,
                            TransactionEventType = TransactionBalanceEventType.Posted,
                            TransactionAccountType = TransactionAccountType.Debit,
                        },
                    ]
                }
            ]
        });
        new AccountingPeriodValidator(secondAccountingPeriod).Validate(new AccountingPeriodState
        {
            Year = 2024,
            Month = 12,
            IsOpen = true,
            AccountBalanceCheckpoints =
            [
                new AccountBalanceCheckpointState
                {
                    AccountName = account.Name,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = fund.Name,
                            Amount = 25.00m,
                        }
                    ]
                },
            ],
            Transactions = []
        });
    }

    /// <summary>
    /// Tests that closing an Accounting Period with a Transaction that falls in a future calendar month works
    /// as expected
    /// </summary>
    [Fact]
    public void TestWithTransactionsInFutureMonth()
    {
        // Add a Fund
        Fund fund = _fundService.CreateNewFund("Test");

        // Add two Accounting Periods
        AccountingPeriod firstAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 11);
        _accountingPeriodRepository.Add(firstAccountingPeriod);
        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        _accountingPeriodRepository.Add(secondAccountingPeriod);

        // Add an Account
        Account account = _accountService.CreateNewAccount("Test", AccountType.Standard,
            [
                new FundAmount
                {
                    Fund = fund,
                    Amount = 50.00m,
                }
            ]);
        _accountRepository.Add(account);

        // Add a Transaction and fully post it
        Transaction transaction = _accountingPeriodService.AddTransaction(firstAccountingPeriod, new DateOnly(2024, 11, 24), account, null,
            [
                new FundAmount
                {
                    Fund = fund,
                    Amount = 25.00m,
                }
            ]);
        transaction.Post(account, new DateOnly(2024, 12, 5));

        // Close the first Accounting Period and expect that Balance Checkpoints are added to the second Accounting Period
        _accountingPeriodService.ClosePeriod(firstAccountingPeriod);
        new AccountingPeriodValidator(firstAccountingPeriod).Validate(new AccountingPeriodState
        {
            Year = 2024,
            Month = 11,
            IsOpen = false,
            AccountBalanceCheckpoints =
            [
                new AccountBalanceCheckpointState
                {
                    AccountName = account.Name,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = fund.Name,
                            Amount = 50.00m,
                        }
                    ]
                },
            ],
            Transactions =
            [
                new TransactionState
                {
                    TransactionDate = new DateOnly(2024, 11, 24),
                    AccountingEntries =
                    [
                        new FundAmountState
                        {
                            FundName = fund.Name,
                            Amount = 25.00m,
                        }
                    ],
                    TransactionBalanceEvents =
                    [
                        new TransactionBalanceEventState
                        {
                            AccountName = account.Name,
                            EventDate = new DateOnly(2024, 11, 24),
                            EventSequence = 1,
                            TransactionEventType = TransactionBalanceEventType.Added,
                            TransactionAccountType = TransactionAccountType.Debit,
                        },
                        new TransactionBalanceEventState
                        {
                            AccountName = account.Name,
                            EventDate = new DateOnly(2024, 12, 5),
                            EventSequence = 1,
                            TransactionEventType = TransactionBalanceEventType.Posted,
                            TransactionAccountType = TransactionAccountType.Debit,
                        },
                    ]
                }
            ]
        });
        new AccountingPeriodValidator(secondAccountingPeriod).Validate(new AccountingPeriodState
        {
            Year = 2024,
            Month = 12,
            IsOpen = true,
            AccountBalanceCheckpoints =
            [
                new AccountBalanceCheckpointState
                {
                    AccountName = account.Name,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundName = fund.Name,
                            Amount = 25.00m,
                        }
                    ]
                },
            ],
            Transactions = []
        });
    }
}