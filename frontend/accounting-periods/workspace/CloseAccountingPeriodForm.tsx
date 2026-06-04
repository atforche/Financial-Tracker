"use client";

import { Button, DialogActions, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
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
    <Stack spacing={2}>
      <Typography>
        Are you sure you want to close the accounting period &quot;
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
          Close
        </Button>
      </DialogActions>
      <ErrorAlert
        errorMessage={state.errorTitle ?? null}
        unmappedErrors={state.unmappedErrors ?? null}
      />
    </Stack>
  );
};

export default CloseAccountingPeriodForm;
