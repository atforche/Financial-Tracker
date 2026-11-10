"use client";

import { type JSX, useState } from "react";
import PagedTable, { type PagedTableColumn } from "@/framework/view/PagedTable";
import { UserRoleModel, UserStatusModel } from "@/framework/data/api";
import { formatDate, formatUserRole } from "@/users/userManagementHelpers";
import {
  getPaginationIndex,
  getRowsPerPage,
} from "@/framework/view/pagination";
import { Button } from "@mui/material";
import InviteUserForm from "@/users/InviteUserForm";
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
  const rowsPerPage = getRowsPerPage(searchParams.get("pageSize"));
  const activeAdministratorCount = users.filter(
    (user) =>
      user.role === UserRoleModel.Admin &&
      user.status === UserStatusModel.Active,
  ).length;
  const paginationIndex = getPaginationIndex(
    searchParams.get(pageParamName),
    users.length,
    rowsPerPage,
  );
  const pageUsers = users.slice(
    paginationIndex * rowsPerPage,
    (paginationIndex + 1) * rowsPerPage,
  );
  const columns: readonly PagedTableColumn<User>[] = [
    {
      name: "name",
      header: "Name",
      getValue: (user) => user.displayName ?? "—",
      width: "minmax(0, 1fr)",
    },
    {
      name: "email",
      header: "Email",
      getValue: (user) => user.email,
      width: "minmax(0, 1.5fr)",
    },
    {
      name: "role",
      header: "Role",
      getValue: (user) => formatUserRole(user.role),
      width: "minmax(0, 0.8fr)",
      hideOnMobile: true,
    },
    {
      name: "status",
      header: "Status",
      getValue: (user) => user.status,
      width: "minmax(0, 0.8fr)",
      hideOnMobile: true,
    },
    {
      name: "lastSignIn",
      header: "Last sign-in",
      getValue: (user) => formatDate(user.lastLoginAt),
      width: "minmax(0, 1fr)",
      hideOnMobile: true,
    },
  ];

  return (
    <>
      <PagedTable
        title="Application Users"
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
        data={pageUsers}
        getKey={(item) => item.id}
        onRowClick={(item) => {
          setManagedUser(item);
        }}
        totalCount={users.length}
        pageParamName={pageParamName}
        emptyState={{
          title: "No Application Users",
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
