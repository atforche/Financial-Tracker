using Domain.AccountingPeriods;
using Tests.Builders;
using Tests.Mocks;
using Tests.Validators;

namespace Tests.AddAccountingPeriod;

/// <summary>
/// Test class that tests adding an Accounting Period with multiple Accounting Periods
/// </summary>
public class MultipleAccountingPeriodTests : TestClass
{
    /// <summary>
    /// Runs the default test for adding an Accounting Period with multiple Accounting Periods
    /// </summary>
    [Fact]
    public void RunDefaultTest()
    {
        AccountingPeriod firstAccountingPeriod = GetService<AccountingPeriodBuilder>().Build();
        new AccountingPeriodValidator().Validate(firstAccountingPeriod,
            new AccountingPeriodState
            {
                Year = 2025,
                Month = 1,
                IsOpen = true
            });

        AccountingPeriod secondAccountingPeriod = GetService<AccountingPeriodBuilder>()
            .WithYear(2025)
            .WithMonth(2)
            .Build();
        new AccountingPeriodValidator().Validate(secondAccountingPeriod,
            new AccountingPeriodState
            {
                Year = 2025,
                Month = 2,
                IsOpen = true
            });

        AccountingPeriod thirdAccountingPeriod = GetService<AccountingPeriodBuilder>()
            .WithYear(2025)
            .WithMonth(3)
            .Build();
        new AccountingPeriodValidator().Validate(thirdAccountingPeriod,
            new AccountingPeriodState
            {
                Year = 2025,
                Month = 3,
                IsOpen = true
            });
    }

    /// <summary>
    /// Runs the test for adding an Accounting Period with multiple closed Accounting Periods
    /// </summary>
    [Fact]
    public void RunClosedTest()
    {
        AccountingPeriod firstAccountingPeriod = GetService<AccountingPeriodBuilder>().Build();
        new AccountingPeriodValidator().Validate(firstAccountingPeriod,
            new AccountingPeriodState
            {
                Year = 2025,
                Month = 1,
                IsOpen = true
            });
        GetService<CloseAccountingPeriodAction>().Run(firstAccountingPeriod);
        GetService<TestUnitOfWork>().SaveChanges();

        AccountingPeriod secondAccountingPeriod = GetService<AccountingPeriodBuilder>()
            .WithYear(2025)
            .WithMonth(2)
            .Build();
        new AccountingPeriodValidator().Validate(secondAccountingPeriod,
            new AccountingPeriodState
            {
                Year = 2025,
                Month = 2,
                IsOpen = true
            });
        GetService<CloseAccountingPeriodAction>().Run(secondAccountingPeriod);
        GetService<TestUnitOfWork>().SaveChanges();

        AccountingPeriod thirdAccountingPeriod = GetService<AccountingPeriodBuilder>()
            .WithYear(2025)
            .WithMonth(3)
            .Build();
        new AccountingPeriodValidator().Validate(thirdAccountingPeriod,
            new AccountingPeriodState
            {
                Year = 2025,
                Month = 3,
                IsOpen = true
            });
    }

    /// <summary>
    /// Runs the test for adding an Accounting Period with a gap between the previous Accounting Period
    /// </summary>
    [Fact]
    public void RunGapTest()
    {
        GetService<AccountingPeriodBuilder>().Build();
        Assert.Throws<InvalidOperationException>(() => GetService<AccountingPeriodBuilder>()
            .WithYear(2025)
            .WithMonth(5)
            .Build());
    }

    /// <summary>
    /// Runs the test for adding an Accounting Period when a duplicate Accounting Period already exists
    /// </summary>
    [Fact]
    public void RunDuplicateTest()
    {
        GetService<AccountingPeriodBuilder>().Build();
        Assert.Throws<InvalidOperationException>(() => GetService<AccountingPeriodBuilder>().Build());
    }

    /// <summary>
    /// Runs the test for adding an Accounting Period when a future Accounting Period already exists
    /// </summary>
    [Fact]
    public void RunPastTest()
    {
        GetService<AccountingPeriodBuilder>().Build();
        Assert.Throws<InvalidOperationException>(() => GetService<AccountingPeriodBuilder>()
            .WithYear(2024)
            .WithMonth(12)
            .Build());
    }
}