using Domain.AccountingPeriods.Queries;
using Domain.Overview;
using Microsoft.AspNetCore.Mvc;
using Models.Overview;
using Rest.AccountingPeriods;

namespace Rest.Overview;

/// <summary>
/// Controller class that exposes the Account and Fund balance summaries shown on the overview.
/// </summary>
[ApiController]
[Route("/overview")]
public sealed class OverviewController(
    OverviewQueryService overviewQueryService,
    OverviewConverter overviewConverter) : ControllerBase
{
    /// <summary>
    /// Retrieves Account and Fund balance summaries over a date range.
    /// </summary>
    [HttpGet("date-range")]
    [ProducesResponseType(typeof(OverviewDateRangeModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OverviewDateRangeModel>> GetDateRangeAsync(
        [FromQuery] OverviewDateRangeQueryParameterModel query,
        CancellationToken cancellationToken)
    {
        const string title = "Unable to retrieve overview.";
        if (query.Range.Start is not DateOnly start || query.Range.End is not DateOnly end)
        {
            return UnprocessableEntity(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["Range"] = ["A start and end date are required."],
            })
            {
                Title = title,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        if (DateRangeValidationProblem.Validate(start, end, title) is ValidationProblemDetails problem)
        {
            return UnprocessableEntity(problem);
        }
        return Ok(overviewConverter.ToModel(await overviewQueryService.GetDateRangeAsync(start, end, cancellationToken)));
    }

    /// <summary>
    /// Retrieves Account and Fund balance summaries over an Accounting Period range.
    /// </summary>
    [HttpGet("accounting-period-range")]
    [ProducesResponseType(typeof(OverviewAccountingPeriodRangeModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OverviewAccountingPeriodRangeModel>> GetAccountingPeriodRangeAsync(
        [FromQuery] OverviewAccountingPeriodRangeQueryParameterModel query,
        CancellationToken cancellationToken)
    {
        AccountingPeriodRangeQueryResult<OverviewAccountingPeriodRange> result = await overviewQueryService.GetAccountingPeriodRangeAsync(
            query.Range.Start,
            query.Range.End,
            cancellationToken);
        return result.Range == null
            ? UnprocessableEntity(AccountingPeriodRangeValidationProblem.Create(result.Failure, query.Range.Start, query.Range.End, "Unable to retrieve overview."))
            : Ok(overviewConverter.ToModel(result.Range));
    }
}
