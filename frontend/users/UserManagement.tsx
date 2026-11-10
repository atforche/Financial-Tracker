"use client";

import type { User, UserInvitation } from "@/users/types";
import InvitationListFrame from "@/users/InvitationListFrame";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import { Typography } from "@mui/material";
import UserListFrame from "@/users/UserListFrame";

/**
 * Props for the user-management page client.
 */
interface UserManagementProps {
  readonly currentUserId: string;
  readonly invitations: readonly UserInvitation[];
  readonly users: readonly User[];
}

/**
 * Composes the administrator user and invitation management experience.
 */
const UserManagement = function ({
  currentUserId,
  invitations,
  users,
}: UserManagementProps): JSX.Element {
  return (
    <PageLayout>
      <Typography variant="h4">Manage Users</Typography>
      <UserListFrame currentUserId={currentUserId} users={users} />
      <InvitationListFrame invitations={invitations} />
    </PageLayout>
  );
};

export default UserManagement;
