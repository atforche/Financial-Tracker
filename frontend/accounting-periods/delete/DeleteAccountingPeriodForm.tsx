"use client";

import { Button, DialogActions, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState } from "react";
import Breadcrumbs from "@/framework/Breadcrumbs";
import type { DeleteAccountingPeriodNavigationContext } from "@/accounting-periods/delete/deleteAccountingPeriodNavigationContext";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Link from "next/link";
import deleteAccountingPeriod from "@/accounting-periods/delete/deleteAccountingPeriod";

/**
 * Props for the DeleteAccountingPeriodForm component.
 */
interface DeleteAccountingPeriodFormProps {
  readonly navigationContext: DeleteAccountingPeriodNavigationContext;
}

/**
 * Component that displays the form for deleting an accounting period.
 */
const DeleteAccountingPeriodForm = function ({
  navigationContext,
}: DeleteAccountingPeriodFormProps): JSX.Element {
  const [state, action, pending] = useActionState(deleteAccountingPeriod(
    navigationContext.routeAccountingPeriod.id,
    navigationContext.redirect
  ), {});

  return (
    <Stack spacing={2}>
      <Breadcrumbs breadcrumbs={navigationContext.breadcrumbs} />
      <Stack spacing={2} sx={{ maxWidth: "500px" }}>
        <Typography>
          Are you sure you want to delete the accounting period &quot;
          {navigationContext.routeAccountingPeriod.name}&quot;?
        </Typography>
        <DialogActions>
          <Link
            href={navigationContext.redirect}
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
            Delete
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

export default DeleteAccountingPeriodForm;
