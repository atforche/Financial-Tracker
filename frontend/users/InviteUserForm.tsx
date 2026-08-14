"use client";

import { Button, MenuItem, Stack, TextField, Typography } from "@mui/material";
import { type JSX, type SyntheticEvent, useEffect, useState } from "react";
import { formatUserRole, roles } from "@/users/userManagementHelpers";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { UserRole } from "@/users/types";
import { UserRoleModel } from "@/framework/data/api";
import { createUserInvitation } from "@/users/userManagementActions";
import useUserManagementAction from "@/users/useUserManagementAction";

/**
 * Props for the InviteUserForm component.
 */
interface InviteUserFormProps {
  readonly onClose: () => void;
  readonly open: boolean;
}

/**
 * Displays invitation controls in a dialog for an administrator.
 */
const InviteUserForm = function ({
  onClose,
  open,
}: InviteUserFormProps): JSX.Element {
  const [email, setEmail] = useState("");
  const [role, setRole] = useState<UserRole>(UserRoleModel.Standard);
  const { pending, run, state } = useUserManagementAction();

  useEffect(() => {
    if (state.success && !pending) {
      onClose();
    }
  }, [onClose, pending, state.success]);

  const submit = function (event: SyntheticEvent<HTMLFormElement>): void {
    event.preventDefault();
    run(async () => {
      const result = await createUserInvitation(email, role);
      if (result.success) {
        setEmail("");
      }
      return result;
    });
  };

  return (
    <Dialog
      open={open}
      onClose={pending ? undefined : onClose}
      fullWidth
      maxWidth="sm"
      title="Invite User"
      actions={
        <>
          <Button disabled={pending} onClick={onClose}>
            Cancel
          </Button>
          <Button
            variant="contained"
            loading={pending}
            disabled={email.trim() === ""}
            form="invite-user-form"
            type="submit"
          >
            Send invitation
          </Button>
        </>
      }
    >
      <Stack
        component="form"
        id="invite-user-form"
        spacing={3}
        onSubmit={submit}
      >
        <Typography color="text.secondary">
          Invitations are matched to a verified Google email address on first
          sign-in.
        </Typography>
        <TextField
          required
          fullWidth
          label="Email address"
          type="email"
          value={email}
          onChange={(event) => {
            setEmail(event.target.value);
          }}
        />
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
        <ErrorAlert
          errorMessage={state.errorTitle}
          unmappedErrors={state.unmappedErrors}
        />
      </Stack>
    </Dialog>
  );
};

export default InviteUserForm;
