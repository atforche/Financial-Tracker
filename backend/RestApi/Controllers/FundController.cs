using Data;
using Domain.Entities;
using Domain.Repositories;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;
using RestApi.Models.Fund;

namespace RestApi.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Funds
/// </summary>
[ApiController]
[Route("/funds")]
public class FundController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFundService _fundService;
    private readonly IFundRepository _fundRepository;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="unitOfWork">Unit of work to commit changes to the database</param>
    /// <param name="fundService">Service that constructs Funds</param>
    /// <param name="fundRepository">Repository of Funds</param>
    public FundController(IUnitOfWork unitOfWork, IFundService fundService, IFundRepository fundRepository)
    {
        _unitOfWork = unitOfWork;
        _fundService = fundService;
        _fundRepository = fundRepository;
    }

    /// <summary>
    /// Retrieves all the Funds from the database
    /// </summary>
    /// <returns>A collection of all Funds</returns>
    [HttpGet("")]
    public IReadOnlyCollection<FundModel> GetAllFunds() =>
        _fundRepository.FindAll().Select(FundModel.ConvertEntityToModel).ToList();

    /// <summary>
    /// Retrieves the Fund that mactches the provided ID
    /// </summary>
    /// <param name="fundId">Id of the Fund to retrieve</param>
    /// <returns>The Fund that matches the provided ID</returns>
    [HttpGet("{fundId}")]
    public IActionResult GetFund(Guid fundId)
    {
        Fund? fund = _fundRepository.FindOrNull(fundId);
        return fund != null ? Ok(FundModel.ConvertEntityToModel(fund)) : NotFound();
    }

    /// <summary>
    /// Creates a new Fund with the provided properties
    /// </summary>
    /// <param name="createFundModel">Request to create a Fund</param>
    /// <returns>The created Fund</returns>
    [HttpPost("")]
    public async Task<IActionResult> CreateFundAsync(CreateFundModel createFundModel)
    {
        var createFundRequest = new CreateFundRequest
        {
            Name = createFundModel.Name,
        };
        Fund newFund = _fundService.CreateNewFund(createFundRequest);
        _fundRepository.Add(newFund);
        await _unitOfWork.SaveChangesAsync();
        return Ok(FundModel.ConvertEntityToModel(newFund));
    }
}