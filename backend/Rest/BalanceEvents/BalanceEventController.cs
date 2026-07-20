using Data.BalanceEvents;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.BalanceEvents;

namespace Rest.BalanceEvents;

/// <summary>
/// Controller class that exposes endpoints related to Balance Events.
/// </summary>
[ApiController]
[Route("/balance-events")]
public sealed class BalanceEventController(BalanceEventQueryService balanceEventQueryService) : ControllerBase
{
    /// <summary>
    /// Retrieves Account Balance Events in a date range.
    /// </summary>
    [HttpGet("accounts/date-range")]
    [ProducesResponseType(typeof(CollectionModel<AccountBalanceEventModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionModel<AccountBalanceEventModel>>> GetAccountEventsAsync(
        [FromQuery] AccountBalanceEventsInDateRangeQueryParameterModel query,
        CancellationToken cancellationToken) =>
        Ok(await balanceEventQueryService.GetAccountEventsAsync(query, cancellationToken));

    /// <summary>
    /// Retrieves Account Balance Events in an Accounting Period range.
    /// </summary>
    [HttpGet("accounts/accounting-period-range")]
    [ProducesResponseType(typeof(CollectionModel<AccountBalanceEventModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CollectionModel<AccountBalanceEventModel>>> GetAccountEventsAsync(
        [FromQuery] AccountBalanceEventsInAccountingPeriodRangeQueryParameterModel query,
        CancellationToken cancellationToken)
    {
        CollectionModel<AccountBalanceEventModel>? model = await balanceEventQueryService.GetAccountEventsAsync(query, cancellationToken);
        return model == null ? InvalidAccountingPeriodRange() : Ok(model);
    }

    /// <summary>
    /// Retrieves Fund Balance Events in a date range.
    /// </summary>
    [HttpGet("funds/date-range")]
    [ProducesResponseType(typeof(CollectionModel<FundBalanceEventModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionModel<FundBalanceEventModel>>> GetFundEventsAsync(
        [FromQuery] FundBalanceEventsInDateRangeQueryParameterModel query,
        CancellationToken cancellationToken) =>
        Ok(await balanceEventQueryService.GetFundEventsAsync(query, cancellationToken));

    /// <summary>
    /// Retrieves Fund Balance Events in an Accounting Period range.
    /// </summary>
    [HttpGet("funds/accounting-period-range")]
    [ProducesResponseType(typeof(CollectionModel<FundBalanceEventModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CollectionModel<FundBalanceEventModel>>> GetFundEventsAsync(
        [FromQuery] FundBalanceEventsInAccountingPeriodRangeQueryParameterModel query,
        CancellationToken cancellationToken)
    {
        CollectionModel<FundBalanceEventModel>? model = await balanceEventQueryService.GetFundEventsAsync(query, cancellationToken);
        return model == null ? InvalidAccountingPeriodRange() : Ok(model);
    }

    /// <summary>
    /// Retrieves Fund Plan balance events in a date range.
    /// </summary>
    [HttpGet("fund-plans/date-range")]
    public async Task<ActionResult<CollectionModel<FundPlanBalanceEventModel>>> GetFundPlanEventsAsync(
        [FromQuery] FundPlanBalanceEventsInDateRangeQueryParameterModel query,
        CancellationToken cancellationToken) =>
        Ok(await balanceEventQueryService.GetFundPlanEventsAsync(query, cancellationToken));

    /// <summary>
    /// Retrieves Fund Plan balance events in an Accounting Period range.
    /// </summary>
    [HttpGet("fund-plans/accounting-period-range")]
    public async Task<ActionResult<CollectionModel<FundPlanBalanceEventModel>>> GetFundPlanEventsAsync(
        [FromQuery] FundPlanBalanceEventsInAccountingPeriodRangeQueryParameterModel query,
        CancellationToken cancellationToken)
    {
        CollectionModel<FundPlanBalanceEventModel>? model = await balanceEventQueryService.GetFundPlanEventsAsync(query, cancellationToken);
        return model == null ? InvalidAccountingPeriodRange() : Ok(model);
    }

    /// <summary>
    /// Creates a response for an invalid Accounting Period range.
    /// </summary>
    private UnprocessableEntityObjectResult InvalidAccountingPeriodRange() => UnprocessableEntity(new ValidationProblemDetails
    {
        Title = "Unable to resolve Accounting Period range.",
        Status = StatusCodes.Status422UnprocessableEntity,
    });
}