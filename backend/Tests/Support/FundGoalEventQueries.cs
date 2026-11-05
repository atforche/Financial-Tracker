using Domain;
using Domain.AccountingPeriods.Queries;
using Domain.FundGoals.Queries;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Models.FundGoals;
using Rest.FundGoals;

namespace Tests.Support;

/// <summary>
/// Reads interpreted Fund Goal balance events directly from the test application's domain service.
/// </summary>
internal sealed class FundGoalEventQueries(FinancialTrackerApplicationFactory factory)
{
    /// <summary>
    /// Gets the Fund Goal balance events in an Accounting Period.
    /// </summary>
    public async Task<CollectionModel<FundGoalBalanceEventModel>> GetAsync(
        AccountingPeriodHandle period,
        FundGoalBalanceEventSortModel? sort = null,
        int? limit = null)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        FundGoalBalanceEventConverter converter = scope.ServiceProvider.GetRequiredService<FundGoalBalanceEventConverter>();
        AccountingPeriodRangeQueryResult<QueryPage<FundGoalBalanceEvent>> result = await scope.ServiceProvider
            .GetRequiredService<FundGoalBalanceEventQueryService>()
            .GetAsync(converter.ToDomain(new FundGoalBalanceEventsInAccountingPeriodRangeQueryParameterModel
            {
                Range = new AccountingPeriodRangeModel { Start = period.Id, End = period.Id },
                Sort = sort,
                Limit = limit,
            }));
        return converter.ToModel(result.Range!);
    }
}
