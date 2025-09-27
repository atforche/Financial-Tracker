using Domain.Funds;
using Tests.Mocks;

namespace Tests.Builders;

/// <summary>
/// Builder class that constructs a Fund
/// </summary>
public sealed class FundBuilder(
    FundService fundService,
    IFundRepository fundRepository,
    TestUnitOfWork testUnitOfWork)
{
    private string _name = Guid.NewGuid().ToString();

    /// <summary>
    /// Builds the specified Fund
    /// </summary>
    /// <returns>The newly constructed Fund</returns>
    public Fund Build()
    {
        Fund fund = fundService.Create(_name, "");
        fundRepository.Add(fund);
        testUnitOfWork.SaveChanges();
        return fund;
    }

    /// <summary>
    /// Sets the Name for this Fund Builder
    /// </summary>
    public FundBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
}