"use client";

import { Button, DialogActions, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import Breadcrumbs from "@/framework/Breadcrumbs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Link from "next/link";
import breadcrumbs from "@/accounting-periods/breadcrumbs";
import reopenAccountingPeriod from "@/accounting-periods/reopenAccountingPeriod";
import routes from "@/accounting-periods/routes";

/**
 * Props for the ReopenAccountingPeriodForm component.
 */
interface ReopenAccountingPeriodFormProps {
  readonly accountingPeriod: AccountingPeriod;
}

/**
 * Component that displays the form for reopening an accounting period.
 */
const ReopenAccountingPeriodForm = function ({
  accountingPeriod,
}: ReopenAccountingPeriodFormProps): JSX.Element {
  const [state, action, pending] = useActionState(reopenAccountingPeriod, {
    accountingPeriodId: accountingPeriod.id,
  });

  return (
    <Stack spacing={2}>
      <Breadcrumbs breadcrumbs={breadcrumbs.reopen(accountingPeriod)} />
      <Stack spacing={2} sx={{ maxWidth: "500px" }}>
        <Typography>
          Are you sure you want to reopen the accounting period &quot;
          {accountingPeriod.name}&quot;?
        </Typography>
        <DialogActions>
          <Link
            href={routes.detail({ id: accountingPeriod.id }, {})}
            tabIndex={-1}
          >
            <Button variant="outlined">Cancel</Button>
          </Link>
          <Button
            variant="contained"
            loading={pending}
            onClick={() => {
              startTransition(() => {
                action();
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
    </Stack>
  );
};

export default ReopenAccountingPeriodForm;
