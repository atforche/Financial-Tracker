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
    /// Retrieves Goal Balance Events in a date range.
    /// </summary>
    [HttpGet("goals/date-range")]
    [ProducesResponseType(typeof(CollectionModel<GoalBalanceEventModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionModel<GoalBalanceEventModel>>> GetGoalEventsAsync(
        [FromQuery] GoalBalanceEventsInDateRangeQueryParameterModel query,
        CancellationToken cancellationToken) =>
        Ok(await balanceEventQueryService.GetGoalEventsAsync(query, cancellationToken));

    /// <summary>
    /// Retrieves Goal Balance Events in an Accounting Period range.
    /// </summary>
    [HttpGet("goals/accounting-period-range")]
    [ProducesResponseType(typeof(CollectionModel<GoalBalanceEventModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CollectionModel<GoalBalanceEventModel>>> GetGoalEventsAsync(
        [FromQuery] GoalBalanceEventsInAccountingPeriodRangeQueryParameterModel query,
        CancellationToken cancellationToken)
    {
        CollectionModel<GoalBalanceEventModel>? model = await balanceEventQueryService.GetGoalEventsAsync(query, cancellationToken);
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