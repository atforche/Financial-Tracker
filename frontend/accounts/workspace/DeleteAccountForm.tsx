"use client";

import { type JSX, startTransition, useActionState } from "react";
import type { Account } from "@/accounts/types";
import { Button } from "@mui/material";
import ConfirmActionDialog from "@/framework/dialog/ConfirmActionDialog";
import deleteAccount from "@/accounts/workspace/deleteAccount";

/**
 * Props for the DeleteAccountForm component.
 */
interface DeleteAccountFormProps {
  readonly account: Account;
  readonly redirectUrl: string;
}

/**
 * Displays the action for deleting the selected account.
 */
const DeleteAccountForm = function ({
  account,
  redirectUrl,
}: DeleteAccountFormProps): JSX.Element {
  const [state, action, pending] = useActionState(deleteAccount, {});

  return (
    <ConfirmActionDialog
      trigger={(openDialog) => (
        <Button color="error" variant="outlined" onClick={openDialog}>
          Delete
        </Button>
      )}
      title="Delete Account"
      confirmationCopy={
        <>
          Are you sure you want to delete the account &quot;{account.name}
          &quot;?
        </>
      }
      confirmLabel="Delete"
      confirmButtonProps={{ color: "error" }}
      pending={pending}
      errorTitle={state.errorTitle}
      unmappedErrors={state.unmappedErrors}
      onConfirm={() => {
        startTransition(() => {
          action({ accountId: account.id, redirectUrl });
        });
      }}
    />
  );
};

export default DeleteAccountForm;
