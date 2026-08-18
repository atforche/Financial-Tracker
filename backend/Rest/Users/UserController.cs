using System.Data;
using Data;
using Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Users;
using Rest.Authentication;

namespace Rest.Users;

/// <summary>
/// Exposes current-user and administrator user-management operations.
/// </summary>
[ApiController]
[Route("/users")]
public sealed class UserController(
    UnitOfWork unitOfWork,
    IUserRepository userRepository,
    UserManagementService userManagementService,
    ICurrentApplicationUserAccessor currentUserAccessor) : ControllerBase
{
    /// <summary>
    /// Gets the current database-backed application user.
    /// </summary>
    [HttpGet("me")]
    [Authorize(Policy = UserAuthorizationPolicies.ActiveUser)]
    [ProducesResponseType(typeof(UserModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetCurrentUser()
    {
        User? user = currentUserAccessor.GetCurrentUser();
        return user == null ? new StatusCodeResult(StatusCodes.Status403Forbidden) : Ok(UserConverter.ToModel(user));
    }

    /// <summary>
    /// Lists all application users for an administrator.
    /// </summary>
    [HttpGet("")]
    [Authorize(Policy = UserAuthorizationPolicies.Administrator)]
    [ProducesResponseType(typeof(CollectionModel<UserModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<CollectionModel<UserModel>> GetMany() => Ok(UserConverter.ToModel(userRepository.GetAll()));

    /// <summary>
    /// Changes an application user's role.
    /// </summary>
    [HttpPost("{userId:guid}/role")]
    [Authorize(Policy = UserAuthorizationPolicies.Administrator)]
    [ProducesResponseType(typeof(UserModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ChangeRoleAsync(
        Guid userId,
        ChangeUserRoleModel? request,
        CancellationToken cancellationToken)
    {
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        if (!userRepository.TryGetById(userId, out User? target))
        {
            await transaction.RollbackAsync(cancellationToken);
            return NotFound();
        }
        if (!UserConverter.TryToDomain(request?.Role, out UserRole? newRole))
        {
            await transaction.RollbackAsync(cancellationToken);
            return UserManagementErrorHelper.ToActionResult(
                "Unable to change user role.",
                [UserManagementErrorHelper.InvalidRole()]);
        }

        User? actor = currentUserAccessor.GetCurrentUser();
        if (actor == null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }
        if (!userManagementService.TryChangeRole(
            actor,
            target,
            newRole.Value,
            DateTime.UtcNow,
            out IEnumerable<UserManagementError> errors))
        {
            await transaction.RollbackAsync(cancellationToken);
            return UserManagementErrorHelper.ToActionResult("Unable to change user role.", errors);
        }

        await unitOfWork.SaveChangesAsync();
        await transaction.CommitAsync(cancellationToken);
        return Ok(UserConverter.ToModel(target));
    }

    /// <summary>
    /// Disables an application user's access.
    /// </summary>
    [HttpPost("{userId:guid}/disable")]
    [Authorize(Policy = UserAuthorizationPolicies.Administrator)]
    [ProducesResponseType(typeof(UserModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DisableAsync(Guid userId, CancellationToken cancellationToken)
    {
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        if (!userRepository.TryGetById(userId, out User? target))
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
        if (!userManagementService.TryDisable(actor, target, DateTime.UtcNow, out IEnumerable<UserManagementError> errors))
        {
            await transaction.RollbackAsync(cancellationToken);
            return UserManagementErrorHelper.ToActionResult("Unable to disable user.", errors);
        }

        await unitOfWork.SaveChangesAsync();
        await transaction.CommitAsync(cancellationToken);
        return Ok(UserConverter.ToModel(target));
    }

    /// <summary>
    /// Enables an application user's access.
    /// </summary>
    [HttpPost("{userId:guid}/enable")]
    [Authorize(Policy = UserAuthorizationPolicies.Administrator)]
    [ProducesResponseType(typeof(UserModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> EnableAsync(Guid userId, CancellationToken cancellationToken)
    {
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        if (!userRepository.TryGetById(userId, out User? target))
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
        if (!userManagementService.TryEnable(actor, target, DateTime.UtcNow, out IEnumerable<UserManagementError> errors))
        {
            await transaction.RollbackAsync(cancellationToken);
            return UserManagementErrorHelper.ToActionResult("Unable to enable user.", errors);
        }

        await unitOfWork.SaveChangesAsync();
        await transaction.CommitAsync(cancellationToken);
        return Ok(UserConverter.ToModel(target));
    }
}
