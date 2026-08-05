using Domain.Users;
using Microsoft.AspNetCore.Authorization;

namespace Rest.Authentication;

/// <summary>
/// Requires a valid provider subject, verified email, and parseable email claim.
/// </summary>
internal sealed class ProviderIdentityAuthorizationHandler : AuthorizationHandler<ProviderIdentityRequirement>
{
    /// <inheritdoc/>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProviderIdentityRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated == true
            && ProviderIdentityClaims.TryRead(context.User, out _))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}

/// <summary>
/// Requires a provider identity that has an active application user record.
/// </summary>
internal sealed class ActiveUserAuthorizationHandler(ICurrentApplicationUserAccessor currentUserAccessor)
    : AuthorizationHandler<ActiveUserRequirement>
{
    /// <inheritdoc/>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ActiveUserRequirement requirement)
    {
        User? user = currentUserAccessor.GetCurrentUser();
        if (context.User.Identity?.IsAuthenticated == true && user?.Status == UserStatus.Active)
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}

/// <summary>
/// Requires an active application user with a write-capable role.
/// </summary>
internal sealed class WriteCapableUserAuthorizationHandler(ICurrentApplicationUserAccessor currentUserAccessor)
    : AuthorizationHandler<WriteCapableUserRequirement>
{
    /// <inheritdoc/>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        WriteCapableUserRequirement requirement)
    {
        User? user = currentUserAccessor.GetCurrentUser();
        if (context.User.Identity?.IsAuthenticated == true
            && user?.Status == UserStatus.Active
            && user.Role is UserRole.Admin or UserRole.Standard)
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}

/// <summary>
/// Requires an active administrator application user.
/// </summary>
internal sealed class AdministratorAuthorizationHandler(ICurrentApplicationUserAccessor currentUserAccessor)
    : AuthorizationHandler<AdministratorRequirement>
{
    /// <inheritdoc/>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdministratorRequirement requirement)
    {
        User? user = currentUserAccessor.GetCurrentUser();
        if (context.User.Identity?.IsAuthenticated == true
            && user?.Status == UserStatus.Active
            && user.Role == UserRole.Admin)
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}

/// <summary>
/// Applies active-user access to reads and write-capable access to mutations.
/// </summary>
internal sealed class ApplicationAccessAuthorizationHandler(
    IHttpContextAccessor httpContextAccessor,
    ICurrentApplicationUserAccessor currentUserAccessor)
    : AuthorizationHandler<ApplicationAccessRequirement>
{
    /// <inheritdoc/>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ApplicationAccessRequirement requirement)
    {
        User? user = currentUserAccessor.GetCurrentUser();
        if (context.User.Identity?.IsAuthenticated != true || user?.Status != UserStatus.Active)
        {
            return Task.CompletedTask;
        }

        HttpRequest? request = httpContextAccessor.HttpContext?.Request;
        bool isReadRequest = request != null
            && (HttpMethods.IsGet(request.Method)
                || HttpMethods.IsHead(request.Method)
                || HttpMethods.IsOptions(request.Method));
        if (isReadRequest || user.Role is UserRole.Admin or UserRole.Standard)
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}

/// <summary>
/// Requires a provider identity with valid claims for identity resolution.
/// </summary>
internal sealed class ProviderIdentityRequirement : IAuthorizationRequirement;

/// <summary>
/// Requires an active application user.
/// </summary>
internal sealed class ActiveUserRequirement : IAuthorizationRequirement;

/// <summary>
/// Requires an active user with a write-capable role.
/// </summary>
internal sealed class WriteCapableUserRequirement : IAuthorizationRequirement;

/// <summary>
/// Requires an active administrator.
/// </summary>
internal sealed class AdministratorRequirement : IAuthorizationRequirement;

/// <summary>
/// Requires active read access and method-aware mutation access.
/// </summary>
internal sealed class ApplicationAccessRequirement : IAuthorizationRequirement;