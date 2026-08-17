using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Domain.Validation;

namespace Domain.Locations;

/// <summary>
/// Manages Location identity and lifecycle operations.
/// </summary>
public sealed partial class LocationService(ILocationRepository locationRepository)
{
    /// <summary>
    /// Normalizes a Location name for identity comparison.
    /// </summary>
    public static string NormalizeName(string name) => WhitespaceRegex().Replace(name.Trim(), " ").ToUpperInvariant();

    /// <summary>
    /// Attempts to create a Location.
    /// </summary>
    public bool TryCreate(
        CreateLocationRequest request,
        [NotNullWhen(true)] out Location? location,
        out IEnumerable<ValidationError> errors)
    {
        location = null;
        string displayName = WhitespaceRegex().Replace(request.Name.Trim(), " ");
        string normalizedName = NormalizeName(request.Name);
        errors = ValidateName(displayName, normalizedName, null);
        if (errors.Any())
        {
            return false;
        }
        location = new Location(displayName, normalizedName);
        locationRepository.Add(location);
        return true;
    }

    /// <summary>
    /// Attempts to rename a Location.
    /// </summary>
    public bool TryUpdate(Location location, UpdateLocationRequest request, out IEnumerable<ValidationError> errors)
    {
        string displayName = WhitespaceRegex().Replace(request.Name.Trim(), " ");
        string normalizedName = NormalizeName(request.Name);
        errors = ValidateName(displayName, normalizedName, location);
        if (errors.Any())
        {
            return false;
        }
        location.Name = displayName;
        location.NormalizedName = normalizedName;
        return true;
    }

    /// <summary>
    /// Attempts to delete an unused Location.
    /// </summary>
    public bool TryDelete(Location location, out IEnumerable<ValidationError> errors)
    {
        errors = locationRepository.IsReferenced(location.Id)
            ? [new ValidationError(new ValidationErrorPath(nameof(Location.Id)), "Locations used by Transactions cannot be deleted")]
            : [];
        if (errors.Any())
        {
            return false;
        }
        locationRepository.Delete(location);
        return true;
    }

    /// <summary>
    /// Attempts to consolidate a duplicate Location into a surviving Location.
    /// </summary>
    public bool TryConsolidate(Location source, Location target, out IEnumerable<ValidationError> errors)
    {
        errors = source == target
            ? [new ValidationError(new ValidationErrorPath(nameof(Location.Id)), "A Location cannot be consolidated into itself")]
            : [];
        if (errors.Any())
        {
            return false;
        }
        locationRepository.Consolidate(source, target);
        return true;
    }

    private IEnumerable<ValidationError> ValidateName(string displayName, string normalizedName, Location? existingLocation)
    {
        ValidationErrorPath path = new(nameof(CreateLocationRequest.Name));
        if (displayName.Length == 0)
        {
            return [new ValidationError(path, "Location name cannot be empty")];
        }
        return locationRepository.TryGetByNormalizedName(normalizedName, out Location? matchingLocation)
            && matchingLocation != existingLocation
            ? [new ValidationError(path, "Location name must be unique")]
            : [];
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}