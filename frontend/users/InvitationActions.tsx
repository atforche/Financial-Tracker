"use client";

import { Button, Stack } from "@mui/material";
import ConfirmActionDialog from "@/framework/dialog/ConfirmActionDialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { JSX } from "react";
import type { UserInvitation } from "@/users/types";
import { UserInvitationStatusModel } from "@/framework/data/api";
import { revokeUserInvitation } from "@/users/userManagementActions";
import useUserManagementAction from "@/users/useUserManagementAction";

/**
 * Props for administrative actions for one invitation.
 */
interface InvitationActionsProps {
  readonly invitation: UserInvitation;
}

/**
 * Displays revocation controls for a pending invitation.
 */
const InvitationActions = function ({
  invitation,
}: InvitationActionsProps): JSX.Element | null {
  const { pending, run, state } = useUserManagementAction();
  if (invitation.status !== UserInvitationStatusModel.Pending) {
    return null;
  }

  return (
    <Stack spacing={1}>
      <ConfirmActionDialog
        trigger={(openDialog) => (
          <Button color="error" disabled={pending} onClick={openDialog}>
            Revoke
          </Button>
        )}
        title="Revoke invitation"
        confirmationCopy={`Revoke the pending invitation for ${invitation.email}?`}
        confirmLabel="Revoke"
        confirmButtonProps={{ color: "error" }}
        pending={pending}
        errorTitle={state.errorTitle}
        unmappedErrors={state.unmappedErrors}
        onConfirm={() => {
          run(async () => revokeUserInvitation(invitation.id));
        }}
      />
      <ErrorAlert
        errorMessage={state.errorTitle}
        unmappedErrors={state.unmappedErrors}
      />
    </Stack>
  );
};

export default InvitationActions;
