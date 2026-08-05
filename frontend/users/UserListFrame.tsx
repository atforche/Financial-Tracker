"use client";

import { UserRoleModel, UserStatusModel } from "@/framework/data/api";
import { getPaginationIndex, rowsPerPage } from "@/framework/listframe/page";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import { Typography } from "@mui/material";
import type { User } from "@/users/types";
import UserActions from "@/users/UserActions";
import { formatDate } from "@/users/userManagementHelpers";
import { useSearchParams } from "next/navigation";

/**
 * Props for the paged application-user list.
 */
interface UserListFrameProps {
  readonly currentUserId: string;
  readonly users: readonly User[];
}

const pageParamName = "usersPage";

/**
 * Displays application users with their administrative actions.
 */
const UserListFrame = function ({
  currentUserId,
  users,
}: UserListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const activeAdministratorCount = users.filter(
    (user) =>
      user.role === UserRoleModel.Admin &&
      user.status === UserStatusModel.Active,
  ).length;
  const paginationIndex = getPaginationIndex(
    searchParams.get(pageParamName),
    users.length,
  );
  const pageUsers = users.slice(
    paginationIndex * rowsPerPage,
    (paginationIndex + 1) * rowsPerPage,
  );
  const columns: readonly ColumnDefinition<User>[] = [
    {
      name: "user",
      headerContent: "User",
      getBodyContent: (user) => (
        <>
          <Typography variant="body2">
            {user.displayName ?? user.email}
          </Typography>
          <Typography color="text.secondary" variant="caption">
            {user.email}
          </Typography>
        </>
      ),
    },
    {
      name: "role",
      headerContent: "Role",
      getBodyContent: (user) => user.role,
    },
    {
      name: "status",
      headerContent: "Status",
      getBodyContent: (user) => user.status,
    },
    {
      name: "lastSignIn",
      headerContent: "Last sign-in",
      getBodyContent: (user) => formatDate(user.lastLoginAt),
    },
    {
      name: "actions",
      headerContent: "Actions",
      getBodyContent: (user) => (
        <UserActions
          activeAdministratorCount={activeAdministratorCount}
          currentUserId={currentUserId}
          user={user}
        />
      ),
    },
  ];

  return (
    <ListFrame<User>
      title="Application users"
      columns={columns}
      getId={(user) => user.id}
      data={pageUsers}
      totalCount={users.length}
      pageParamName={pageParamName}
      hasActiveFilters={false}
      initialEmptyState={{
        title: "No application users",
        description: "Users appear here after accepting an invitation.",
      }}
    />
  );
};

export default UserListFrame;
