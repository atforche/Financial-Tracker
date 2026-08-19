using System.Data;
using Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Rest.Authentication;

/// <summary>
/// Resolves authenticated provider identities into application users.
/// </summary>
[ApiController]
[Route("/authentication")]
public sealed class AuthenticationController(
    DatabaseContext databaseContext,
    Domain.Users.UserManagementService userManagementService) : ControllerBase
{
    /// <summary>
    /// Resolves the authenticated provider identity and accepts a matching invitation.
    /// </summary>
    [HttpPost("resolve-user")]
    [Authorize(Policy = UserAuthorizationPolicies.ProviderIdentity)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResolveUserAsync(CancellationToken cancellationToken)
    {
        if (!ProviderIdentityClaims.TryRead(User, out ProviderIdentity identity))
        {
            return Forbid();
        }

        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await databaseContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        if (!userManagementService.TryResolveIdentity(
            identity.Subject,
            identity.Email,
            identity.EmailVerified,
            identity.DisplayName,
            DateTime.UtcNow,
            out _,
            out _))
        {
            await transaction.RollbackAsync(cancellationToken);
            return Forbid();
        }

        _ = await databaseContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return NoContent();
    }
}
