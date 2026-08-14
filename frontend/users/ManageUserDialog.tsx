"use client";

import { Alert, Button, MenuItem, Stack, TextField } from "@mui/material";
import { type JSX, useEffect, useState } from "react";
import type { User, UserRole } from "@/users/types";
import { UserRoleModel, UserStatusModel } from "@/framework/data/api";
import {
  changeUserRole,
  disableUser,
  enableUser,
} from "@/users/userManagementActions";
import { formatUserRole, roles } from "@/users/userManagementHelpers";
import ConfirmActionDialog from "@/framework/dialog/ConfirmActionDialog";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import ReadOnlyField from "@/framework/forms/ReadOnlyField";
import useUserManagementAction from "@/users/useUserManagementAction";

/**
 * Props for the ManageUserDialog component.
 */
interface ManageUserDialogProps {
  readonly activeAdministratorCount: number;
  readonly currentUserId: string;
  readonly onClose: () => void;
  readonly open: boolean;
  readonly user: User;
}

/**
 * Displays administrative controls for one application user outside the list.
 */
const ManageUserDialog = function ({
  activeAdministratorCount,
  currentUserId,
  onClose,
  open,
  user,
}: ManageUserDialogProps): JSX.Element {
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
  const statusAction =
    user.status === UserStatusModel.Active ? "Disable" : "Enable";

  useEffect(() => {
    if (state.success && !pending) {
      onClose();
    }
  }, [onClose, pending, state.success]);

  return (
    <Dialog
      open={open}
      onClose={onClose}
      fullWidth
      maxWidth="sm"
      title={`Manage ${user.displayName ?? user.email}`}
      actions={
        <>
          <Button disabled={pending} onClick={onClose}>
            Cancel
          </Button>
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
                {statusAction}
              </Button>
            )}
            title={`${statusAction} User`}
            confirmationCopy={`${statusAction} ${user.email}?${selfWarning}`}
            confirmLabel={statusAction}
            confirmButtonProps={{
              color:
                user.status === UserStatusModel.Active ? "error" : "success",
            }}
            pending={pending}
            success={state.success}
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
          <Button
            variant="contained"
            loading={pending}
            disabled={
              role === user.role ||
              (isLastActiveAdministrator && role !== UserRoleModel.Admin)
            }
            onClick={() => {
              run(async () => changeUserRole(user.id, role));
            }}
          >
            Save changes
          </Button>
        </>
      }
    >
      <Stack spacing={3}>
        {isLastActiveAdministrator ? (
          <Alert severity="warning">
            This is the last active administrator. Keep this user active and
            assigned the Admin role before changing another administrator.
          </Alert>
        ) : null}
        <ReadOnlyField label="Name" value={user.displayName} />
        <ReadOnlyField label="Email address" value={user.email} />
        <TextField
          select
          fullWidth
          label="Role"
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
              {formatUserRole(option)}
            </MenuItem>
          ))}
        </TextField>
        <ReadOnlyField label="Status" value={user.status} />
        <ErrorAlert
          errorMessage={state.errorTitle}
          unmappedErrors={state.unmappedErrors}
        />
      </Stack>
    </Dialog>
  );
};

export default ManageUserDialog;
