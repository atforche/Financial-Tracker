"use client";

import { Button, DialogActions, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState } from "react";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Link from "next/link";
import type { Transaction } from "@/transactions/types";
import deleteTransaction from "@/transactions/deleteTransaction";

/**
 * Props for the DeleteTransactionForm component.
 */
interface DeleteTransactionFormProps {
  readonly transaction: Transaction;
}

/**
 * Component that displays the form for deleting a transaction.
 */
const DeleteTransactionForm = function ({
  transaction,
}: DeleteTransactionFormProps): JSX.Element {
  const [state, action, pending] = useActionState(deleteTransaction, {
    transactionId: transaction.id,
    redirectUrl: "/",
  });

  return (
    <Stack spacing={2}>
      <Stack spacing={2} sx={{ maxWidth: "600px" }}>
        <Typography>
          Are you sure you want to delete this transaction?
        </Typography>
        <DialogActions>
          <Link href={state.redirectUrl} tabIndex={-1}>
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

export default DeleteTransactionForm;
