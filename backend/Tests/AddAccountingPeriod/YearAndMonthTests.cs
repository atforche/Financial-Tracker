using Domain.AccountingPeriods;
using Tests.Builders;
using Tests.Validators;

namespace Tests.AddAccountingPeriod;

/// <summary>
/// Test class that tests adding an Accounting Period with different years and months
/// </summary>
public class YearAndMonthTests : TestClass
{
    /// <summary>
    /// Runs the tests for adding an Accounting Period with different years and months
    /// </summary>
    [Theory]
    [MemberData(nameof(GetYearsAndMonthsToTest))]
    public void RunTest(int year, int month)
    {
        if (month is < 1 or > 12 || year is < 2000 or > 2100)
        {
            Assert.Throws<InvalidOperationException>(() => GetService<AccountingPeriodBuilder>()
                .WithYear(year)
                .WithMonth(month)
                .Build());
        }
        else
        {
            AccountingPeriod accountingPeriod = GetService<AccountingPeriodBuilder>()
                .WithYear(year)
                .WithMonth(month)
                .Build();
            new AccountingPeriodValidator().Validate(accountingPeriod,
                new AccountingPeriodState
                {
                    Year = year,
                    Month = month,
                    IsOpen = true
                });
        }
    }

    /// <summary>
    /// Gets the collection of years and months to test
    /// </summary>
    public static IEnumerable<TheoryDataRow<int, int>> GetYearsAndMonthsToTest()
    {
        List<int> yearsToTest = [0, 2025, 3000];
        var monthsToTest = Enumerable.Range(0, 14).ToList();
        foreach (int year in yearsToTest)
        {
            foreach (int month in monthsToTest)
            {
                yield return new TheoryDataRow<int, int>(year, month);
            }
        }
    }
}