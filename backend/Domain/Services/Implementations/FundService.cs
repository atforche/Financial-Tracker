using Domain.Entities;
using Domain.Repositories;

namespace Domain.Services.Implementations;

/// <inheritdoc/>
public class FundService : IFundService
{
    private readonly IFundRepository _fundRepository;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="fundRepository">Repository of Funds</param>
    public FundService(IFundRepository fundRepository)
    {
        _fundRepository = fundRepository;
    }

    /// <inheritdoc/>
    public Fund CreateNewFund(CreateFundRequest createFundRequest)
    {
        ValidateNewFundName(createFundRequest.Name);
        return new Fund(createFundRequest.Name);
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