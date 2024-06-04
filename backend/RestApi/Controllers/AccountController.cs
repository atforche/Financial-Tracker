using Microsoft.AspNetCore.Mvc;

namespace RestApi.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Accounts.
/// </summary>
[ApiController]
[Route("/accounts")]
public class AccountController : ControllerBase
{
    /// <summary>
    /// Test endpoint to ensure API is working
    /// </summary>
    [HttpGet("GetTest")]
    public string TestEndpoint()
    {
        return "This is a test";
    }
}