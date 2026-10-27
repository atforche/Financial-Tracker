using Domain.Overview;
using Microsoft.AspNetCore.Mvc;
using Models.Overview;

namespace Rest.Overview;

/// <summary>
/// Controller class that exposes everything shown on the overview.
/// </summary>
[ApiController]
[Route("/overview")]
public sealed class OverviewController(
    OverviewQueryService overviewQueryService,
    OverviewConverter overviewConverter) : ControllerBase
{
    /// <summary>
    /// Retrieves the balances, most recent open Accounting Period, and items needing attention as of a date.
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(typeof(OverviewModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<OverviewModel>> GetAsync(
        [FromQuery] OverviewQueryParameterModel query,
        CancellationToken cancellationToken) =>
        Ok(overviewConverter.ToModel(await overviewQueryService.GetAsync(
            query.AsOf,
            OverviewConverter.ToDomain(query.History),
            cancellationToken)));
}
