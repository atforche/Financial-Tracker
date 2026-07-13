namespace Domain.Validation;

/// <summary>
/// Represents an immutable path to a property associated with a validation error.
/// </summary>
public sealed record ValidationErrorPath
{
    /// <summary>
    /// Value of the validation error path.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Represents an entity-level validation error.
    /// </summary>
    public static readonly ValidationErrorPath Empty = new(string.Empty);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public ValidationErrorPath(string value, int? index = null)
    {
        if (value.Length == 0 && index.HasValue)
        {
            throw new ArgumentException("An empty validation path cannot identify a collection item.", nameof(value));
        }
        if (value.Length > 0)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
        }
        Value = index.HasValue ? $"{value}[{index.Value}]" : value;
    }

    /// <summary>
    /// Returns a path with the provided property prepended.
    /// </summary>
    public ValidationErrorPath Prepend(string prefix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        return string.IsNullOrEmpty(Value)
            ? new ValidationErrorPath(prefix)
            : new ValidationErrorPath($"{prefix}.{Value}");
    }

    /// <summary>
    /// Returns a path with the provided collection item prepended.
    /// </summary>
    public ValidationErrorPath PrependWithIndex(string collection, int index)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(collection);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        return Prepend($"{collection}[{index}]");
    }

    /// <summary>
    /// Returns a path with the provided property appended.
    /// </summary>
    public ValidationErrorPath Append(string suffix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(suffix);
        return string.IsNullOrEmpty(Value)
            ? new ValidationErrorPath(suffix)
            : new ValidationErrorPath($"{Value}.{suffix}");
    }

    /// <summary>
    /// Returns a path with the provided collection item appended.
    /// </summary>
    public ValidationErrorPath AppendWithIndex(string collection, int index)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(collection);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        return Append($"{collection}[{index}]");
    }

    /// <inheritdoc/>
    public override string ToString() => Value;
}
