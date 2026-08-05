"use client";

import { Button, MenuItem, Stack, TextField, Typography } from "@mui/material";
import { type JSX, type SyntheticEvent, useState } from "react";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Frame from "@/framework/view/Frame";
import type { UserRole } from "@/users/types";
import { UserRoleModel } from "@/framework/data/api";
import { createUserInvitation } from "@/users/userManagementActions";
import { roles } from "@/users/userManagementHelpers";
import useUserManagementAction from "@/users/useUserManagementAction";

/**
 * Displays invitation controls for an administrator.
 */
const InviteUserForm = function (): JSX.Element {
  const [email, setEmail] = useState("");
  const [role, setRole] = useState<UserRole>(UserRoleModel.Standard);
  const { pending, run, state } = useUserManagementAction();

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
    <Frame title="Invite user" color="primary">
      <Stack component="form" spacing={2} onSubmit={submit}>
        <Typography color="text.secondary">
          Invitations are matched to a verified Google email address on first
          sign-in.
        </Typography>
        <Stack direction={{ xs: "column", md: "row" }} spacing={2}>
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
            label="Role"
            value={role}
            sx={{ minWidth: 160 }}
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
          <Button loading={pending} type="submit" variant="contained">
            Send invitation
          </Button>
        </Stack>
        <ErrorAlert
          errorMessage={state.errorTitle}
          unmappedErrors={state.unmappedErrors}
        />
      </Stack>
    </Frame>
  );
};

export default InviteUserForm;
