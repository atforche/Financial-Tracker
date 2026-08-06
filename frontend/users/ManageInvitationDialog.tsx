"use client";

import { Button, Stack } from "@mui/material";
import { type JSX, useEffect } from "react";
import { formatDate, formatUserRole } from "@/users/userManagementHelpers";
import ConfirmActionDialog from "@/framework/dialog/ConfirmActionDialog";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import ReadOnlyField from "@/framework/forms/ReadOnlyField";
import type { UserInvitation } from "@/users/types";
import { UserInvitationStatusModel } from "@/framework/data/api";
import { revokeUserInvitation } from "@/users/userManagementActions";
import useUserManagementAction from "@/users/useUserManagementAction";

/**
 * Props for the ManageInvitationDialog component.
 */
interface ManageInvitationDialogProps {
  readonly invitation: UserInvitation;
  readonly onClose: () => void;
  readonly open: boolean;
}

/**
 * Displays invitation details and the pending-invitation revocation action.
 */
const ManageInvitationDialog = function ({
  invitation,
  onClose,
  open,
}: ManageInvitationDialogProps): JSX.Element {
  const { pending, run, state } = useUserManagementAction();
  const canRevoke = invitation.status === UserInvitationStatusModel.Pending;

  useEffect(() => {
    if (state.success && !pending) {
      onClose();
    }
  }, [onClose, pending, state.success]);

  return (
    <Dialog
      open={open}
      onClose={pending ? undefined : onClose}
      fullWidth
      maxWidth="sm"
      title="Invitation details"
      actions={
        <>
          <Button disabled={pending} onClick={onClose}>
            Cancel
          </Button>
          {canRevoke ? (
            <ConfirmActionDialog
              trigger={(openDialog) => (
                <Button
                  color="error"
                  disabled={pending}
                  onClick={openDialog}
                  variant="outlined"
                >
                  Revoke invitation
                </Button>
              )}
              title="Revoke invitation"
              confirmationCopy={`Revoke the pending invitation for ${invitation.email}?`}
              confirmLabel="Revoke"
              confirmButtonProps={{ color: "error" }}
              pending={pending}
              success={state.success}
              errorTitle={state.errorTitle}
              unmappedErrors={state.unmappedErrors}
              onConfirm={() => {
                run(async () => revokeUserInvitation(invitation.id));
              }}
            />
          ) : null}
        </>
      }
    >
      <Stack spacing={3}>
        <ReadOnlyField label="Email address" value={invitation.email} />
        <ReadOnlyField label="Role" value={formatUserRole(invitation.role)} />
        <ReadOnlyField label="Status" value={invitation.status} />
        <ReadOnlyField
          label="Created"
          value={formatDate(invitation.createdAt)}
        />
        <ReadOnlyField
          label="Accepted"
          value={formatDate(invitation.acceptedAt)}
        />
        <ErrorAlert
          errorMessage={state.errorTitle}
          unmappedErrors={state.unmappedErrors}
        />
      </Stack>
    </Dialog>
  );
};

export default ManageInvitationDialog;
