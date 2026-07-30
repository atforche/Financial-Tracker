namespace Domain.Validation;

/// <summary>
/// Represents a validation failure for a property in a request.
/// </summary>
public sealed record ValidationError(ValidationErrorPath Path, string Message)
{
    /// <summary>
    /// Returns an error with the provided property prepended to its path.
    /// </summary>
    public ValidationError WithPrefix(string prefix) => this with { Path = Path.Prepend(prefix) };
}