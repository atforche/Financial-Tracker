"use client";

import { Button, DialogActions, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState } from "react";
import type { Account } from "@/accounts/types";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import deleteAccount from "@/accounts/workspace/deleteAccount";

/**
 * Props for the DeleteAccountForm component.
 */
interface DeleteAccountFormProps {
  readonly account: Account;
  readonly redirectUrl: string;
}

/**
 * Displays the inline delete confirmation for the selected account.
 */
const DeleteAccountForm = function ({
  account,
  redirectUrl,
}: DeleteAccountFormProps): JSX.Element {
  const [state, action, pending] = useActionState(deleteAccount, {
    accountId: account.id,
    redirectUrl,
  });

  return (
    <Stack spacing={3}>
      <Typography>
        Are you sure you want to delete the account &quot;{account.name}&quot;?
      </Typography>
      <ErrorAlert
        errorMessage={state.errorTitle ?? null}
        unmappedErrors={state.unmappedErrors ?? null}
      />
      <DialogActions sx={{ px: 0, pb: 0 }}>
        <Button
          variant="contained"
          color="error"
          loading={pending}
          onClick={() => {
            startTransition(() => {
              action();
            });
          }}
        >
          Delete account
        </Button>
      </DialogActions>
    </Stack>
  );
};

export default DeleteAccountForm;
