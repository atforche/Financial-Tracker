using Domain;
using Microsoft.AspNetCore.Mvc;

namespace Rest;

/// <summary>
/// Builds validation problems for date ranges that are summarized into balance series.
/// </summary>
internal static class DateRangeValidationProblem
{
    /// <summary>
    /// Creates validation details when a date range is reversed or too long to summarize, or null when it is valid.
    /// Ranges missing either date are left to the caller.
    /// </summary>
    public static ValidationProblemDetails? Validate(DateOnly? start, DateOnly? end, string title)
    {
        if (start is not DateOnly startDate || end is not DateOnly endDate)
        {
            return null;
        }
        string? error = startDate > endDate
            ? "The start date must not occur after the end date."
            : !BalanceSeries.IsWithinMaximumRange(startDate, endDate)
                ? $"The date range must not span more than {BalanceSeries.MaximumRangeYears} years."
                : null;
        return error == null
            ? null
            : new ValidationProblemDetails(new Dictionary<string, string[]> { ["Range"] = [error] })
            {
                Title = title,
                Status = StatusCodes.Status422UnprocessableEntity,
            };
    }
}
