using Data;
using Domain.Actions;
using Domain.Funds;
using Microsoft.AspNetCore.Mvc;
using Rest.Models.Fund;

namespace Rest.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Funds
/// </summary>
[ApiController]
[Route("/funds")]
internal sealed class FundController(
    UnitOfWork unitOfWork,
    AddFundAction addFundAction,
    IFundRepository fundRepository,
    FundIdFactory fundIdFactory) : ControllerBase
{
    /// <summary>
    /// Retrieves all the Funds from the database
    /// </summary>
    /// <returns>A collection of all Funds</returns>
    [HttpGet("")]
    public IReadOnlyCollection<FundModel> GetAllFunds() =>
        fundRepository.FindAll().Select(fund => new FundModel(fund)).ToList();

    /// <summary>
    /// Retrieves the Fund that matches the provided ID
    /// </summary>
    /// <param name="fundId">Id of the Fund to retrieve</param>
    /// <returns>The Fund that matches the provided ID</returns>
    [HttpGet("{fundId}")]
    public IActionResult GetFund(Guid fundId)
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
    public async Task<IActionResult> CreateFundAsync(CreateFundModel createFundModel)
    {
        Fund newFund = addFundAction.Run(createFundModel.Name);
        fundRepository.Add(newFund);
        await unitOfWork.SaveChangesAsync();
        return Ok(new FundModel(newFund));
    }
}