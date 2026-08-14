import Frame from "@/framework/view/Frame";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import { Typography } from "@mui/material";
import UserManagement from "@/users/UserManagement";
import { UserRoleModel } from "@/framework/data/api";
import createApiClient from "@/framework/data/createApiClient";
import getCurrentApplicationUser from "@/framework/auth/currentApplicationUser";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Displays administration controls for application users and invitations.
 */
const UserManagementPage = async function (): Promise<JSX.Element> {
  const currentUser = await getCurrentApplicationUser();
  if (currentUser.user?.role !== UserRoleModel.Admin) {
    return (
      <PageLayout>
        <Frame title="Administrator Access Required" color="error">
          <Typography>
            Your current role cannot manage application users or invitations.
          </Typography>
        </Frame>
      </PageLayout>
    );
  }

  const apiClient = await createApiClient();
  const [usersResponse, invitationsResponse] = await Promise.all([
    apiClient.GET("/users"),
    apiClient.GET("/user-invitations"),
  ]);
  const users = unwrapApiResponse(
    usersResponse,
    "Application users could not be loaded.",
  );
  const invitations = unwrapApiResponse(
    invitationsResponse,
    "User invitations could not be loaded.",
  );

  return (
    <UserManagement
      currentUserId={currentUser.user.id}
      invitations={invitations.items}
      users={users.items}
    />
  );
};

export default UserManagementPage;
