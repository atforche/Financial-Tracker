"use client";

import { Button, DialogActions, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
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
    <Stack spacing={2}>
      <Typography>
        Are you sure you want to reopen the accounting period &quot;
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
          Reopen
        </Button>
      </DialogActions>
      <ErrorAlert
        errorMessage={state.errorTitle ?? null}
        unmappedErrors={state.unmappedErrors ?? null}
      />
    </Stack>
  );
};

export default ReopenAccountingPeriodForm;
