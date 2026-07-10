"use client";

import { type JSX, startTransition, useActionState } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import { Button } from "@mui/material";
import ConfirmActionDialog from "@/framework/dialog/ConfirmActionDialog";
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
    <ConfirmActionDialog
      trigger={(openDialog) => (
        <Button color="error" variant="contained" onClick={openDialog}>
          Delete
        </Button>
      )}
      title="Delete Accounting Period"
      confirmationCopy={
        <>
          Are you sure you want to delete the accounting period &quot;
          {accountingPeriod.name}&quot;?
        </>
      }
      confirmLabel="Delete"
      confirmColor="error"
      pending={pending}
      errorTitle={state.errorTitle}
      unmappedErrors={state.unmappedErrors}
      onConfirm={() => {
        startTransition(() => {
          action({ accountingPeriodId: accountingPeriod.id, redirectUrl });
        });
      }}
    />
  );
};

export default DeleteAccountingPeriodForm;
