"use client";

import { type JSX, useState } from "react";
import { UserRoleModel, UserStatusModel } from "@/framework/data/api";
import { formatDate, formatUserRole } from "@/users/userManagementHelpers";
import { getPaginationIndex, rowsPerPage } from "@/framework/listframe/page";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import { Button } from "@mui/material";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import InviteUserForm from "@/users/InviteUserForm";
import ListFrame from "@/framework/listframe/ListFrame";
import ListFrameActionButton from "@/framework/listframe/ListFrameActionButton";
import ManageUserDialog from "@/users/ManageUserDialog";
import type { User } from "@/users/types";
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
  const [inviteDialogOpen, setInviteDialogOpen] = useState(false);
  const [managedUser, setManagedUser] = useState<User | null>(null);
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
      name: "name",
      headerContent: "Name",
      getBodyContent: (user) => user.displayName ?? "—",
      minWidth: 150,
    },
    {
      name: "email",
      headerContent: "Email",
      getBodyContent: (user) => user.email,
      minWidth: 220,
    },
    {
      name: "role",
      headerContent: "Role",
      getBodyContent: (user) => formatUserRole(user.role),
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
      headerContent: "",
      getBodyContent: (user) => (
        <ListFrameActionButton
          size="small"
          color="primary"
          onClick={() => {
            setManagedUser(user);
          }}
          ariaLabel={`Manage ${user.displayName ?? user.email}`}
        >
          <ArrowForwardOutlined fontSize="small" color="action" />
        </ListFrameActionButton>
      ),
      alignment: "right",
      minWidth: 52,
      maxWidth: 52,
    },
  ];

  return (
    <>
      <ListFrame<User>
        title="Application users"
        headerContent={
          <Button
            variant="contained"
            onClick={() => {
              setInviteDialogOpen(true);
            }}
          >
            Invite user
          </Button>
        }
        columns={columns}
        getId={(user) => user.id}
        data={pageUsers}
        totalCount={users.length}
        pageParamName={pageParamName}
        hasActiveFilters={false}
        onRowClick={(user) => {
          setManagedUser(user);
        }}
        initialEmptyState={{
          title: "No application users",
          description: "Users appear here after accepting an invitation.",
        }}
      />
      <InviteUserForm
        open={inviteDialogOpen}
        onClose={() => {
          setInviteDialogOpen(false);
        }}
      />
      {managedUser !== null ? (
        <ManageUserDialog
          activeAdministratorCount={activeAdministratorCount}
          currentUserId={currentUserId}
          open
          user={managedUser}
          onClose={() => {
            setManagedUser(null);
          }}
        />
      ) : null}
    </>
  );
};

export default UserListFrame;
