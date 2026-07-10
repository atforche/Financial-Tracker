"use client";

import { type JSX, startTransition, useActionState } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import { Button } from "@mui/material";
import ConfirmActionDialog from "@/framework/dialog/ConfirmActionDialog";
import closeAccountingPeriod from "@/accounting-periods/workspace/closeAccountingPeriod";

/**
 * Props for the CloseAccountingPeriodForm component.
 */
interface CloseAccountingPeriodFormProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly redirectUrl: string;
}

/**
 * Component that displays the form for closing an accounting period.
 */
const CloseAccountingPeriodForm = function ({
  accountingPeriod,
  redirectUrl,
}: CloseAccountingPeriodFormProps): JSX.Element {
  const [state, action, pending] = useActionState(closeAccountingPeriod, {});
  return (
    <ConfirmActionDialog
      trigger={(openDialog) => (
        <Button variant="contained" onClick={openDialog}>
          Close
        </Button>
      )}
      title="Close Accounting Period"
      confirmationCopy={
        <>
          Are you sure you want to close the accounting period &quot;
          {accountingPeriod.name}&quot;?
        </>
      }
      confirmLabel="Close"
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

export default CloseAccountingPeriodForm;
