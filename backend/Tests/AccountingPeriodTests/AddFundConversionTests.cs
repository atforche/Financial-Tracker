using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.ValueObjects;
using Tests.Validators;

namespace Tests.AccountingPeriodTests;

/// <summary>
/// Test class that tests adding a Fund Conversion to an Accounting Period
/// </summary>
public class AddFundConversionTests : UnitTestBase
{
    private readonly IAccountingPeriodRepository _accountingPeriodRepository;
    private readonly IAccountingPeriodService _accountingPeriodService;
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountService _accountService;
    private readonly IFundRepository _fundRepository;
    private readonly IFundService _fundService;
    private readonly IAccountBalanceService _accountBalanceService;

    private readonly Fund _testFromFund;
    private readonly Fund _testToFund;
    private readonly AccountingPeriod _testAccountingPeriod;
    private readonly Account _testAccount;

    /// <summary>
    /// Constructs a new instance of this class.
    /// This constructor is run again before each individual test in this test class.
    /// </summary>
    public AddFundConversionTests()
    {
        _accountingPeriodRepository = GetService<IAccountingPeriodRepository>();
        _accountingPeriodService = GetService<IAccountingPeriodService>();
        _accountRepository = GetService<IAccountRepository>();
        _accountService = GetService<IAccountService>();
        _fundRepository = GetService<IFundRepository>();
        _fundService = GetService<IFundService>();
        _accountBalanceService = GetService<IAccountBalanceService>();

        // Setup shared by all tests
        _testFromFund = _fundService.CreateNewFund("From");
        _fundRepository.Add(_testFromFund);
        _testToFund = _fundService.CreateNewFund("To");
        _fundRepository.Add(_testToFund);
        _testAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 11);
        _accountingPeriodRepository.Add(_testAccountingPeriod);
        _testAccount = _accountService.CreateNewAccount("Test", AccountType.Standard,
            [
                new FundAmount()
                {
                    Fund = _testFromFund,
                    Amount = 2500.00m
                },
            ]);
        _accountRepository.Add(_testAccount);
    }

    /// <summary>
    /// Tests that a Fund Conversion can be added successfully
    /// </summary>
    [Fact]
    public void TestAddFundConversion()
    {
        FundConversion fundConversion = _accountingPeriodService.AddFundConversion(_testAccountingPeriod,
            new DateOnly(2024, 11, 15),
            _testAccount,
            _testFromFund,
            _testToFund,
            100.00m);
        new FundConversionValidator(fundConversion).Validate(new FundConversionState
        {
            AccountName = _testAccount.Name,
            EventDate = new DateOnly(2024, 11, 15),
            EventSequence = 1,
            FromFundName = _testFromFund.Name,
            ToFundName = _testToFund.Name,
            Amount = 100.00m,
        });
    }

    /// <summary>
    /// Tests that adding a Fund Conversion to a closed Accounting Period fails
    /// </summary>
    [Fact]
    public void TestWithClosedAccountingPeriod()
    {
        _accountingPeriodService.ClosePeriod(_testAccountingPeriod);
        Assert.Throws<InvalidOperationException>(() =>
            _accountingPeriodService.AddFundConversion(_testAccountingPeriod,
                new DateOnly(2024, 11, 15),
                _testAccount,
                _testFromFund,
                _testToFund,
                100.00m));
    }

    /// <summary>
    /// Tets that a Fund Conversion can be added to an Accounting Period when its date falls in a different Accounting Period
    /// </summary>
    [Fact]
    public void TestWithDateThatFallsInDifferentAccountingPeriod()
    {
        // Add an additional Accounting Period
        AccountingPeriod secondAccountingPeriod = _accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        _accountingPeriodRepository.Add(secondAccountingPeriod);

        // Add a Fund Conversion to the first Accounting Period that falls in the second period
        FundConversion fundConversion = _accountingPeriodService.AddFundConversion(_testAccountingPeriod,
            new DateOnly(2024, 12, 15),
            _testAccount,
            _testFromFund,
            _testToFund,
            100.00m);
        new FundConversionValidator(fundConversion).Validate(new FundConversionState
        {
            AccountName = _testAccount.Name,
            EventDate = new DateOnly(2024, 12, 15),
            EventSequence = 1,
            FromFundName = _testFromFund.Name,
            ToFundName = _testToFund.Name,
            Amount = 100.00m,
        });

        // Add a Fund Conversion to the second Accounting Period that falls in the first period
        fundConversion = _accountingPeriodService.AddFundConversion(secondAccountingPeriod,
            new DateOnly(2024, 11, 15),
            _testAccount,
            _testFromFund,
            _testToFund,
            100.00m);
        new FundConversionValidator(fundConversion).Validate(new FundConversionState
        {
            AccountName = _testAccount.Name,
            EventDate = new DateOnly(2024, 11, 15),
            EventSequence = 1,
            FromFundName = _testFromFund.Name,
            ToFundName = _testToFund.Name,
            Amount = 100.00m,
        });
    }

    /// <summary>
    /// Tests that adding a Fund Conversion with an invalid date will fail
    /// </summary>
    [Fact]
    public void TestWithInvalidDate()
    {
        // Test that adding a Fund Conversion too far in the past will fail
        Assert.Throws<InvalidOperationException>(() =>
            _accountingPeriodService.AddFundConversion(_testAccountingPeriod,
                new DateOnly(2024, 9, 15),
                _testAccount,
                _testFromFund,
                _testToFund,
                100.00m));

        // Test that adding a Fund Conversion too far in the future will fail
        Assert.Throws<InvalidOperationException>(() =>
            _accountingPeriodService.AddFundConversion(_testAccountingPeriod,
                new DateOnly(2025, 1, 15),
                _testAccount,
                _testFromFund,
                _testToFund,
                100.00m));
    }

    /// <summary>
    /// Tests that adding a Fund Conversion with an invalid Fund will fail
    /// </summary>
    [Fact]
    public void TestWithInvalidFunds() =>
        // Test that having the same from Fund and to Fund will fail
        Assert.Throws<InvalidOperationException>(() =>
            _accountingPeriodService.AddFundConversion(_testAccountingPeriod,
                new DateOnly(2024, 11, 15),
                _testAccount,
                _testFromFund,
                _testFromFund,
                100.00m));

    /// <summary>
    /// Tests that adding a Fund Conversion with an invalid amount will fail
    /// </summary>
    [Fact]
    public void TestWithInvalidAmount()
    {
        // Tests that having an amount of zero will fail
        Assert.Throws<InvalidOperationException>(() =>
            _accountingPeriodService.AddFundConversion(_testAccountingPeriod,
                new DateOnly(2024, 11, 15),
                _testAccount,
                _testFromFund,
                _testToFund,
                0.00m));

        // Tests that having a negative amount will fail
        Assert.Throws<InvalidOperationException>(() =>
            _accountingPeriodService.AddFundConversion(_testAccountingPeriod,
                new DateOnly(2024, 11, 15),
                _testAccount,
                _testFromFund,
                _testToFund,
                -100.00m));

        // Tests that having an amount greater than the current balance of the from Fund in the account will fail
        Assert.Throws<InvalidOperationException>(() =>
            _accountingPeriodService.AddFundConversion(_testAccountingPeriod,
                new DateOnly(2024, 11, 15),
                _testAccount,
                _testFromFund,
                _testToFund,
                10000.00m));
    }

    /// <summary>
    /// Tests that adding a Fund Conversion affects the Account's balances as expected
    /// </summary>
    [Fact]
    public void TestEffectOnAccountBalance()
    {
        _accountingPeriodService.AddFundConversion(_testAccountingPeriod,
            new DateOnly(2024, 11, 15),
            _testAccount,
            _testFromFund,
            _testToFund,
            100.00m);
        new AccountBalanceByEventValidator(_accountBalanceService.GetAccountBalancesByEvent(
            _testAccount,
            new DateRange(new DateOnly(2024, 11, 1), new DateOnly(2024, 11, 30))))
            .Validate(new AccountBalanceByEventState
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
                        FundName = _testFromFund.Name,
                        Amount = 2400.00m,
                    },
                    new FundAmountState
                    {
                        FundName = _testToFund.Name,
                        Amount = 100.00m,
                    }
                ],
                PendingFundBalanceChanges = [],
            });
    }

    /// <summary>
    /// Tests that adding a Fund Conversion affects a debt Account's balances as expected (no difference)
    /// </summary>
    [Fact]
    public void TestEffectOnDebtAccountBalance()
    {
        Account debtAccount = _accountService.CreateNewAccount("Debt", AccountType.Debt,
        [
            new FundAmount
            {
                Fund = _testFromFund,
                Amount = 2500.00m,
            }
        ]);

        _accountingPeriodService.AddFundConversion(_testAccountingPeriod,
            new DateOnly(2024, 11, 15),
            debtAccount,
            _testFromFund,
            _testToFund,
            100.00m);
        new AccountBalanceByEventValidator(_accountBalanceService.GetAccountBalancesByEvent(
            debtAccount,
            new DateRange(new DateOnly(2024, 11, 1), new DateOnly(2024, 11, 30))))
            .Validate(new AccountBalanceByEventState
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
                        FundName = _testFromFund.Name,
                        Amount = 2400.00m,
                    },
                    new FundAmountState
                    {
                        FundName = _testToFund.Name,
                        Amount = 100.00m,
                    }
                ],
                PendingFundBalanceChanges = [],
            });
    }

    /// <summary>
    /// Test that adding a Fund Conversion that would invalidate a future Fund Conversion will fail
    /// </summary>
    [Fact]
    public void TestInvalidateFutureFundConversion()
    {
        FundConversion fundConversion = _accountingPeriodService.AddFundConversion(_testAccountingPeriod,
            new DateOnly(2024, 11, 15),
            _testAccount,
            _testFromFund,
            _testToFund,
            100.00m);
        Assert.Throws<InvalidOperationException>(() => _accountingPeriodService.AddFundConversion(_testAccountingPeriod,
            new DateOnly(2024, 11, 10),
            _testAccount,
            _testFromFund,
            _testToFund,
            2450.00m));
    }
}