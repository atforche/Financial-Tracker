"use client";

import { Button, DialogActions, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState } from "react";
import Breadcrumbs from "@/framework/Breadcrumbs";
import type { CloseAccountingPeriodNavigationContext } from "@/accounting-periods/close/closeAccountingPeriodNavigationContext";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Link from "next/link";
import closeAccountingPeriod from "@/accounting-periods/close/closeAccountingPeriod";

/**
 * Props for the CloseAccountingPeriodForm component.
 */
interface CloseAccountingPeriodFormProps {
  readonly navigationContext: CloseAccountingPeriodNavigationContext;
}

/**
 * Component that displays the form for closing an accounting period.
 */
const CloseAccountingPeriodForm = function ({
  navigationContext,
}: CloseAccountingPeriodFormProps): JSX.Element {
  const accountingPeriod = navigationContext.routeAccountingPeriod;
  const [state, action, pending] = useActionState(closeAccountingPeriod(accountingPeriod.id, navigationContext.redirect), {});

  return (
    <Stack spacing={2}>
      <Breadcrumbs breadcrumbs={navigationContext.breadcrumbs} />
      <Stack spacing={2} sx={{ maxWidth: "500px" }}>
        <Typography>
          Are you sure you want to close the accounting period &quot;
          {accountingPeriod.name}&quot;?
        </Typography>
        <DialogActions>
          <Link href={navigationContext.redirect} tabIndex={-1}>
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
            Close
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

export default CloseAccountingPeriodForm;
