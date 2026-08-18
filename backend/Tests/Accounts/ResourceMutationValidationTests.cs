using System.Net;
using Models.Accounts;
using Models.Funds;
using Tests.AccountingPeriods;
using Tests.Infrastructure;

namespace Tests.Accounts;

/// <summary>
/// Covers missing-resource and closed-period validation at account and fund mutation boundaries.
/// </summary>
public sealed class ResourceMutationValidationTests
{
    /// <summary>
    /// Rejects missing resources and prohibits adding accounts or funds to a closed Accounting Period.
    /// </summary>
    [Fact]
    public async Task ResourceMutationsRejectMissingResourcesAndClosedPeriods()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        await test.Api.PostAsync($"/accounting-periods/{july.Id}/close");

        using HttpResponseMessage missingAccountPeriod = await test.Api.PostResponseAsync("/accounts", new CreateAccountModel
        {
            Name = "Missing period",
            Type = AccountTypeModel.Standard,
            OpeningAccountingPeriodId = Guid.NewGuid(),
            DateOpened = new DateOnly(2026, 7, 1)
        });
        using HttpResponseMessage closedAccountPeriod = await test.Api.PostResponseAsync("/accounts", new CreateAccountModel
        {
            Name = "Closed account",
            Type = AccountTypeModel.Standard,
            OpeningAccountingPeriodId = july.Id,
            DateOpened = new DateOnly(2026, 7, 1)
        });
        using HttpResponseMessage closedFundPeriod = await test.Api.PostResponseAsync("/funds", new CreateFundModel
        {
            Name = "Closed fund",
            Description = "Closed fund",
            AccountingPeriodId = july.Id
        });
        using HttpResponseMessage missingFundUpdate = await test.Api.PostResponseAsync($"/funds/{Guid.NewGuid()}", new UpdateFundModel
        {
            Name = "Missing",
            Description = "Missing"
        });
        using HttpResponseMessage missingFundDelete = await test.Api.DeleteResponseAsync($"/funds/{Guid.NewGuid()}");
        using HttpResponseMessage missingPeriodClose = await test.Api.PostResponseAsync($"/accounting-periods/{Guid.NewGuid()}/close", new { });
        using HttpResponseMessage missingPeriodReopen = await test.Api.PostResponseAsync($"/accounting-periods/{Guid.NewGuid()}/reopen", new { });
        using HttpResponseMessage missingPeriodDelete = await test.Api.DeleteResponseAsync($"/accounting-periods/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, missingAccountPeriod.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, closedAccountPeriod.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, closedFundPeriod.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, missingFundUpdate.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, missingFundDelete.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, missingPeriodClose.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, missingPeriodReopen.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, missingPeriodDelete.StatusCode);
    }
}
