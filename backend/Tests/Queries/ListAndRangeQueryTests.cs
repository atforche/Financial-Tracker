using System.Net;
using Tests.AccountingPeriods;
using Tests.Infrastructure;

namespace Tests.Queries;

/// <summary>
/// Covers paged list and accounting-period range read contracts.
/// </summary>
public sealed class ListAndRangeQueryTests
{
    /// <summary>
    /// Rejects a noncontiguous accounting-period range.
    /// </summary>
    [Fact]
    public async Task AccountingPeriodRangeRejectsMissingPeriods()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();

        using HttpResponseMessage response = await test.Api.GetResponseAsync(
            $"/accounting-periods/range?range.start={july.Id}&range.end={Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}
