using Models;
using Models.UserInvitations;
using Models.Users;
using Tests.Infrastructure;

namespace Tests.Pages;

/// <summary>
/// Characterizes the administrator page's combined user and invitation reads.
/// </summary>
public sealed class UserAdministrationPageReadTests
{
    /// <summary>
    /// Returns the signed-in administrator in the user list and the pending invitation list.
    /// </summary>
    [Fact]
    public async Task AdministrationReturnsCurrentUserAndPendingInvitations()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();

        UserModel currentUser = await test.Api.GetAsync<UserModel>("/users/me");
        CollectionModel<UserModel> users = await test.Api.GetAsync<CollectionModel<UserModel>>("/users");
        CollectionModel<UserInvitationModel> invitations = await test.Api.GetAsync<CollectionModel<UserInvitationModel>>(
            "/user-invitations");

        Assert.Equal(UserRoleModel.Admin, currentUser.Role);
        Assert.Contains(users.Items, item => item.Id == currentUser.Id);
        Assert.Empty(invitations.Items);
    }
}
