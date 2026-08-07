using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Users;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Models.UserInvitations;
using Models.Users;
using Tests.Infrastructure;

namespace Tests.Users;

/// <summary>
/// Verifies the database-backed user and invitation administration API.
/// </summary>
public sealed class UserAdministrationEndpointTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>
    /// Returns the current application user without exposing the provider subject.
    /// </summary>
    [Fact]
    public async Task CurrentUserReturnsSafeDatabaseProfile()
    {
        using FinancialTrackerApplicationFactory factory = new();
        await factory.InitializeDatabaseAsync();
        using HttpClient client = CreateClient(factory);

        using HttpResponseMessage response = await client.GetAsync(new Uri("/users/me", UriKind.Relative));
        string body = await response.Content.ReadAsStringAsync();
        UserModel model = Deserialize<UserModel>(body);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("test-user@example.test", model.Email);
        Assert.Equal(UserRoleModel.Admin, model.Role);
        Assert.Equal(UserStatusModel.Active, model.Status);
        Assert.DoesNotContain("googleSubject", body, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Denies user-management reads and writes to standard and read-only users.
    /// </summary>
    [Fact]
    public async Task NonAdministratorsCannotUseAdministrationEndpoints()
    {
        using FinancialTrackerApplicationFactory factory = new();
        await factory.InitializeDatabaseAsync();
        await factory.SeedUserAsync("standard-admin-api", "standard-admin-api@example.test", UserRole.Standard);
        await factory.SeedUserAsync("readonly-admin-api", "readonly-admin-api@example.test", UserRole.ReadOnly);

        foreach (string subject in new[] { "standard-admin-api", "readonly-admin-api" })
        {
            using HttpClient client = CreateClient(factory, subject);
            using HttpResponseMessage usersResponse = await client.GetAsync(new Uri("/users", UriKind.Relative));
            using HttpResponseMessage invitationsResponse = await client.GetAsync(new Uri("/user-invitations", UriKind.Relative));
            using HttpResponseMessage createResponse = await client.PostAsJsonAsync(
                "/user-invitations",
                new CreateUserInvitationModel
                {
                    Email = $"{subject}@invite.example.test",
                    Role = UserRoleModel.Standard,
                },
                JsonOptions);

            Assert.Equal(HttpStatusCode.Forbidden, usersResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, invitationsResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);
        }
    }

    /// <summary>
    /// Creates, lists, and revokes invitations while returning conflicts for repeated transitions.
    /// </summary>
    [Fact]
    public async Task AdministratorManagesInvitationLifecycle()
    {
        using FinancialTrackerApplicationFactory factory = new();
        await factory.InitializeDatabaseAsync();
        using HttpClient client = CreateClient(factory);
        int initialAuditCount = (await GetAuditsAsync(factory)).Count;

        using HttpResponseMessage createResponse = await client.PostAsJsonAsync(
            "/user-invitations",
            new CreateUserInvitationModel
            {
                Email = "Invitee@Example.test",
                Role = UserRoleModel.Standard,
            },
            JsonOptions);
        UserInvitationModel invitation = await ReadAsync<UserInvitationModel>(createResponse);

        using HttpResponseMessage listResponse = await client.GetAsync(new Uri("/user-invitations", UriKind.Relative));
        CollectionModel<UserInvitationModel> invitations = await ReadAsync<CollectionModel<UserInvitationModel>>(listResponse);
        using HttpResponseMessage duplicateResponse = await client.PostAsJsonAsync(
            "/user-invitations",
            new CreateUserInvitationModel
            {
                Email = "invitee@example.test",
                Role = UserRoleModel.Standard,
            },
            JsonOptions);
        using HttpResponseMessage revokeResponse = await client.DeleteAsync(new Uri($"/user-invitations/{invitation.Id}", UriKind.Relative));
        using HttpResponseMessage repeatedRevokeResponse = await client.DeleteAsync(new Uri($"/user-invitations/{invitation.Id}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.Contains(invitations.Items, candidate => candidate.Id == invitation.Id);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, revokeResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, repeatedRevokeResponse.StatusCode);
        UserInvitation persistedInvitation = Assert.IsType<UserInvitation>(await factory.GetInvitationAsync("invitee@example.test"));
        Assert.Equal(UserInvitationStatus.Revoked, persistedInvitation.Status);
        Assert.Equal(initialAuditCount + 2, (await GetAuditsAsync(factory)).Count);
    }

    /// <summary>
    /// Changes, disables, and re-enables a user through administrator routes.
    /// </summary>
    [Fact]
    public async Task AdministratorChangesRoleAndAccess()
    {
        using FinancialTrackerApplicationFactory factory = new();
        await factory.InitializeDatabaseAsync();
        await factory.SeedUserAsync("managed-user", "managed-user@example.test", UserRole.Standard);
        User managedUser = Assert.IsType<User>(await factory.GetUserAsync("managed-user"));
        using HttpClient client = CreateClient(factory);

        using HttpResponseMessage roleResponse = await client.PostAsJsonAsync(
            $"/users/{managedUser.Id.Value}/role",
            new ChangeUserRoleModel { Role = UserRoleModel.ReadOnly },
            JsonOptions);
        using HttpResponseMessage disableResponse = await client.PostAsync(new Uri($"/users/{managedUser.Id.Value}/disable", UriKind.Relative), null);
        using HttpResponseMessage enableResponse = await client.PostAsync(new Uri($"/users/{managedUser.Id.Value}/enable", UriKind.Relative), null);
        User persistedUser = Assert.IsType<User>(await factory.GetUserAsync("managed-user"));

        Assert.Equal(HttpStatusCode.OK, roleResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, disableResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, enableResponse.StatusCode);
        Assert.Equal(UserRole.ReadOnly, persistedUser.Role);
        Assert.Equal(UserStatus.Active, persistedUser.Status);
        Assert.Equal(
            3,
            (await GetAuditsAsync(factory)).Count(audit => audit.TargetUserId == managedUser.Id));
    }

    /// <summary>
    /// Preserves the final active administrator when role or status changes are requested.
    /// </summary>
    [Fact]
    public async Task FinalAdministratorCannotBeDemotedOrDisabled()
    {
        using FinancialTrackerApplicationFactory factory = new();
        await factory.InitializeDatabaseAsync();
        User admin = Assert.IsType<User>(await factory.GetUserAsync("test-user"));
        using HttpClient client = CreateClient(factory);
        int initialAuditCount = (await GetAuditsAsync(factory)).Count;

        using HttpResponseMessage roleResponse = await client.PostAsJsonAsync(
            $"/users/{admin.Id.Value}/role",
            new ChangeUserRoleModel { Role = UserRoleModel.Standard },
            JsonOptions);
        using HttpResponseMessage disableResponse = await client.PostAsync(new Uri($"/users/{admin.Id.Value}/disable", UriKind.Relative), null);
        User persistedAdmin = Assert.IsType<User>(await factory.GetUserAsync("test-user"));

        Assert.Equal(HttpStatusCode.Conflict, roleResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, disableResponse.StatusCode);
        Assert.Equal(UserRole.Admin, persistedAdmin.Role);
        Assert.Equal(UserStatus.Active, persistedAdmin.Status);
        Assert.Equal(initialAuditCount, (await GetAuditsAsync(factory)).Count);
    }

    /// <summary>
    /// Returns validation and not-found errors using the user-management contract.
    /// </summary>
    [Fact]
    public async Task InvalidAdministrationRequestsReturnDeliberateErrors()
    {
        using FinancialTrackerApplicationFactory factory = new();
        await factory.InitializeDatabaseAsync();
        using HttpClient client = CreateClient(factory);

        using HttpResponseMessage invalidEmailResponse = await client.PostAsJsonAsync(
            "/user-invitations",
            new CreateUserInvitationModel
            {
                Email = "not-an-email",
                Role = UserRoleModel.Standard,
            },
            JsonOptions);
        using HttpResponseMessage invalidRoleResponse = await client.PostAsJsonAsync(
            "/user-invitations",
            new CreateUserInvitationModel { Email = "valid@example.test" },
            JsonOptions);
        using HttpResponseMessage missingUserResponse = await client.PostAsJsonAsync(
            $"/users/{Guid.NewGuid()}/role",
            new ChangeUserRoleModel { Role = UserRoleModel.Standard },
            JsonOptions);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, invalidEmailResponse.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, invalidRoleResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingUserResponse.StatusCode);
    }

    private static HttpClient CreateClient(FinancialTrackerApplicationFactory factory, string subject = "test-user")
    {
        HttpClient client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-User", subject);
        return client;
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        _ = response.EnsureSuccessStatusCode();
        T? value = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        return value ?? throw new InvalidOperationException("The response body was empty.");
    }

    private static T Deserialize<T>(string body) =>
        JsonSerializer.Deserialize<T>(body, JsonOptions)
        ?? throw new InvalidOperationException("The response body was empty.");

    private static async Task<IReadOnlyCollection<UserAdministrationAuditEvent>> GetAuditsAsync(
        FinancialTrackerApplicationFactory factory)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        IUserAdministrationAuditEventRepository repository =
            scope.ServiceProvider.GetRequiredService<IUserAdministrationAuditEventRepository>();
        return await Task.FromResult(repository.GetAll().ToArray());
    }
}