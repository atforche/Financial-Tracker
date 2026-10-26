namespace Domain.Validation;

/// <summary>
/// Represents a validation failure for a property in a request.
/// </summary>
public sealed record ValidationError(ValidationErrorPath Path, string Message);
