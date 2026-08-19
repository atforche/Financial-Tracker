using Data;
using Domain.Locations;
using Domain.Locations.Queries;
using Domain.Validation;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Locations;

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
    /// Gets a Location by ID.
    /// </summary>
    [HttpGet("{locationId:guid}")]
    [ProducesResponseType(typeof(LocationModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<LocationModel> Get(Guid locationId) =>
        locationRepository.TryGetById(locationId, out Location? location)
            ? Ok(locationConverter.ToModel(location))
            : NotFound();

    /// <summary>
    /// Creates a Location.
    /// </summary>
    [HttpPost("")]
    [ProducesResponseType(typeof(LocationModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync(CreateLocationModel model)
    {
        if (!locationService.TryCreate(
            new CreateLocationRequest { Name = model.Name },
            out Location? location,
            out IEnumerable<ValidationError> validationErrors))
        {
            return ValidationProblem("Unable to create Location.", validationErrors);
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(locationConverter.ToModel(location));
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
}
