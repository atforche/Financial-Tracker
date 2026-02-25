using System.Diagnostics.CodeAnalysis;
using Domain.Funds.Exceptions;
using Domain.Transactions;

namespace Domain.Funds;

/// <summary>
/// Service for managing Funds
/// </summary>
public class FundService(IFundRepository fundRepository, ITransactionRepository transactionRepository)
{
    /// <summary>
    /// Attempts to create a new Fund
    /// </summary>
    /// <param name="name">Name for the Fund</param>
    /// <param name="description">Description for the Fund</param>
    /// <param name="fund">The created Fund, or null if creation failed</param>
    /// <param name="exceptions">List of exceptions encountered during creation</param>
    /// <returns>True if the Fund was created successfully, false otherwise</returns>
    public bool TryCreate(string name, string description, [NotNullWhen(true)] out Fund? fund, out IEnumerable<Exception> exceptions)
    {
        fund = null;
        exceptions = [];

        if (!ValidateName(name, null, out IEnumerable<Exception> nameExceptions))
        {
            exceptions = exceptions.Concat(nameExceptions);
        }
        if (exceptions.Any())
        {
            return false;
        }
        fund = new Fund(name, description);
        return true;
    }

    /// <summary>
    /// Attempts to update an existing Fund
    /// </summary>
    /// <param name="fund">Fund to be updated</param>
    /// <param name="name">New name for the Fund</param>
    /// <param name="description">New description for the Fund</param>
    /// <param name="exceptions">List of exceptions encountered during update</param>
    /// <returns>True if the Fund was updated successfully, false otherwise</returns>
    public bool TryUpdate(Fund fund, string name, string description, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (!ValidateName(name, fund, out IEnumerable<Exception> nameExceptions))
        {
            exceptions = exceptions.Concat(nameExceptions);
        }
        if (exceptions.Any())
        {
            return false;
        }
        fund.Name = name;
        fund.Description = description;
        return true;
    }

    /// <summary>
    /// Attempts to delete an existing Fund
    /// </summary>
    /// <param name="fund">Fund to be deleted</param>
    /// <param name="exceptions">List of exceptions encountered during deletion</param>
    /// <returns>True if the Fund was deleted successfully, false otherwise</returns>
    public bool TryDelete(Fund fund, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (transactionRepository.DoAnyTransactionsExistForFund(fund.Id))
        {
            exceptions = [new UnableToDeleteException("Cannot delete a Fund that has Transactions.")];
            return false;
        }
        fundRepository.Delete(fund);
        return true;
    }

    /// <summary>
    /// Validates the name for a Fund
    /// </summary>
    /// <param name="name">Name for the Fund</param>
    /// <param name="existingFund">The existing Fund being updated, if any</param>
    /// <param name="exceptions">Exceptions encountered during validation</param>
    /// <returns>True if this name is valid for a Fund, false otherwise</returns>
    private bool ValidateName(string name, Fund? existingFund, out IEnumerable<Exception> exceptions)
    {
        exceptions = [];

        if (string.IsNullOrEmpty(name))
        {
            exceptions = exceptions.Append(new InvalidNameException("Fund name cannot be empty"));
        }
        if (fundRepository.TryGetByName(name, out Fund? existingFundWithName) && existingFundWithName != existingFund)
        {
            exceptions = exceptions.Append(new InvalidNameException("Fund name must be unique"));
        }
        return !exceptions.Any();
    }
}