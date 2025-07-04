using Data;
using Domain.Funds;
using Microsoft.AspNetCore.Mvc;
using Rest.Models.Funds;

namespace Rest.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Funds
/// </summary>
[ApiController]
[Route("/funds")]
public sealed class FundController(
    UnitOfWork unitOfWork,
    IFundRepository fundRepository,
    FundFactory fundFactory,
    FundIdFactory fundIdFactory) : ControllerBase
{
    /// <summary>
    /// Retrieves all the Funds from the database
    /// </summary>
    /// <returns>A collection of all Funds</returns>
    [HttpGet("")]
    public IReadOnlyCollection<FundModel> GetAll() =>
        fundRepository.FindAll().Select(fund => new FundModel(fund)).ToList();

    /// <summary>
    /// Retrieves the Fund that matches the provided ID
    /// </summary>
    /// <param name="fundId">Id of the Fund to retrieve</param>
    /// <returns>The Fund that matches the provided ID</returns>
    [HttpGet("{fundId}")]
    public IActionResult Get(Guid fundId)
    {
        FundId id = fundIdFactory.Create(fundId);
        return Ok(new FundModel(fundRepository.FindById(id)));
    }

    /// <summary>
    /// Creates a new Fund with the provided properties
    /// </summary>
    /// <param name="createFundModel">Request to create a Fund</param>
    /// <returns>The created Fund</returns>
    [HttpPost("")]
    public async Task<IActionResult> CreateAsync(CreateFundModel createFundModel)
    {
        Fund newFund = fundFactory.Create(createFundModel.Name);
        fundRepository.Add(newFund);
        await unitOfWork.SaveChangesAsync();
        return Ok(new FundModel(newFund));
    }
}