using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Models.Accounts;
using Tests.Infrastructure;

namespace Tests.Authentication;

/// <summary>
/// Verifies provider identity resolution and database-backed application authorization.
/// </summary>
public sealed class UserResolutionAuthorizationTests
{
    /// <summary>
    /// Allows an existing active user to refresh a changed provider email.
    /// </summary>
    [Fact]
    public async Task ExistingActiveUserSucceedsWithChangedEmail()
    {
        using JwtBearerAuthenticationApplicationFactory factory = new();
        await factory.InitializeDatabaseAsync();
        using HttpClient client = CreateClient(factory, factory.CreateToken(
            JwtBearerAuthenticationApplicationFactory.ProvisionedSubject,
            email: "changed@example.test",
            emailVerified: true,
            displayName: "Changed Name"));

        using HttpResponseMessage response = await ResolveUserAsync(client);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        User user = Assert.IsType<User>(await factory.GetUserAsync(JwtBearerAuthenticationApplicationFactory.ProvisionedSubject));
        Assert.Equal("changed@example.test", user.Email);
        Assert.Equal("Changed Name", user.DisplayName);
    }

    /// <summary>
    /// Rejects an authenticated provider subject whose application user is disabled.
    /// </summary>
    [Fact]
    public async Task ExistingDisabledUserIsRejected()
    {
        using JwtBearerAuthenticationApplicationFactory factory = new();
        await factory.InitializeDatabaseAsync();
        await factory.SeedUserAsync("disabled-subject", "disabled@example.test", UserRole.Standard, UserStatus.Disabled);
        using HttpClient client = CreateClient(factory, factory.CreateToken(
            "disabled-subject",
            email: "disabled@example.test",
            emailVerified: true));

        using HttpResponseMessage response = await ResolveUserAsync(client);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Accepts a verified matching invitation and persists the provider subject.
    /// </summary>
    [Fact]
    public async Task VerifiedMatchingEmailAcceptsInvitation()
    {
        using JwtBearerAuthenticationApplicationFactory factory = new();
        await factory.InitializeDatabaseAsync();
        await factory.CreateInvitationAsync("test-user", "Invitee@Example.test", UserRole.Standard);
        using HttpClient client = CreateClient(factory, factory.CreateToken(
            "invited-subject",
            email: "invitee@example.test",
            emailVerified: true));

        using HttpResponseMessage response = await ResolveUserAsync(client);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        User user = Assert.IsType<User>(await factory.GetUserAsync("invited-subject"));
        Assert.Equal(UserRole.Standard, user.Role);
        Assert.Equal(UserStatus.Active, user.Status);
        UserInvitation invitation = Assert.IsType<UserInvitation>(await factory.GetInvitationAsync("invitee@example.test"));
        Assert.Equal(UserInvitationStatus.Accepted, invitation.Status);
        Assert.Equal(user.Id, invitation.AcceptedByUserId);
    }

    /// <summary>
    /// Rejects missing, false, and malformed email verification claims.
    /// </summary>
    [Fact]
    public async Task InvalidEmailVerificationClaimsAreRejected()
    {
        using JwtBearerAuthenticationApplicationFactory factory = new();
        await factory.InitializeDatabaseAsync();

        using HttpClient missingClient = CreateClient(factory, factory.CreateToken(
            "missing-verified",
            email: "missing@example.test"));
        using HttpClient falseClient = CreateClient(factory, factory.CreateToken(
            "false-verified",
            email: "false@example.test",
            emailVerified: false));
        using HttpClient malformedClient = CreateClient(factory, factory.CreateToken(
            "malformed-verified",
            email: "not-an-email",
            emailVerified: true));

        using HttpResponseMessage missingResponse = await ResolveUserAsync(missingClient);
        using HttpResponseMessage falseResponse = await ResolveUserAsync(falseClient);
        using HttpResponseMessage malformedResponse = await ResolveUserAsync(malformedClient);

        Assert.Equal(HttpStatusCode.Forbidden, missingResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, falseResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, malformedResponse.StatusCode);
    }

    /// <summary>
    /// Rejects missing or uninvited emails without exposing invitation state.
    /// </summary>
    [Fact]
    public async Task MissingOrMismatchedEmailIsRejectedGenerically()
    {
        using JwtBearerAuthenticationApplicationFactory factory = new();
        await factory.InitializeDatabaseAsync();
        using HttpClient missingClient = CreateClient(factory, factory.CreateToken(
            "missing-email",
            emailVerified: true));
        using HttpClient mismatchedClient = CreateClient(factory, factory.CreateToken(
            "mismatched-email",
            email: "not-invited@example.test",
            emailVerified: true));

        using HttpResponseMessage missingResponse = await ResolveUserAsync(missingClient);
        using HttpResponseMessage mismatchedResponse = await ResolveUserAsync(mismatchedClient);

        Assert.Equal(HttpStatusCode.Forbidden, missingResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, mismatchedResponse.StatusCode);
        Assert.DoesNotContain(
            "not-invited@example.test",
            await mismatchedResponse.Content.ReadAsStringAsync(),
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Rejects provider subject and email collisions without changing either user.
    /// </summary>
    [Fact]
    public async Task SubjectAndEmailCollisionsAreRejected()
    {
        using JwtBearerAuthenticationApplicationFactory factory = new();
        await factory.InitializeDatabaseAsync();
        await factory.SeedUserAsync("existing-subject", "existing@example.test", UserRole.Standard);
        await factory.SeedUserAsync("other-subject", "other@example.test", UserRole.Standard);
        using HttpClient emailCollisionClient = CreateClient(factory, factory.CreateToken(
            "new-subject",
            email: "existing@example.test",
            emailVerified: true));
        using HttpClient subjectCollisionClient = CreateClient(factory, factory.CreateToken(
            "existing-subject",
            email: "other@example.test",
            emailVerified: true));

        using HttpResponseMessage emailCollisionResponse = await ResolveUserAsync(emailCollisionClient);
        using HttpResponseMessage subjectCollisionResponse = await ResolveUserAsync(subjectCollisionClient);

        Assert.Equal(HttpStatusCode.Forbidden, emailCollisionResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, subjectCollisionResponse.StatusCode);
    }

    /// <summary>
    /// Accepts a pending invitation at most once when first-login requests race.
    /// </summary>
    [Fact]
    public async Task ConcurrentFirstLoginAcceptsInvitationOnce()
    {
        using JwtBearerAuthenticationApplicationFactory factory = new();
        await factory.InitializeDatabaseAsync();
        await factory.CreateInvitationAsync("test-user", "race@example.test", UserRole.Standard);
        using HttpClient firstClient = CreateClient(factory, factory.CreateToken(
            "race-subject-one",
            email: "race@example.test",
            emailVerified: true));
        using HttpClient secondClient = CreateClient(factory, factory.CreateToken(
            "race-subject-two",
            email: "race@example.test",
            emailVerified: true));

        Task<HttpResponseMessage> firstRequest = ResolveUserAsync(firstClient);
        Task<HttpResponseMessage> secondRequest = ResolveUserAsync(secondClient);
        HttpResponseMessage[] responses = await Task.WhenAll(firstRequest, secondRequest);

        Assert.Equal(1, responses.Count(response => response.StatusCode == HttpStatusCode.NoContent));
        Assert.Equal(1, responses.Count(response => response.StatusCode == HttpStatusCode.Forbidden));
        foreach (HttpResponseMessage response in responses)
        {
            response.Dispose();
        }
    }

    /// <summary>
    /// Permits every active role to read financial data.
    /// </summary>
    [Fact]
    public async Task EveryActiveRoleCanReadFinancialData()
    {
        using JwtBearerAuthenticationApplicationFactory factory = new();
        await factory.InitializeDatabaseAsync();
        await factory.SeedUserAsync("standard-reader", "standard-reader@example.test", UserRole.Standard);
        await factory.SeedUserAsync("readonly-reader", "readonly-reader@example.test", UserRole.ReadOnly);

        using HttpClient adminClient = CreateClient(factory, factory.CreateToken(JwtBearerAuthenticationApplicationFactory.ProvisionedSubject));
        using HttpClient standardClient = CreateClient(factory, factory.CreateToken("standard-reader"));
        using HttpClient readOnlyClient = CreateClient(factory, factory.CreateToken("readonly-reader"));

        using HttpResponseMessage adminResponse = await adminClient.GetAsync(new Uri("/accounts", UriKind.Relative));
        using HttpResponseMessage standardResponse = await standardClient.GetAsync(new Uri("/accounts", UriKind.Relative));
        using HttpResponseMessage readOnlyResponse = await readOnlyClient.GetAsync(new Uri("/accounts", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, standardResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, readOnlyResponse.StatusCode);
    }

    /// <summary>
    /// Permits administrators and standard users to perform a representative financial mutation.
    /// </summary>
    [Fact]
    public async Task AdminAndStandardUsersCanWriteFinancialData()
    {
        using JwtBearerAuthenticationApplicationFactory factory = new();
        await factory.InitializeDatabaseAsync();
        await factory.SeedUserAsync("standard-writer", "standard-writer@example.test", UserRole.Standard);

        using HttpClient adminClient = CreateClient(factory, factory.CreateToken(JwtBearerAuthenticationApplicationFactory.ProvisionedSubject));
        using HttpClient standardClient = CreateClient(factory, factory.CreateToken("standard-writer"));
        using HttpResponseMessage adminResponse = await adminClient.PostAsJsonAsync("/accounts/onboard", new OnboardAccountModel
        {
            Name = "Admin account",
            Type = AccountTypeModel.Standard,
            OnboardedBalance = 10m,
        });
        using HttpResponseMessage standardResponse = await standardClient.PostAsJsonAsync("/accounts/onboard", new OnboardAccountModel
        {
            Name = "Standard account",
            Type = AccountTypeModel.Standard,
            OnboardedBalance = 10m,
        });

        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, standardResponse.StatusCode);
    }

    /// <summary>
    /// Denies read-only users across representative mutation methods.
    /// </summary>
    [Fact]
    public async Task ReadOnlyUserCannotUseMutationMethods()
    {
        using JwtBearerAuthenticationApplicationFactory factory = new();
        await factory.InitializeDatabaseAsync();
        await factory.SeedUserAsync("readonly-writer", "readonly-writer@example.test", UserRole.ReadOnly);
        using HttpClient client = CreateClient(factory, factory.CreateToken("readonly-writer"));
        (HttpMethod Method, string Uri)[] requests =
        [
            (HttpMethod.Post, "/accounts/onboard"),
            (HttpMethod.Post, "/accounting-periods"),
            (HttpMethod.Post, "/funds/onboard"),
            (HttpMethod.Post, $"/fund-goals/{Guid.NewGuid()}"),
            (HttpMethod.Post, "/transactions"),
            (HttpMethod.Delete, $"/accounts/{Guid.NewGuid()}"),
        ];

        foreach ((HttpMethod method, string uri) in requests)
        {
            Assert.Equal(HttpStatusCode.Forbidden, await SendMutationAsync(client, method, uri));
        }
    }

    /// <summary>
    /// Applies disablement and role changes to the next authenticated request.
    /// </summary>
    [Fact]
    public async Task DisablementAndRoleChangesAffectTheNextRequest()
    {
        using JwtBearerAuthenticationApplicationFactory factory = new();
        await factory.InitializeDatabaseAsync();
        await factory.SeedUserAsync("mutable-user", "mutable@example.test", UserRole.Standard);
        using HttpClient client = CreateClient(factory, factory.CreateToken("mutable-user"));

        using HttpResponseMessage initialRead = await client.GetAsync(new Uri("/accounts", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, initialRead.StatusCode);

        await factory.ChangeUserRoleAsync("test-user", "mutable-user", UserRole.ReadOnly);
        using HttpResponseMessage blockedWrite = await client.PostAsJsonAsync("/accounts/onboard", new OnboardAccountModel
        {
            Name = "Blocked after role change",
            Type = AccountTypeModel.Standard,
            OnboardedBalance = 10m,
        });
        Assert.Equal(HttpStatusCode.Forbidden, blockedWrite.StatusCode);

        await factory.ChangeUserRoleAsync("test-user", "mutable-user", UserRole.Standard);
        await factory.DisableUserAsync("test-user", "mutable-user");
        using HttpResponseMessage disabledRead = await client.GetAsync(new Uri("/accounts", UriKind.Relative));
        Assert.Equal(HttpStatusCode.Forbidden, disabledRead.StatusCode);
    }

    /// <summary>
    /// Allows the administrator policy only for an active administrator user.
    /// </summary>
    [Fact]
    public async Task OnlyAdministratorsSatisfyAdministratorPolicy()
    {
        using JwtBearerAuthenticationApplicationFactory factory = new();
        await factory.InitializeDatabaseAsync();
        await factory.SeedUserAsync("standard-admin-policy", "standard-policy@example.test", UserRole.Standard);
        using IServiceScope scope = factory.Services.CreateScope();
        IHttpContextAccessor httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        IAuthorizationService authorizationService = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        DefaultHttpContext httpContext = new()
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim("sub", "standard-admin-policy")],
                "Test"))
        };
        httpContextAccessor.HttpContext = httpContext;

        AuthorizationResult standardResult = await authorizationService.AuthorizeAsync(
            httpContext.User,
            null,
            "administrator");
        httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim("sub", JwtBearerAuthenticationApplicationFactory.ProvisionedSubject)],
                "Test"))
        };
        httpContextAccessor.HttpContext = httpContext;
        AuthorizationResult adminResult = await authorizationService.AuthorizeAsync(
            httpContext.User,
            null,
            "administrator");

        Assert.False(standardResult.Succeeded);
        Assert.True(adminResult.Succeeded);
    }

    private static HttpClient CreateClient(JwtBearerAuthenticationApplicationFactory factory, string token)
    {
        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        return client;
    }

    private static Task<HttpResponseMessage> ResolveUserAsync(HttpClient client) =>
        client.PostAsync(new Uri("/authentication/resolve-user", UriKind.Relative), null);

    private static async Task<HttpStatusCode> SendMutationAsync(HttpClient client, HttpMethod method, string uri)
    {
        using HttpRequestMessage request = new(method, new Uri(uri, UriKind.Relative));
        if (uri == "/accounts/onboard")
        {
            request.Content = JsonContent.Create(new OnboardAccountModel
            {
                Name = "Blocked account",
                Type = AccountTypeModel.Standard,
                OnboardedBalance = 10m,
            });
        }

        using HttpResponseMessage response = await client.SendAsync(request);
        return response.StatusCode;
    }
}