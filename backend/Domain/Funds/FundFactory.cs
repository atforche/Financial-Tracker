using System.Diagnostics.CodeAnalysis;

namespace Domain.Funds;

/// <summary>
/// Factory for building a <see cref="Fund"/>
/// </summary>
public class FundFactory(IFundRepository fundRepository)
{
    /// <summary>
    /// Create a new Fund
    /// </summary>
    /// <param name="name">Name for the Fund</param>
    /// <returns>The newly created Fund</returns>
    public Fund Create(string name)
    {
        if (!ValidateName(name, out Exception? exception))
        {
            throw exception;
        }
        return new Fund(name);
    }

    /// <summary>
    /// Validates the name for this Fund
    /// </summary>
    /// <param name="name">Name for the Fund</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if the name for this Fund is valid, false otherwise</returns>
    private bool ValidateName(string name, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (string.IsNullOrEmpty(name))
        {
            exception = new InvalidOperationException();
        }
        if (fundRepository.FindByNameOrNull(name) != null)
        {
            exception ??= new InvalidOperationException();
        }
        return exception == null;
    }
}