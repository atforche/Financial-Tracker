using Domain.Aggregates.AccountingPeriods;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Funds;
using Domain.Services;
using Domain.Services.Implementations;
using Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tests.Mocks;

namespace Tests.AccountingPeriodTests;

/// <summary>
/// Test class that tests creating an AccountingPeriod
/// </summary>
public class CreateAccountingPeriodTests
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public CreateAccountingPeriodTests()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IAccountRepository, MockAccountRepository>();
        serviceCollection.AddScoped<IAccountingPeriodRepository, MockAccountingPeriodRepository>();
        serviceCollection.AddScoped<IFundRepository, MockFundRepository>();
        serviceCollection.AddScoped<IAccountingPeriodService, AccountingPeriodService>();
        serviceCollection.AddScoped<IAccountBalanceService, AccountBalanceService>();
        serviceCollection.AddScoped<IAccountService, AccountService>();
        serviceCollection.AddScoped<IFundService, FundService>();

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    /// <summary>
    /// Tests a new Accounting Period can be added successfully
    /// </summary>
    [Fact]
    public void CreateNewAccountingPeriod()
    {
        const int year = 2024;
        const int month = 11;

        IAccountingPeriodService accountingPeriodService = GetService<IAccountingPeriodService>();

        AccountingPeriod newAccountingPeriod = accountingPeriodService.CreateNewAccountingPeriod(year, month);
        Assert.NotEqual(newAccountingPeriod.Id.ExternalId, Guid.NewGuid());
        Assert.Equal(year, newAccountingPeriod.Year);
        Assert.Equal(month, newAccountingPeriod.Month);
        Assert.Empty(newAccountingPeriod.AccountBalanceCheckpoints);
        Assert.Empty(newAccountingPeriod.Transactions);
    }

    /// <summary>
    /// Tests that creating an Accounting Period with an invalid year fails
    /// </summary>
    /// <param name="value">Value to test</param>
    [Theory]
    [InlineData(0)]
    [InlineData(3000)]
    public void InvalidAccountingPeriodYear(int value)
    {
        const int month = 11;

        IAccountingPeriodService accountingPeriodService = GetService<IAccountingPeriodService>();
        Assert.Throws<InvalidOperationException>(() => accountingPeriodService.CreateNewAccountingPeriod(value, month));
    }

    /// <summary>
    /// Tests that creating an Accounting Period with an invalid month fails
    /// </summary>
    /// <param name="value">Value to test</param>
    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void InvalidAccountingPeriodMonth(int value)
    {
        const int year = 2023;

        IAccountingPeriodService accountingPeriodService = GetService<IAccountingPeriodService>();
        Assert.Throws<InvalidOperationException>(() => accountingPeriodService.CreateNewAccountingPeriod(year, value));
    }

    /// <summary>
    /// Tests that multiple Accounting Periods can be created
    /// </summary>
    [Fact]
    public void CreateMultipleAccountingPeriods()
    {
        IAccountingPeriodService accountingPeriodService = GetService<IAccountingPeriodService>();
        IAccountingPeriodRepository accountingPeriodRepository = GetService<IAccountingPeriodRepository>();

        // Add a first Accounting Period and mark it closed
        AccountingPeriod firstAccountingPeriod = accountingPeriodService.CreateNewAccountingPeriod(2024, 11);
        accountingPeriodRepository.Add(firstAccountingPeriod);
        accountingPeriodService.ClosePeriod(firstAccountingPeriod, null);

        // Add a second accounting period
        AccountingPeriod secondAccountingPeriod = accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        Assert.NotEqual(secondAccountingPeriod.Id.ExternalId, Guid.NewGuid());
        Assert.Equal(2024, secondAccountingPeriod.Year);
        Assert.Equal(12, secondAccountingPeriod.Month);
        Assert.Empty(secondAccountingPeriod.AccountBalanceCheckpoints);
        Assert.Empty(secondAccountingPeriod.Transactions);
    }

    /// <summary>
    /// Tests that a new Accounting Period can be created if the previous Accounting Period is still open
    /// </summary>
    [Fact]
    public void CreateMultipleOpenAccountingPeriods()
    {
        IAccountingPeriodService accountingPeriodService = GetService<IAccountingPeriodService>();
        IAccountingPeriodRepository accountingPeriodRepository = GetService<IAccountingPeriodRepository>();

        // Add a first Accounting Period and leave it open
        AccountingPeriod firstAccountingPeriod = accountingPeriodService.CreateNewAccountingPeriod(2024, 11);
        accountingPeriodRepository.Add(firstAccountingPeriod);

        // Add a second accounting period
        AccountingPeriod secondAccountingPeriod = accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        Assert.NotEqual(secondAccountingPeriod.Id.ExternalId, Guid.NewGuid());
        Assert.Equal(2024, secondAccountingPeriod.Year);
        Assert.Equal(12, secondAccountingPeriod.Month);
        Assert.Empty(secondAccountingPeriod.AccountBalanceCheckpoints);
        Assert.Empty(secondAccountingPeriod.Transactions);
    }

    /// <summary>
    /// Tests that creating non-contiguous Accounting Periods fails
    /// </summary>
    [Fact]
    public void CreateNonContiguousAccountingPeriods()
    {
        IAccountingPeriodService accountingPeriodService = GetService<IAccountingPeriodService>();
        IAccountingPeriodRepository accountingPeriodRepository = GetService<IAccountingPeriodRepository>();

        // Add a first Accounting Period
        AccountingPeriod firstAccountingPeriod = accountingPeriodService.CreateNewAccountingPeriod(2024, 11);
        accountingPeriodRepository.Add(firstAccountingPeriod);

        // Attempt to add a second Accounting Period with a gap 
        Assert.Throws<InvalidOperationException>(() => accountingPeriodService.CreateNewAccountingPeriod(2025, 1));
    }

    /// <summary>
    /// Tests that creating an Accounting Period that falls before an existing Accounting Period fails
    /// </summary>
    [Fact]
    public void CreateAccountingPeriodInPast()
    {
        IAccountingPeriodService accountingPeriodService = GetService<IAccountingPeriodService>();
        IAccountingPeriodRepository accountingPeriodRepository = GetService<IAccountingPeriodRepository>();

        // Add a first Accounting Period
        AccountingPeriod firstAccountingPeriod = accountingPeriodService.CreateNewAccountingPeriod(2024, 11);
        accountingPeriodRepository.Add(firstAccountingPeriod);

        // Attempt to add a second Accounting Period in the past
        Assert.Throws<InvalidOperationException>(() => accountingPeriodService.CreateNewAccountingPeriod(2024, 10));
    }

    /// <summary>
    /// Tests that creating an Accounting Period when there's a previous open Accounting Period with an Account
    /// won't create any Account Balance Checkpoints
    /// </summary>
    [Fact]
    public void CreateAccountingPeriodNoBalanceCheckpoints()
    {
        IAccountingPeriodService accountingPeriodService = GetService<IAccountingPeriodService>();
        IAccountingPeriodRepository accountingPeriodRepository = GetService<IAccountingPeriodRepository>();
        IAccountService accountService = GetService<IAccountService>();
        IAccountRepository accountRepository = GetService<IAccountRepository>();
        IFundService fundService = GetService<IFundService>();
        IFundRepository fundRepository = GetService<IFundRepository>();

        // Add a test fund
        Fund fund = fundService.CreateNewFund("Test");
        fundRepository.Add(fund);

        // Add a first Accounting Period
        AccountingPeriod firstAccountingPeriod = accountingPeriodService.CreateNewAccountingPeriod(2024, 11);
        accountingPeriodRepository.Add(firstAccountingPeriod);

        // Add an Account with an initial period of the first Accounting Period
        var fundAmount = new FundAmount
        {
            Fund = fund,
            Amount = 2500.00m,
        };
        Account account = accountService.CreateNewAccount(firstAccountingPeriod, "Test", AccountType.Standard, [fundAmount]);
        accountRepository.Add(account);

        // Verify that Account Balances Checkpoints were created for the first Accounting Period
        Assert.Equal(2, firstAccountingPeriod.AccountBalanceCheckpoints.Count);
        var periodCheckpoint = firstAccountingPeriod.AccountBalanceCheckpoints
            .Single(checkpoint => checkpoint.Type == AccountBalanceCheckpointType.StartOfPeriod);
        Assert.Equal(account, periodCheckpoint.Account);
        Assert.Single(periodCheckpoint.FundBalances);
        Assert.Equal(fundAmount, periodCheckpoint.FundBalances.First());
        var monthCheckpoint = firstAccountingPeriod.AccountBalanceCheckpoints
            .Single(checkpoint => checkpoint.Type == AccountBalanceCheckpointType.StartOfMonth);
        Assert.Equal(account, monthCheckpoint.Account);
        Assert.Single(monthCheckpoint.FundBalances);
        Assert.Equal(fundAmount, monthCheckpoint.FundBalances.First());

        // Add a new accounting period, leaving the first accounting period open
        AccountingPeriod secondAccountingPeriod = accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        accountingPeriodRepository.Add(secondAccountingPeriod);

        // Verify that no Account Balance Checkpoints were added
        Assert.Empty(secondAccountingPeriod.AccountBalanceCheckpoints);
    }

    /// <summary>
    /// Tests that creating an Accounting Period when there's a previous closed Accounting Period with an Account
    /// will create Account Balance Checkpoints
    /// </summary>
    /// <summary>
    /// Tests that creating an Accounting Period when there's a previous open Accounting Period with an Account
    /// won't create any Account Balance Checkpoints
    /// </summary>
    [Fact]
    public void CreateAccountingPeriodBalanceCheckpoints()
    {
        IAccountingPeriodService accountingPeriodService = GetService<IAccountingPeriodService>();
        IAccountingPeriodRepository accountingPeriodRepository = GetService<IAccountingPeriodRepository>();
        IAccountService accountService = GetService<IAccountService>();
        IAccountRepository accountRepository = GetService<IAccountRepository>();
        IFundService fundService = GetService<IFundService>();
        IFundRepository fundRepository = GetService<IFundRepository>();

        // Add a test fund
        Fund fund = fundService.CreateNewFund("Test");
        fundRepository.Add(fund);

        // Add a first Accounting Period
        AccountingPeriod firstAccountingPeriod = accountingPeriodService.CreateNewAccountingPeriod(2024, 11);
        accountingPeriodRepository.Add(firstAccountingPeriod);

        // Add an Account with an initial period of the first Accounting Period
        var fundAmount = new FundAmount
        {
            Fund = fund,
            Amount = 2500.00m,
        };
        Account account = accountService.CreateNewAccount(firstAccountingPeriod, "Test", AccountType.Standard, [fundAmount]);
        accountRepository.Add(account);

        // Verify that Account Balances Checkpoints were created for the first Accounting Period
        Assert.Equal(2, firstAccountingPeriod.AccountBalanceCheckpoints.Count);
        var periodCheckpoint = firstAccountingPeriod.AccountBalanceCheckpoints
            .Single(checkpoint => checkpoint.Type == AccountBalanceCheckpointType.StartOfPeriod);
        Assert.Equal(account, periodCheckpoint.Account);
        Assert.Single(periodCheckpoint.FundBalances);
        Assert.Equal(fundAmount, periodCheckpoint.FundBalances.First());
        var monthCheckpoint = firstAccountingPeriod.AccountBalanceCheckpoints
            .Single(checkpoint => checkpoint.Type == AccountBalanceCheckpointType.StartOfMonth);
        Assert.Equal(account, monthCheckpoint.Account);
        Assert.Single(monthCheckpoint.FundBalances);
        Assert.Equal(fundAmount, monthCheckpoint.FundBalances.First());

        // Close the first accounting period
        accountingPeriodService.ClosePeriod(firstAccountingPeriod, null);

        // Add a new accounting period
        AccountingPeriod secondAccountingPeriod = accountingPeriodService.CreateNewAccountingPeriod(2024, 12);
        accountingPeriodRepository.Add(secondAccountingPeriod);

        // Verify that Account Balances Checkpoints were created for the second Accounting Period
        Assert.Equal(2, secondAccountingPeriod.AccountBalanceCheckpoints.Count);
        periodCheckpoint = secondAccountingPeriod.AccountBalanceCheckpoints
            .Single(checkpoint => checkpoint.Type == AccountBalanceCheckpointType.StartOfPeriod);
        Assert.Equal(account, periodCheckpoint.Account);
        Assert.Single(periodCheckpoint.FundBalances);
        Assert.Equal(fundAmount, periodCheckpoint.FundBalances.First());
        monthCheckpoint = secondAccountingPeriod.AccountBalanceCheckpoints
            .Single(checkpoint => checkpoint.Type == AccountBalanceCheckpointType.StartOfMonth);
        Assert.Equal(account, monthCheckpoint.Account);
        Assert.Single(monthCheckpoint.FundBalances);
        Assert.Equal(fundAmount, monthCheckpoint.FundBalances.First());
    }

    /// <summary>
    /// Gets the service of the provided type from the service provider
    /// </summary>
    private TService GetService<TService>() =>
        _serviceProvider.GetService<TService>() ?? throw new InvalidOperationException();
}