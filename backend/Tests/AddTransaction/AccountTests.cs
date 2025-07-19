using Domain;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Tests.Builders;
using Tests.Validators;

namespace Tests.AddTransaction;

/// <summary>
/// Test class that tests adding a Transaction with different Accounts
/// </summary>
public class AccountTests : TestClass
{
    /// <summary>
    /// Runs the test for adding a Transaction that debits a standard Account
    /// </summary>
    [Fact]
    public void RunStandardDebitTest()
    {
        AccountingPeriod accountingPeriod = GetService<AccountingPeriodBuilder>().Build();
        Fund fund = GetService<FundBuilder>().Build();
        Account account = GetService<AccountBuilder>().Build();

        Transaction transaction = GetService<TransactionBuilder>()
            .WithDebitAccount(account.Id)
            .WithDebitFundAmounts(
                [
                    new FundAmount
                    {
                        FundId = fund.Id,
                        Amount = 500.00m
                    }
                ])
            .Build();
        new TransactionValidator().Validate(transaction,
            new TransactionState
            {
                AccountingPeriodId = accountingPeriod.Id,
                Date = new DateOnly(2025, 1, 15),
                DebitAccountId = account.Id,
                DebitFundAmounts =
                [
                    new FundAmountState
                    {
                        FundId = fund.Id,
                        Amount = 500.00m,
                    }
                ],
                TransactionBalanceEvents =
                [
                    new TransactionBalanceEventState
                    {
                        AccountingPeriodId = accountingPeriod.Id,
                        EventDate = new DateOnly(2025, 1, 15),
                        EventSequence = 1,
                        Parts =
                        [
                            TransactionBalanceEventPartType.AddedDebit
                        ]
                    }
                ]
            });
        new AccountBalanceByEventValidator().Validate(
            GetService<AccountBalanceService>().GetAccountBalancesByEvent(account.Id,
                new DateRange(new DateOnly(2025, 1, 15), new DateOnly(2025, 1, 15))),
            [
                new AccountBalanceByEventState
                {
                    AccountingPeriodId = accountingPeriod.Id,
                    EventDate = new DateOnly(2025, 1, 15),
                    EventSequence = 1,
                    AccountId = account.Id,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundId = fund.Id,
                            Amount = 2500.00m
                        }
                    ],
                    PendingFundBalanceChanges =
                    [
                        new FundAmountState
                        {
                            FundId = fund.Id,
                            Amount = -500.00m
                        }
                    ]
                }
            ]);
    }

    /// <summary>
    /// Runs the test for adding a Transaction that debits a debt Account
    /// </summary>
    [Fact]
    public void RunDebtDebitTest()
    {
        AccountingPeriod accountingPeriod = GetService<AccountingPeriodBuilder>().Build();
        Fund fund = GetService<FundBuilder>().Build();
        Account account = GetService<AccountBuilder>()
            .WithType(AccountType.Debt)
            .Build();

        Transaction transaction = GetService<TransactionBuilder>()
            .WithDebitAccount(account.Id)
            .WithDebitFundAmounts(
                [
                    new FundAmount
                    {
                        FundId = fund.Id,
                        Amount = 500.00m
                    }
                ])
            .Build();
        new TransactionValidator().Validate(transaction,
            new TransactionState
            {
                AccountingPeriodId = accountingPeriod.Id,
                Date = new DateOnly(2025, 1, 15),
                DebitAccountId = account.Id,
                DebitFundAmounts =
                [
                    new FundAmountState
                    {
                        FundId = fund.Id,
                        Amount = 500.00m,
                    }
                ],
                TransactionBalanceEvents =
                [
                    new TransactionBalanceEventState
                    {
                        AccountingPeriodId = accountingPeriod.Id,
                        EventDate = new DateOnly(2025, 1, 15),
                        EventSequence = 1,
                        Parts =
                        [
                            TransactionBalanceEventPartType.AddedDebit
                        ]
                    }
                ]
            });
        new AccountBalanceByEventValidator().Validate(
            GetService<AccountBalanceService>().GetAccountBalancesByEvent(account.Id,
                new DateRange(new DateOnly(2025, 1, 15), new DateOnly(2025, 1, 15))),
            [
                new AccountBalanceByEventState
                {
                    AccountingPeriodId = accountingPeriod.Id,
                    EventDate = new DateOnly(2025, 1, 15),
                    EventSequence = 1,
                    AccountId = account.Id,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundId = fund.Id,
                            Amount = 2500.00m
                        }
                    ],
                    PendingFundBalanceChanges =
                    [
                        new FundAmountState
                        {
                            FundId = fund.Id,
                            Amount = 500.00m
                        }
                    ]
                }
            ]);
    }

    /// <summary>
    /// Runs the test for adding a Transaction that credits a standard Account
    /// </summary>
    [Fact]
    public void RunStandardCreditTest()
    {
        AccountingPeriod accountingPeriod = GetService<AccountingPeriodBuilder>().Build();
        Fund fund = GetService<FundBuilder>().Build();
        Account account = GetService<AccountBuilder>().Build();

        Transaction transaction = GetService<TransactionBuilder>()
            .WithCreditAccount(account.Id)
            .WithCreditFundAmounts(
                [
                    new FundAmount
                    {
                        FundId = fund.Id,
                        Amount = 500.00m
                    }
                ])
            .Build();
        new TransactionValidator().Validate(transaction,
            new TransactionState
            {
                AccountingPeriodId = accountingPeriod.Id,
                Date = new DateOnly(2025, 1, 15),
                CreditAccountId = account.Id,
                CreditFundAmounts =
                [
                    new FundAmountState
                    {
                        FundId = fund.Id,
                        Amount = 500.00m,
                    }
                ],
                TransactionBalanceEvents =
                [
                    new TransactionBalanceEventState
                    {
                        AccountingPeriodId = accountingPeriod.Id,
                        EventDate = new DateOnly(2025, 1, 15),
                        EventSequence = 1,
                        Parts =
                        [
                            TransactionBalanceEventPartType.AddedCredit
                        ]
                    }
                ]
            });
        new AccountBalanceByEventValidator().Validate(
            GetService<AccountBalanceService>().GetAccountBalancesByEvent(account.Id,
                new DateRange(new DateOnly(2025, 1, 15), new DateOnly(2025, 1, 15))),
            [
                new AccountBalanceByEventState
                {
                    AccountingPeriodId = accountingPeriod.Id,
                    EventDate = new DateOnly(2025, 1, 15),
                    EventSequence = 1,
                    AccountId = account.Id,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundId = fund.Id,
                            Amount = 2500.00m
                        }
                    ],
                    PendingFundBalanceChanges =
                    [
                        new FundAmountState
                        {
                            FundId = fund.Id,
                            Amount = 500.00m
                        }
                    ]
                }
            ]);
    }

    /// <summary>
    /// Runs the test for adding a Transaction that credits a debt Account
    /// </summary>
    [Fact]
    public void RunDebtCreditTest()
    {
        AccountingPeriod accountingPeriod = GetService<AccountingPeriodBuilder>().Build();
        Fund fund = GetService<FundBuilder>().Build();
        Account account = GetService<AccountBuilder>()
            .WithType(AccountType.Debt)
            .Build();

        Transaction transaction = GetService<TransactionBuilder>()
            .WithCreditAccount(account.Id)
            .WithCreditFundAmounts(
                [
                    new FundAmount
                    {
                        FundId = fund.Id,
                        Amount = 500.00m
                    }
                ])
            .Build();
        new TransactionValidator().Validate(transaction,
            new TransactionState
            {
                AccountingPeriodId = accountingPeriod.Id,
                Date = new DateOnly(2025, 1, 15),
                CreditAccountId = account.Id,
                CreditFundAmounts =
                [
                    new FundAmountState
                    {
                        FundId = fund.Id,
                        Amount = 500.00m,
                    }
                ],
                TransactionBalanceEvents =
                [
                    new TransactionBalanceEventState
                    {
                        AccountingPeriodId = accountingPeriod.Id,
                        EventDate = new DateOnly(2025, 1, 15),
                        EventSequence = 1,
                        Parts =
                        [
                            TransactionBalanceEventPartType.AddedCredit
                        ]
                    }
                ]
            });
        new AccountBalanceByEventValidator().Validate(
            GetService<AccountBalanceService>().GetAccountBalancesByEvent(account.Id,
                new DateRange(new DateOnly(2025, 1, 15), new DateOnly(2025, 1, 15))),
            [
                new AccountBalanceByEventState
                {
                    AccountingPeriodId = accountingPeriod.Id,
                    EventDate = new DateOnly(2025, 1, 15),
                    EventSequence = 1,
                    AccountId = account.Id,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundId = fund.Id,
                            Amount = 2500.00m
                        }
                    ],
                    PendingFundBalanceChanges =
                    [
                        new FundAmountState
                        {
                            FundId = fund.Id,
                            Amount = -500.00m
                        }
                    ]
                }
            ]);
    }

    /// <summary>
    /// Runs the test for adding a Transaction that transfers between two standard Accounts
    /// </summary>
    [Fact]
    public void RunStandardTransferTest()
    {
        AccountingPeriod accountingPeriod = GetService<AccountingPeriodBuilder>().Build();
        Fund fund = GetService<FundBuilder>().Build();
        Account debitAccount = GetService<AccountBuilder>().Build();
        Account creditAccount = GetService<AccountBuilder>().Build();

        Transaction transaction = GetService<TransactionBuilder>()
            .WithDebitAccount(debitAccount.Id)
            .WithDebitFundAmounts(
                [
                    new FundAmount
                    {
                        FundId = fund.Id,
                        Amount = 500.00m
                    }
                ])
            .WithCreditAccount(creditAccount.Id)
            .WithCreditFundAmounts(
                [
                    new FundAmount
                    {
                        FundId = fund.Id,
                        Amount = 500.00m
                    }
                ])
            .Build();
        new TransactionValidator().Validate(transaction,
            new TransactionState
            {
                AccountingPeriodId = accountingPeriod.Id,
                Date = new DateOnly(2025, 1, 15),
                DebitAccountId = debitAccount.Id,
                DebitFundAmounts =
                [
                    new FundAmountState
                    {
                        FundId = fund.Id,
                        Amount = 500.00m,
                    }
                ],
                CreditAccountId = creditAccount.Id,
                CreditFundAmounts =
                [
                    new FundAmountState
                    {
                        FundId = fund.Id,
                        Amount = 500.00m,
                    }
                ],
                TransactionBalanceEvents =
                [
                    new TransactionBalanceEventState
                    {
                        AccountingPeriodId = accountingPeriod.Id,
                        EventDate = new DateOnly(2025, 1, 15),
                        EventSequence = 1,
                        Parts =
                        [
                            TransactionBalanceEventPartType.AddedDebit,
                            TransactionBalanceEventPartType.AddedCredit
                        ]
                    }
                ]
            });
        new AccountBalanceByEventValidator().Validate(
            GetService<AccountBalanceService>().GetAccountBalancesByEvent(debitAccount.Id,
                new DateRange(new DateOnly(2025, 1, 15), new DateOnly(2025, 1, 15))),
            [
                new AccountBalanceByEventState
                {
                    AccountingPeriodId = accountingPeriod.Id,
                    EventDate = new DateOnly(2025, 1, 15),
                    EventSequence = 1,
                    AccountId = debitAccount.Id,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundId = fund.Id,
                            Amount = 2500.00m
                        }
                    ],
                    PendingFundBalanceChanges =
                    [
                        new FundAmountState
                        {
                            FundId = fund.Id,
                            Amount = -500.00m
                        }
                    ]
                }
            ]);
        new AccountBalanceByEventValidator().Validate(
            GetService<AccountBalanceService>().GetAccountBalancesByEvent(creditAccount.Id,
                new DateRange(new DateOnly(2025, 1, 15), new DateOnly(2025, 1, 15))),
            [
                new AccountBalanceByEventState
                {
                    AccountingPeriodId = accountingPeriod.Id,
                    EventDate = new DateOnly(2025, 1, 15),
                    EventSequence = 1,
                    AccountId = creditAccount.Id,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundId = fund.Id,
                            Amount = 2500.00m
                        }
                    ],
                    PendingFundBalanceChanges =
                    [
                        new FundAmountState
                        {
                            FundId = fund.Id,
                            Amount = 500.00m
                        }
                    ]
                }
            ]);
    }

    /// <summary>
    /// Runs the test for adding a Transaction that transfers between two debt Accounts
    /// </summary>
    [Fact]
    public void RunDebtTransferTest()
    {
        AccountingPeriod accountingPeriod = GetService<AccountingPeriodBuilder>().Build();
        Fund fund = GetService<FundBuilder>().Build();
        Account debitAccount = GetService<AccountBuilder>()
            .WithType(AccountType.Debt)
            .Build();
        Account creditAccount = GetService<AccountBuilder>()
            .WithType(AccountType.Debt)
            .Build();

        Transaction transaction = GetService<TransactionBuilder>()
            .WithDebitAccount(debitAccount.Id)
            .WithDebitFundAmounts(
                [
                    new FundAmount
                    {
                        FundId = fund.Id,
                        Amount = 500.00m
                    }
                ])
            .WithCreditAccount(creditAccount.Id)
            .WithCreditFundAmounts(
                [
                    new FundAmount
                    {
                        FundId = fund.Id,
                        Amount = 500.00m
                    }
                ])
            .Build();
        new TransactionValidator().Validate(transaction,
            new TransactionState
            {
                AccountingPeriodId = accountingPeriod.Id,
                Date = new DateOnly(2025, 1, 15),
                DebitAccountId = debitAccount.Id,
                DebitFundAmounts =
                [
                    new FundAmountState
                    {
                        FundId = fund.Id,
                        Amount = 500.00m,
                    }
                ],
                CreditAccountId = creditAccount.Id,
                CreditFundAmounts =
                [
                    new FundAmountState
                    {
                        FundId = fund.Id,
                        Amount = 500.00m,
                    }
                ],
                TransactionBalanceEvents =
                [
                    new TransactionBalanceEventState
                    {
                        AccountingPeriodId = accountingPeriod.Id,
                        EventDate = new DateOnly(2025, 1, 15),
                        EventSequence = 1,
                        Parts =
                        [
                            TransactionBalanceEventPartType.AddedDebit,
                            TransactionBalanceEventPartType.AddedCredit
                        ]
                    }
                ]
            });
        new AccountBalanceByEventValidator().Validate(
            GetService<AccountBalanceService>().GetAccountBalancesByEvent(debitAccount.Id,
                new DateRange(new DateOnly(2025, 1, 15), new DateOnly(2025, 1, 15))),
            [
                new AccountBalanceByEventState
                {
                    AccountingPeriodId = accountingPeriod.Id,
                    EventDate = new DateOnly(2025, 1, 15),
                    EventSequence = 1,
                    AccountId = debitAccount.Id,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundId = fund.Id,
                            Amount = 2500.00m
                        }
                    ],
                    PendingFundBalanceChanges =
                    [
                        new FundAmountState
                        {
                            FundId = fund.Id,
                            Amount = 500.00m
                        }
                    ]
                }
            ]);
        new AccountBalanceByEventValidator().Validate(
            GetService<AccountBalanceService>().GetAccountBalancesByEvent(creditAccount.Id,
                new DateRange(new DateOnly(2025, 1, 15), new DateOnly(2025, 1, 15))),
            [
                new AccountBalanceByEventState
                {
                    AccountingPeriodId = accountingPeriod.Id,
                    EventDate = new DateOnly(2025, 1, 15),
                    EventSequence = 1,
                    AccountId = creditAccount.Id,
                    FundBalances =
                    [
                        new FundAmountState
                        {
                            FundId = fund.Id,
                            Amount = 2500.00m
                        }
                    ],
                    PendingFundBalanceChanges =
                    [
                        new FundAmountState
                        {
                            FundId = fund.Id,
                            Amount = -500.00m
                        }
                    ]
                }
            ]);
    }
}