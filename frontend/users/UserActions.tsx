"use client";

import { Button, MenuItem, Stack, TextField } from "@mui/material";
import { type JSX, useState } from "react";
import type { User, UserRole } from "@/users/types";
import { UserRoleModel, UserStatusModel } from "@/framework/data/api";
import {
  changeUserRole,
  disableUser,
  enableUser,
} from "@/users/userManagementActions";
import ConfirmActionDialog from "@/framework/dialog/ConfirmActionDialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import { roles } from "@/users/userManagementHelpers";
import useUserManagementAction from "@/users/useUserManagementAction";

/**
 * Props for administrative actions for one application user.
 */
interface UserActionsProps {
  readonly activeAdministratorCount: number;
  readonly currentUserId: string;
  readonly user: User;
}

/**
 * Displays administrative actions for one application user.
 */
const UserActions = function ({
  activeAdministratorCount,
  currentUserId,
  user,
}: UserActionsProps): JSX.Element {
  const [role, setRole] = useState<UserRole>(user.role);
  const { pending, run, state } = useUserManagementAction();
  const isSelf = user.id === currentUserId;
  const isLastActiveAdministrator =
    user.role === UserRoleModel.Admin &&
    user.status === UserStatusModel.Active &&
    activeAdministratorCount === 1;
  const selfWarning = isSelf
    ? " This changes your own access; another active administrator must remain."
    : "";

  return (
    <Stack spacing={1}>
      <Stack direction={{ xs: "column", lg: "row" }} spacing={1}>
        <TextField
          select
          label="Role"
          size="small"
          value={role}
          onChange={(event) => {
            const nextRole = roles.find(
              (option) => option.toString() === event.target.value,
            );
            if (nextRole !== undefined) {
              setRole(nextRole);
            }
          }}
        >
          {roles.map((option) => (
            <MenuItem key={option} value={option}>
              {option}
            </MenuItem>
          ))}
        </TextField>
        <ConfirmActionDialog
          trigger={(openDialog) => (
            <Button
              disabled={
                pending ||
                role === user.role ||
                (isLastActiveAdministrator && role !== UserRoleModel.Admin)
              }
              onClick={openDialog}
              variant="outlined"
            >
              Change role
            </Button>
          )}
          title="Change user role"
          confirmationCopy={`Change ${user.email} to ${role}?${selfWarning}`}
          confirmLabel="Change role"
          pending={pending}
          errorTitle={state.errorTitle}
          unmappedErrors={state.unmappedErrors}
          onConfirm={() => {
            run(async () => changeUserRole(user.id, role));
          }}
        />
        <ConfirmActionDialog
          trigger={(openDialog) => (
            <Button
              color={
                user.status === UserStatusModel.Active ? "error" : "success"
              }
              disabled={pending || isLastActiveAdministrator}
              onClick={openDialog}
              variant="outlined"
            >
              {user.status === UserStatusModel.Active ? "Disable" : "Enable"}
            </Button>
          )}
          title={
            user.status === UserStatusModel.Active
              ? "Disable user"
              : "Enable user"
          }
          confirmationCopy={`${user.status === UserStatusModel.Active ? "Disable" : "Enable"} ${user.email}?${selfWarning}`}
          confirmLabel={
            user.status === UserStatusModel.Active ? "Disable" : "Enable"
          }
          confirmButtonProps={{
            color: user.status === UserStatusModel.Active ? "error" : "success",
          }}
          pending={pending}
          errorTitle={state.errorTitle}
          unmappedErrors={state.unmappedErrors}
          onConfirm={() => {
            run(async () =>
              user.status === UserStatusModel.Active
                ? disableUser(user.id)
                : enableUser(user.id),
            );
          }}
        />
      </Stack>
      <ErrorAlert
        errorMessage={state.errorTitle}
        unmappedErrors={state.unmappedErrors}
      />
    </Stack>
  );
};

export default UserActions;
