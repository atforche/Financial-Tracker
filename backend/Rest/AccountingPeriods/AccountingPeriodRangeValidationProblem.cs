using Domain.AccountingPeriods.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Rest.AccountingPeriods;

/// <summary>
/// Creates consistent API validation details for invalid Accounting Period ranges.
/// </summary>
public static class AccountingPeriodRangeValidationProblem
{
    /// <summary>
    /// Creates validation details for an Accounting Period range failure.
    /// </summary>
    public static ValidationProblemDetails Create(
        AccountingPeriodRangeQueryFailure failure,
        Guid startId,
        Guid endId,
        string title)
    {
        Dictionary<string, string[]> errors = [];
        if (failure.HasFlag(AccountingPeriodRangeQueryFailure.StartNotFound))
        {
            errors["Range.Start"] = [$"Accounting Period with ID {startId} not found."];
        }
        if (failure.HasFlag(AccountingPeriodRangeQueryFailure.EndNotFound))
        {
            errors["Range.End"] = [$"Accounting Period with ID {endId} not found."];
        }
        if (failure.HasFlag(AccountingPeriodRangeQueryFailure.Reversed))
        {
            errors["Range"] = ["The start Accounting Period must not occur after the end Accounting Period."];
        }
        if (failure.HasFlag(AccountingPeriodRangeQueryFailure.NotContiguous))
        {
            errors["Range"] = ["The Accounting Period range must be contiguous."];
        }
        return new ValidationProblemDetails(errors)
        {
            Title = title,
            Status = StatusCodes.Status422UnprocessableEntity,
        };
    }
}
