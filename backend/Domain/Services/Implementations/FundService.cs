using Domain.Aggregates.Funds;

namespace Domain.Services.Implementations;

/// <inheritdoc/>
public class FundService(IFundRepository fundRepository) : IFundService
{
    private readonly IFundRepository _fundRepository = fundRepository;

    /// <inheritdoc/>
    public Fund CreateNewFund(string name)
    {
        ValidateNewFundName(name);
        return new Fund(name);
    }

    /// <summary>
    /// Validates that the new name for a Fund is unique among the existing Funds
    /// </summary>
    /// <param name="newName">New name to be given to a Fund</param>
    private void ValidateNewFundName(string newName)
    {
        if (_fundRepository.FindByNameOrNull(newName) != null)
        {
            throw new InvalidOperationException();
        }
    }
}