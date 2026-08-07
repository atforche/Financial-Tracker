using Domain.Users;
using Domain.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Rest.Users;

/// <summary>
/// Converts user-management lifecycle errors to the API error contract.
/// </summary>
internal static class UserManagementErrorHelper
{
    /// <summary>
    /// Creates an API response for a user-management lifecycle failure.
    /// </summary>
    internal static IActionResult ToActionResult(string title, IEnumerable<UserManagementError> errors)
    {
        UserManagementError[] errorList = errors.ToArray();
        if (errorList.Any(error => error.Kind == UserManagementErrorKind.Forbidden))
        {
            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }
        if (errorList.Any(error => error.Kind == UserManagementErrorKind.NotFound))
        {
            return new NotFoundResult();
        }
        if (errorList.Any(error => error.Kind == UserManagementErrorKind.Conflict))
        {
            return new ObjectResult(new ProblemDetails
            {
                Title = title,
                Detail = string.Join(" ", errorList.Select(error => error.Message)),
                Status = StatusCodes.Status409Conflict,
            })
            {
                StatusCode = StatusCodes.Status409Conflict,
            };
        }

        var validationErrors = errorList
            .GroupBy(error => error.Path.Value, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Message).ToArray(),
                StringComparer.Ordinal);
        return new UnprocessableEntityObjectResult(new ValidationProblemDetails
        {
            Title = title,
            Errors = validationErrors,
            Status = StatusCodes.Status422UnprocessableEntity,
        });
    }

    /// <summary>
    /// Creates a field validation error for a missing or invalid role.
    /// </summary>
    internal static UserManagementError InvalidRole() => new(
        UserManagementErrorKind.Validation,
        new ValidationErrorPath("role"),
        "A valid user role is required.");
}