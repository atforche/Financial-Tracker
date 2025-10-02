using System.Diagnostics.CodeAnalysis;
using Domain.FundConversions;
using Domain.Transactions;

namespace Domain.Funds;

/// <summary>
/// Service for managing Funds
/// </summary>
public class FundService(
    IFundRepository fundRepository,
    IFundConversionRepository fundConversionRepository,
    ITransactionRepository transactionRepository)
{
    /// <summary>
    /// Create a new Fund
    /// </summary>
    /// <param name="name">Name for the Fund</param>
    /// <param name="description">Description for the Fund</param>
    /// <returns>The newly created Fund</returns>
    public Fund Create(string name, string description)
    {
        if (!ValidateName(name, null, out Exception? exception))
        {
            throw exception;
        }
        return new Fund(name, description);
    }

    /// <summary>
    /// Updates an existing Fund
    /// </summary>
    /// <param name="fund">Fund to be updated</param>
    /// <param name="name">New name for the Fund</param>
    /// <param name="description">New description for the Fund</param>
    /// <returns>The updated Fund</returns>
    public Fund Update(Fund fund, string name, string description)
    {
        if (!ValidateName(name, fund, out Exception? exception))
        {
            throw exception;
        }
        fund.Name = name;
        fund.Description = description;
        return fund;
    }

    /// <summary>
    /// Deletes an existing Fund
    /// </summary>
    /// <param name="fund">Fund to be deleted</param>
    public void Delete(Fund fund)
    {
        if (!ValidateDelete(fund, out Exception? exception))
        {
            throw exception;
        }
        fundRepository.Delete(fund);
    }

    /// <summary>
    /// Validates the name for this Fund
    /// </summary>
    /// <param name="name">Name for the Fund</param>
    /// <param name="existingFund">The existing fund being updated, if any</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if the name for this Fund is valid, false otherwise</returns>
    private bool ValidateName(string name, Fund? existingFund, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (string.IsNullOrEmpty(name))
        {
            exception = new InvalidOperationException();
        }
        Fund? existingFundWithName = fundRepository.FindByNameOrNull(name);
        if (existingFundWithName != null && existingFundWithName != existingFund)
        {
            exception ??= new InvalidOperationException();
        }
        return exception == null;
    }

    /// <summary>
    /// Validates that the Fund can be deleted
    /// </summary>
    /// <param name="fund">Fund to be deleted</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if the Fund can be deleted, false otherwise</returns>
    private bool ValidateDelete(Fund fund, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;
        if (transactionRepository.DoesTransactionWithFundExist(fund))
        {
            exception = new InvalidOperationException();
        }
        if (fundConversionRepository.DoesFundConversionWithFundExist(fund))
        {
            exception ??= new InvalidOperationException();
        }
        return exception == null;
    }
}