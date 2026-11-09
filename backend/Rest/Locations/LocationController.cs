using Data;
using Domain.AccountingPeriods.Queries;
using Domain.Locations;
using Domain.Locations.Queries;
using Domain.Validation;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Locations;
using Rest.AccountingPeriods;

namespace Rest.Locations;

/// <summary>
/// REST endpoints for Location management.
/// </summary>
[ApiController]
[Route("/locations")]
public sealed class LocationController(
    UnitOfWork unitOfWork,
    ILocationRepository locationRepository,
    LocationService locationService,
    LocationQueryService locationQueryService,
    LocationConverter locationConverter) : ControllerBase
{
    /// <summary>
    /// Gets Locations matching the provided query.
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(typeof(CollectionModel<LocationModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionModel<LocationModel>>> GetManyAsync(
        [FromQuery] LocationQueryParameterModel query,
        CancellationToken cancellationToken) =>
        Ok(locationConverter.ToModel(await locationQueryService.GetAsync(locationConverter.ToDomain(query), cancellationToken)));

    /// <summary>
    /// Retrieves the Locations' activity over a date range, most spent first by default.
    /// </summary>
    [HttpGet("date-range")]
    [ProducesResponseType(typeof(LocationsInRangeModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<LocationsInRangeModel>> GetDateRangeAsync(
        [FromQuery] LocationsInDateRangeQueryParameterModel query,
        CancellationToken cancellationToken) =>
        ValidateDates(query.Range, "Unable to retrieve Location range.") is ValidationProblemDetails problem
            ? UnprocessableEntity(problem)
            : Ok(locationConverter.ToModel(await locationQueryService.GetDateRangeAsync(
                query.Range.Start!.Value,
                query.Range.End!.Value,
                locationConverter.ToDomain(query.NameSearch, query.Sort, query),
                cancellationToken)));

    /// <summary>
    /// Retrieves the Locations' activity over an Accounting Period range, most spent first by default.
    /// </summary>
    [HttpGet("accounting-period-range")]
    [ProducesResponseType(typeof(LocationsInRangeModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<LocationsInRangeModel>> GetAccountingPeriodRangeAsync(
        [FromQuery] LocationsInAccountingPeriodRangeQueryParameterModel query,
        CancellationToken cancellationToken)
    {
        AccountingPeriodRangeQueryResult<LocationRange> result = await locationQueryService.GetAccountingPeriodRangeAsync(
            query.Range.Start,
            query.Range.End,
            locationConverter.ToDomain(query.NameSearch, query.Sort, query),
            cancellationToken);
        return result.Range == null
            ? UnprocessableEntity(AccountingPeriodRangeValidationProblem.Create(result.Failure, query.Range.Start, query.Range.End, "Unable to retrieve Location range."))
            : Ok(locationConverter.ToModel(result.Range));
    }

    /// <summary>
    /// Retrieves one Location's activity over a date range, by month, and how it was paid for.
    /// </summary>
    [HttpGet("{locationId:guid}/date-range")]
    [ProducesResponseType(typeof(LocationDetailRangeModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<LocationDetailRangeModel>> GetDetailDateRangeAsync(
        Guid locationId,
        [FromQuery] LocationDateRangeQueryParameterModel query,
        CancellationToken cancellationToken)
    {
        if (ValidateDates(query.Range, "Unable to retrieve Location range.") is ValidationProblemDetails problem)
        {
            return UnprocessableEntity(problem);
        }
        LocationDetailRange? range = await locationQueryService.GetDetailDateRangeAsync(
            locationId,
            query.Range.Start!.Value,
            query.Range.End!.Value,
            cancellationToken);
        return range == null ? NotFound() : Ok(locationConverter.ToModel(range));
    }

    /// <summary>
    /// Retrieves one Location's activity over an Accounting Period range, by period, and how it was paid for.
    /// </summary>
    [HttpGet("{locationId:guid}/accounting-period-range")]
    [ProducesResponseType(typeof(LocationDetailRangeModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<LocationDetailRangeModel>> GetDetailAccountingPeriodRangeAsync(
        Guid locationId,
        [FromQuery] LocationAccountingPeriodRangeQueryParameterModel query,
        CancellationToken cancellationToken)
    {
        AccountingPeriodRangeQueryResult<LocationDetailRange>? result = await locationQueryService.GetDetailAccountingPeriodRangeAsync(
            locationId,
            query.Range.Start,
            query.Range.End,
            cancellationToken);
        if (result == null)
        {
            return NotFound();
        }
        return result.Range == null
            ? UnprocessableEntity(AccountingPeriodRangeValidationProblem.Create(result.Failure, query.Range.Start, query.Range.End, "Unable to retrieve Location range."))
            : Ok(locationConverter.ToModel(result.Range));
    }

    /// <summary>
    /// Renames a Location.
    /// </summary>
    [HttpPost("{locationId:guid}")]
    [ProducesResponseType(typeof(LocationModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateAsync(Guid locationId, UpdateLocationModel model)
    {
        if (!locationRepository.TryGetById(locationId, out Location? location))
        {
            return MissingLocation("update", locationId);
        }
        if (!locationService.TryUpdate(
            location,
            new UpdateLocationRequest { Name = model.Name },
            out IEnumerable<ValidationError> validationErrors))
        {
            return ValidationProblem("Unable to update Location.", validationErrors);
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(locationConverter.ToModel(location));
    }

    /// <summary>
    /// Deletes an unused Location.
    /// </summary>
    [HttpDelete("{locationId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteAsync(Guid locationId)
    {
        if (!locationRepository.TryGetById(locationId, out Location? location))
        {
            return MissingLocation("delete", locationId);
        }
        if (!locationService.TryDelete(location, out IEnumerable<ValidationError> validationErrors))
        {
            return ValidationProblem("Unable to delete Location.", validationErrors);
        }
        await unitOfWork.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Consolidates a duplicate Location into a surviving Location.
    /// </summary>
    [HttpPost("{sourceLocationId:guid}/consolidate")]
    [ProducesResponseType(typeof(LocationModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ConsolidateAsync(Guid sourceLocationId, ConsolidateLocationModel model)
    {
        if (!locationRepository.TryGetById(sourceLocationId, out Location? source))
        {
            return MissingLocation("consolidate", sourceLocationId);
        }
        if (!locationRepository.TryGetById(model.TargetLocationId, out Location? target))
        {
            return MissingLocation("consolidate", model.TargetLocationId);
        }
        if (!locationService.TryConsolidate(source, target, out IEnumerable<ValidationError> validationErrors))
        {
            return ValidationProblem("Unable to consolidate Location.", validationErrors);
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(locationConverter.ToModel(target));
    }

    private UnprocessableEntityObjectResult MissingLocation(string action, Guid locationId) =>
        UnprocessableEntity(new ValidationProblemDetails
        {
            Title = $"Unable to {action} Location.",
            Errors = { { nameof(locationId), [$"Location with ID {locationId} was not found."] } },
            Status = StatusCodes.Status422UnprocessableEntity,
        });

    private UnprocessableEntityObjectResult ValidationProblem(string title, IEnumerable<ValidationError> validationErrors) =>
        UnprocessableEntity(new ValidationProblemDetails
        {
            Title = title,
            Errors = ValidationErrorHelper.GroupValidationErrors(validationErrors),
            Status = StatusCodes.Status422UnprocessableEntity,
        });

    /// <summary>
    /// Requires both ends of a date range, in order and within the maximum span.
    /// </summary>
    private static ValidationProblemDetails? ValidateDates(DateRangeModel range, string title) =>
        range.Start == null || range.End == null
            ? new ValidationProblemDetails(new Dictionary<string, string[]> { ["Range"] = ["Both a start date and an end date are required."] })
            {
                Title = title,
                Status = StatusCodes.Status422UnprocessableEntity,
            }
            : DateRangeValidationProblem.Validate(range.Start, range.End, title);
}
