"use client";

import { Button, DialogActions, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState } from "react";
import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Link from "next/link";
import reopenAccountingPeriod from "@/app/accounting-periods/[id]/reopen/reopenAccountingPeriod";

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
      <Breadcrumbs
        breadcrumbs={[
          {
            label: "Accounting Periods",
            href: "/accounting-periods",
          },
          {
            label: accountingPeriod.name,
            href: `/accounting-periods/${accountingPeriod.id}`,
          },
          {
            label: "Reopen",
            href: `/accounting-periods/${accountingPeriod.id}/reopen`,
          },
        ]}
      />
      <Stack spacing={2} sx={{ maxWidth: "500px" }}>
        <Typography>
          Are you sure you want to reopen the accounting period &quot;
          {accountingPeriod.name}&quot;?
        </Typography>
        <DialogActions>
          <Link
            href={`/accounting-periods/${accountingPeriod.id}`}
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
