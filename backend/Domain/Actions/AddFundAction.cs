using System.Diagnostics.CodeAnalysis;
using Domain.Funds;

namespace Domain.Actions;

/// <summary>
/// Action class that adds a Fund
/// </summary>
/// <param name="fundRepository">Fund Repository</param>
public class AddFundAction(IFundRepository fundRepository)
{
    /// <summary>
    /// Runs this action
    /// </summary>
    /// <param name="name">Name for this Fund</param>
    /// <returns>The Fund that was created</returns>
    public Fund Run(string name)
    {
        if (!IsValid(name, out Exception? exception))
        {
            throw exception;
        }
        var newFund = new Fund(name);
        return newFund;
    }

    /// <summary>
    /// Determines if this action is valid to run
    /// </summary>
    /// <param name="name">Name for the Fund</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if this action is valid to run, false otherwise</returns>
    private bool IsValid(string name, [NotNullWhen(false)] out Exception? exception)
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