using System.Diagnostics.CodeAnalysis;

namespace Tests.Validators;

/// <summary>
/// Base class of all comparers used to compare Entites and their states
/// </summary>
internal abstract class EntityComparerBase
{
    /// <summary>
    /// Performs comparison on the two entities if either is null
    /// </summary>
    /// <param name="first">First entity to compare</param>
    /// <param name="second">Second entity to compare</param>
    /// <param name="result">The comparison result if either entity is null</param>
    /// <returns>True if either entity was null, false otherwise</returns>
    protected static bool TryCompareNull([NotNullWhen(false)] object? first,
        [NotNullWhen(false)] object? second,
        [NotNullWhen(true)] out int? result)
    {
        if (first == null && second == null)
        {
            result = 0;
            return true;
        }
        if (first == null)
        {
            result = -1;
            return true;
        }
        if (second == null)
        {
            result = 1;
            return true;
        }
        result = null;
        return false;
    }
}