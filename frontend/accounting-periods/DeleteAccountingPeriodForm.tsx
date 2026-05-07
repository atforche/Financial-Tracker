"use client";

import { Button, DialogActions, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import Breadcrumbs from "@/framework/Breadcrumbs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Link from "next/link";
import breadcrumbs from "@/accounting-periods/breadcrumbs";
import deleteAccountingPeriod from "@/accounting-periods/deleteAccountingPeriod";
import routes from "@/accounting-periods/routes";

/**
 * Props for the DeleteAccountingPeriodForm component.
 */
interface DeleteAccountingPeriodFormProps {
  readonly accountingPeriod: AccountingPeriod;
}

/**
 * Component that displays the form for deleting an accounting period.
 */
const DeleteAccountingPeriodForm = function ({
  accountingPeriod,
}: DeleteAccountingPeriodFormProps): JSX.Element {
  const [state, action, pending] = useActionState(deleteAccountingPeriod, {
    accountingPeriodId: accountingPeriod.id,
  });

  return (
    <Stack spacing={2}>
      <Breadcrumbs breadcrumbs={breadcrumbs.delete(accountingPeriod)} />
      <Stack spacing={2} sx={{ maxWidth: "500px" }}>
        <Typography>
          Are you sure you want to delete the accounting period &quot;
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
