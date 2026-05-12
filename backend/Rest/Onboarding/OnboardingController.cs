using Data;
using Domain.Exceptions;
using Domain.Onboarding;
using Microsoft.AspNetCore.Mvc;
using Models.Onboarding;

namespace Rest.Onboarding;

/// <summary>
/// Controller class that exposes endpoints related to onboarding.
/// </summary>
[ApiController]
[Route("/onboarding")]
public sealed class OnboardingController(UnitOfWork unitOfWork, OnboardingService onboardingService) : ControllerBase
{
    /// <summary>
    /// Retrieves whether onboarding can be completed.
    /// </summary>
    [HttpGet("eligibility")]
    [ProducesResponseType(typeof(OnboardingEligibilityModel), StatusCodes.Status200OK)]
    public IActionResult GetEligibility()
    {
        bool isEligible = onboardingService.IsEligible(out IEnumerable<Exception> exceptions);
        return Ok(new OnboardingEligibilityModel
        {
            IsEligible = isEligible,
            Errors = exceptions.Select(exception => exception.Message).ToArray(),
        });
    }

    /// <summary>
    /// Completes the onboarding flow.
    /// </summary>
    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> OnboardAsync(OnboardingModel onboardingModel)
    {
        if (!OnboardingRequestConverter.TryToDomain(onboardingModel, out OnboardingRequest? onboardingRequest, out Dictionary<string, string[]> errors))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to complete onboarding.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        if (!onboardingService.Onboard(onboardingRequest, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to complete onboarding.",
                Errors = GetOnboardingErrors(exceptions),
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Maps onboarding exceptions to validation errors.
    /// </summary>
    private static Dictionary<string, string[]> GetOnboardingErrors(IEnumerable<Exception> exceptions) =>
        exceptions.GroupBy(exception => exception switch
        {
            InvalidAccountException => nameof(OnboardingModel.Accounts),
            InvalidFundException => nameof(OnboardingModel.Funds),
            _ => string.Empty,
        }).ToDictionary(grouping => grouping.Key, grouping => grouping.Select(exception => exception.Message).ToArray());
}