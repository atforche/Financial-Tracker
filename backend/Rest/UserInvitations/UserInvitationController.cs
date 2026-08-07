using System.Data;
using Data;
using Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.UserInvitations;
using Rest.Authentication;
using Rest.Users;

namespace Rest.UserInvitations;

/// <summary>
/// Exposes administrator operations for user invitations.
/// </summary>
[ApiController]
[Route("/user-invitations")]
public sealed class UserInvitationController(
    UnitOfWork unitOfWork,
    IUserInvitationRepository invitationRepository,
    UserManagementService userManagementService,
    ICurrentApplicationUserAccessor currentUserAccessor) : ControllerBase
{
    /// <summary>
    /// Lists all invitations, including completed invitation history.
    /// </summary>
    [HttpGet("")]
    [Authorize(Policy = UserAuthorizationPolicies.Administrator)]
    [ProducesResponseType(typeof(CollectionModel<UserInvitationModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<CollectionModel<UserInvitationModel>> GetMany() =>
        Ok(UserInvitationConverter.ToModel(invitationRepository.GetAll()));

    /// <summary>
    /// Creates a pending user invitation.
    /// </summary>
    [HttpPost("")]
    [Authorize(Policy = UserAuthorizationPolicies.Administrator)]
    [ProducesResponseType(typeof(UserInvitationModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync(
        CreateUserInvitationModel? request,
        CancellationToken cancellationToken)
    {
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        User? actor = currentUserAccessor.GetCurrentUser();
        if (actor == null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }
        UserRole role = UserConverter.TryToDomain(request?.Role, out UserRole? domainRole)
            ? domainRole.Value
            : (UserRole)(-1);
        if (!userManagementService.TryCreateInvitation(
            actor,
            request?.Email,
            role,
            request?.ExpiresAt,
            DateTime.UtcNow,
            out UserInvitation? invitation,
            out IEnumerable<UserManagementError> errors))
        {
            await transaction.RollbackAsync(cancellationToken);
            return UserManagementErrorHelper.ToActionResult("Unable to create invitation.", errors);
        }

        await unitOfWork.SaveChangesAsync();
        await transaction.CommitAsync(cancellationToken);
        return Ok(UserInvitationConverter.ToModel(invitation!));
    }

    /// <summary>
    /// Revokes a pending user invitation.
    /// </summary>
    [HttpDelete("{invitationId:guid}")]
    [Authorize(Policy = UserAuthorizationPolicies.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RevokeAsync(Guid invitationId, CancellationToken cancellationToken)
    {
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        if (!invitationRepository.TryGetById(invitationId, out UserInvitation? invitation))
        {
            await transaction.RollbackAsync(cancellationToken);
            return NotFound();
        }
        User? actor = currentUserAccessor.GetCurrentUser();
        if (actor == null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }
        if (!userManagementService.TryRevokeInvitation(
            actor,
            invitation,
            DateTime.UtcNow,
            out IEnumerable<UserManagementError> errors))
        {
            await transaction.RollbackAsync(cancellationToken);
            return UserManagementErrorHelper.ToActionResult("Unable to revoke invitation.", errors);
        }

        await unitOfWork.SaveChangesAsync();
        await transaction.CommitAsync(cancellationToken);
        return NoContent();
    }
}