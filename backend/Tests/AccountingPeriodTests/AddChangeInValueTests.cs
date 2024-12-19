using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Validators;

namespace Tests.AccountingPeriodTests;

/// <summary>
/// Test class that tests adding a Change In Value to an Accounting Period
/// </summary>
public class AddChangeInValueTests : UnitTestBase
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
    public AddChangeInValueTests()
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
                },
            ]);
        _accountRepository.Add(_testAccount);
    }

    /// <summary>
    /// Tests that a Change In Value can be added successfully
    /// </summary>
    [Fact]
    public void TestAddChangeInValue()
    {
        ChangeInValue changeInValue = _accountingPeriodService.AddChangeInValue(_testAccountingPeriod,
            new DateOnly(2024, 11, 15),
            _testAccount,
            new FundAmount
            {
                Fund = _testFund,
                Amount = -100.00m,
            });
        new ChangeInValueValidator(changeInValue).Validate(new ChangeInValueState
        {
            AccountName = _testAccount.Name,
            EventDate = new DateOnly(2024, 11, 15),
            EventSequence = 1,
            AccountingEntry = new FundAmountState
            {
                FundName = _testFund.Name,
                Amount = -100.00m,
            }
        });
    }

    /// <summary>
    /// Tests that adding a Change In Value to a closed Accounting Period will fail
    /// </summary>
    [Fact]
    public void TestWithClosedAccountingPeriod()
    {
        _accountingPeriodService.ClosePeriod(_testAccountingPeriod);
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.AddChangeInValue(_testAccountingPeriod,
            new DateOnly(2024, 11, 15),
            _testAccount,
            new FundAmount
            {
                Fund = _testFund,
                Amount = -100.00m,
            }));
    }

    /// <summary>
    /// Test that a Change In Value can be added to an Accounting Period when its date falls in a different Accounting Period
    /// </summary>
    [Fact]
    public void TestWithDateThatFallsInDifferentAccountingPeriod()
    {
        // Add an additional Accounting Period
        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        _accountingPeriodRepository.Add(secondAccountingPeriod);

        // Add a Change In Value to the first Accounting Period that falls in the second period
        ChangeInValue changeInValue = _accountingPeriodService.AddChangeInValue(_testAccountingPeriod,
            new DateOnly(2024, 12, 15),
            _testAccount,
            new FundAmount
            {
                Fund = _testFund,
                Amount = -100.00m,
            });
        new ChangeInValueValidator(changeInValue).Validate(new ChangeInValueState
        {
            AccountName = _testAccount.Name,
            EventDate = new DateOnly(2024, 12, 15),
            EventSequence = 1,
            AccountingEntry = new FundAmountState
            {
                FundName = _testFund.Name,
                Amount = -100.00m,
            }
        });

        // Add a Change In Value to the second Accounting Period that falls in the first period
        changeInValue = _accountingPeriodService.AddChangeInValue(secondAccountingPeriod,
            new DateOnly(2024, 11, 15),
            _testAccount,
            new FundAmount
            {
                Fund = _testFund,
                Amount = -100.00m,
            });
        new ChangeInValueValidator(changeInValue).Validate(new ChangeInValueState
        {
            AccountName = _testAccount.Name,
            EventDate = new DateOnly(2024, 11, 15),
            EventSequence = 1,
            AccountingEntry = new FundAmountState
            {
                FundName = _testFund.Name,
                Amount = -100.00m,
            }
        });
    }

    /// <summary>
    /// Test that adding a Change In Value with an invalid ate will fail
    /// </summary>
    [Fact]
    public void TestWithInvalidDate()
    {
        // Test that adding a Change In Value to far in the past will fail
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.AddChangeInValue(_testAccountingPeriod,
            new DateOnly(2024, 9, 15),
            _testAccount,
            new FundAmount
            {
                Fund = _testFund,
                Amount = -100.00m,
            }));

        // Test that adding a Change In Value to far in the future will fail
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.AddChangeInValue(_testAccountingPeriod,
            new DateOnly(2025, 1, 15),
            _testAccount,
            new FundAmount
            {
                Fund = _testFund,
                Amount = -100.00m,
            }));
    }

    /// <summary>
    /// Test that adding a Change In Value with an invalid amount will fail
    /// </summary>
    [Fact]
    public void TestWithInvalidAmount()
    {
        // Tests that having an amount of zero will fail
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.AddChangeInValue(_testAccountingPeriod,
            new DateOnly(2024, 11, 15),
            _testAccount,
            new FundAmount
            {
                Fund = _testFund,
                Amount = 0.00m,
            }));
    }

    /// <summary>
    /// Tests that adding a Change In Value affects the Account's balances as expected
    /// </summary>
    [Fact]
    public void TestEffectOnAccountBalance()
    {
        // Add a Change In Value with a positive amount
        _accountingPeriodService.AddChangeInValue(_testAccountingPeriod,
            new DateOnly(2024, 11, 10),
            _testAccount,
            new FundAmount
            {
                Fund = _testFund,
                Amount = 100.00m,
            });

        // Add a Change In Value with a negative amount
        _accountingPeriodService.AddChangeInValue(_testAccountingPeriod,
            new DateOnly(2024, 11, 15),
            _testAccount,
            new FundAmount
            {
                Fund = _testFund,
                Amount = -100.00m,
            });

        new AccountBalanceByEventValidator(_accountBalanceService.GetAccountBalancesByEvent(
            _testAccount,
            new DateRange(new DateOnly(2024, 11, 1), new DateOnly(2024, 11, 30))))
            .Validate(
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
                                Amount = 2600.00m,
                            }
                        ],
                        PendingFundBalanceChanges = [],
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
                        PendingFundBalanceChanges = [],
                    },
                ]);
    }

    /// <summary>
    /// Tests that adding a Change In Value affects a debt Account's balances as expected (no difference)
    /// </summary>
    [Fact]
    public void TestEffectOnDebtAccountBalance()
    {
        Account debtAccount = _accountService.CreateNewAccount("Debt", AccountType.Debt,
        [
            new FundAmount
            {
                Fund = _testFund,
                Amount = 2500.00m,
            }
        ]);

        // Add a Change In Value with a positive amount
        _accountingPeriodService.AddChangeInValue(_testAccountingPeriod,
            new DateOnly(2024, 11, 10),
            debtAccount,
            new FundAmount
            {
                Fund = _testFund,
                Amount = 100.00m,
            });

        // Add a Change In Value with a negative amount
        _accountingPeriodService.AddChangeInValue(_testAccountingPeriod,
            new DateOnly(2024, 11, 15),
            debtAccount,
            new FundAmount
            {
                Fund = _testFund,
                Amount = -100.00m,
            });

        new AccountBalanceByEventValidator(_accountBalanceService.GetAccountBalancesByEvent(
            debtAccount,
            new DateRange(new DateOnly(2024, 11, 1), new DateOnly(2024, 11, 30))))
            .Validate(
                [
                    new AccountBalanceByEventState
                    {
                        AccountName = debtAccount.Name,
                        AccountingPeriodYear = _testAccountingPeriod.Year,
                        AccountingPeriodMonth = _testAccountingPeriod.Month,
                        EventDate = new DateOnly(2024, 11, 10),
                        EventSequence = 1,
                        FundBalances =
                        [
                            new FundAmountState
                            {
                                FundName = _testFund.Name,
                                Amount = 2600.00m,
                            }
                        ],
                        PendingFundBalanceChanges = [],
                    },
                    new AccountBalanceByEventState
                    {
                        AccountName = debtAccount.Name,
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
                        PendingFundBalanceChanges = [],
                    },
                ]);
    }


    /// <summary>
    /// Tests that adding a Change In Value that invalidates a future Change In Value will fail
    /// </summary>
    [Fact]
    public void TestInvalidatesFutureChangeInValue()
    {
        ChangeInValue changeInValue = _accountingPeriodService.AddChangeInValue(_testAccountingPeriod,
            new DateOnly(2024, 11, 15),
            _testAccount,
            new FundAmount
            {
                Fund = _testFund,
                Amount = -100.00m,
            });
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.AddChangeInValue(_testAccountingPeriod,
            new DateOnly(2024, 11, 10),
            _testAccount,
            new FundAmount
            {
                Fund = _testFund,
                Amount = -2450.00m,
            }));
    }

    /// <summary>
    /// Tests that adding a Change In Value that causes a future Change In Value to invalidate an Account's balance
    /// within the Accounting Period will fail
    /// </summary>
    [Fact]
    public void TestInvalidatesFutureChangeInValueWithinAccountingPeriod()
    {
        // Add an additional Accounting Period
        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        _accountingPeriodRepository.Add(secondAccountingPeriod);

        ChangeInValue changeInValue = _accountingPeriodService.AddChangeInValue(_testAccountingPeriod,
            new DateOnly(2024, 11, 15),
            _testAccount,
            new FundAmount
            {
                Fund = _testFund,
                Amount = -100.00m,
            });
        ChangeInValue secondChangeInValue = _accountingPeriodService.AddChangeInValue(secondAccountingPeriod,
            new DateOnly(2024, 11, 10),
            _testAccount,
            new FundAmount
            {
                Fund = _testFund,
                Amount = 5000.00m,
            });
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.AddChangeInValue(_testAccountingPeriod,
            new DateOnly(2024, 11, 5),
            _testAccount,
            new FundAmount
            {
                Fund = _testFund,
                Amount = -2450.00m,
            }));
    }

    /// <summary>
    /// Tests that adding a Change In Value that invalidates a future Transaction will fail
    /// </summary>
    [Fact]
    public void TestInvalidatesFutureTransaction()
    {
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
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.AddChangeInValue(_testAccountingPeriod,
            new DateOnly(2024, 11, 5),
            _testAccount,
            new FundAmount
            {
                Fund = _testFund,
                Amount = -2450.00m,
            }));
    }

    /// <summary>
    /// Tests that adding a Change In Value that invalidates a future Fund Conversion will fail
    /// </summary>
    [Fact]
    public void TestInvalidatesFutureFundConversion()
    {
        Fund toFund = _fundService.CreateNewFund("To");
        _fundRepository.Add(toFund);

        FundConversion fundConversion = _accountingPeriodService.AddFundConversion(_testAccountingPeriod,
            new DateOnly(2024, 11, 15),
            _testAccount,
            _testFund,
            toFund,
            100.00m);
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.AddChangeInValue(_testAccountingPeriod,
            new DateOnly(2024, 11, 5),
            _testAccount,
            new FundAmount
            {
                Fund = _testFund,
                Amount = -2450.00m,
            }));
    }

}