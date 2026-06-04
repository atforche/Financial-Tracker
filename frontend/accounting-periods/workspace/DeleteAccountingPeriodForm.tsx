"use client";

import { Button, DialogActions, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import deleteAccountingPeriod from "@/accounting-periods/workspace/deleteAccountingPeriod";

/**
 * Props for the DeleteAccountingPeriodForm component.
 */
interface DeleteAccountingPeriodFormProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly redirectUrl: string;
}

/**
 * Component that displays the form for deleting an accounting period.
 */
const DeleteAccountingPeriodForm = function ({
  accountingPeriod,
  redirectUrl,
}: DeleteAccountingPeriodFormProps): JSX.Element {
  const [state, action, pending] = useActionState(deleteAccountingPeriod, {});
  return (
    <Stack spacing={2}>
      <Typography>
        Are you sure you want to delete the accounting period &quot;
        {accountingPeriod.name}&quot;?
      </Typography>
      <DialogActions>
        <Button
          variant="contained"
          loading={pending}
          onClick={() => {
            startTransition(() => {
              action({ accountingPeriodId: accountingPeriod.id, redirectUrl });
            });
          }}
        >
          Delete
        </Button>
      </DialogActions>
      <ErrorAlert
        errorMessage={state.errorTitle ?? null}
        unmappedErrors={state.unmappedErrors ?? null}
      />
    </Stack>
  );
};

export default DeleteAccountingPeriodForm;
