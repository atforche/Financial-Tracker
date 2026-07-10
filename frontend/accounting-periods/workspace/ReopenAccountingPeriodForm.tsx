"use client";

import { type JSX, startTransition, useActionState } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import { Button } from "@mui/material";
import ConfirmActionDialog from "@/framework/dialog/ConfirmActionDialog";
import reopenAccountingPeriod from "@/accounting-periods/workspace/reopenAccountingPeriod";

/**
 * Props for the ReopenAccountingPeriodForm component.
 */
interface ReopenAccountingPeriodFormProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly redirectUrl: string;
}

/**
 * Component that displays the form for reopening an accounting period.
 */
const ReopenAccountingPeriodForm = function ({
  accountingPeriod,
  redirectUrl,
}: ReopenAccountingPeriodFormProps): JSX.Element {
  const [state, action, pending] = useActionState(reopenAccountingPeriod, {});

  return (
    <ConfirmActionDialog
      trigger={(openDialog) => (
        <Button variant="contained" onClick={openDialog}>
          Reopen
        </Button>
      )}
      title="Reopen Accounting Period"
      confirmationCopy={
        <>
          Are you sure you want to reopen the accounting period &quot;
          {accountingPeriod.name}&quot;?
        </>
      }
      confirmLabel="Reopen"
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

export default ReopenAccountingPeriodForm;
