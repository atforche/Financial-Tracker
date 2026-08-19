using Domain.Validation;

namespace Rest;

/// <summary>
/// Helper class for validation errors
/// </summary>
internal static class ValidationErrorHelper
{
    /// <summary>
    /// Groups validation errors together by path
    /// </summary>
    internal static Dictionary<string, string[]> GroupValidationErrors(
        IEnumerable<ValidationError> validationErrors,
        Func<string, string>? resolvePath = null) =>
        validationErrors
            .GroupBy(error => resolvePath?.Invoke(error.Path.Value) ?? error.Path.Value)
            .ToDictionary(group => group.Key, group => group.Select(error => error.Message).ToArray());
}
