"use client";

import { Button, DialogActions, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState } from "react";
import type { Account } from "@/data/accountTypes";
import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { Fund } from "@/data/fundTypes";
import Link from "next/link";
import type { Transaction } from "@/data/transactionTypes";
import deleteTransaction from "@/app/transactions/[id]/delete/deleteTransaction";
import getBreadcrumbs from "@/app/transactions/[id]/delete/getBreadcrumbs";

/**
 * Props for the DeleteTransactionForm component.
 */
interface DeleteTransactionFormProps {
  readonly transaction: Transaction;
  readonly redirectUrl: string;
  readonly providedAccountingPeriod?: AccountingPeriod | null;
  readonly providedAccount?: Account | null;
  readonly providedFund?: Fund | null;
}

/**
 * Component that displays the form for deleting a transaction.
 */
const DeleteTransactionForm = function ({
  transaction,
  redirectUrl,
  providedAccountingPeriod = null,
  providedAccount = null,
  providedFund = null,
}: DeleteTransactionFormProps): JSX.Element {
  const [state, action, pending] = useActionState(deleteTransaction, {
    transactionId: transaction.id,
    redirectUrl,
  });

  return (
    <Stack spacing={2} maxWidth={800}>
      <Breadcrumbs
        breadcrumbs={getBreadcrumbs(
          transaction,
          providedAccountingPeriod,
          providedAccount,
          providedFund,
        )}
      />
      <Typography>Are you sure you want to delete this transaction?</Typography>
      <DialogActions sx={{ maxWidth: "800px" }}>
        <Link href={redirectUrl} tabIndex={-1}>
          <Button variant="outlined">Cancel</Button>
        </Link>
        <Button
          variant="contained"
          color="error"
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
  );
};

export default DeleteTransactionForm;
