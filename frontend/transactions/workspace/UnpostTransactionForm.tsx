"use client";

import { Button, DialogActions, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState } from "react";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Link from "next/link";
import type { Transaction } from "@/transactions/types";
import unpostTransaction from "@/transactions/workspace/unpostTransaction";

/**
 * Props for the UnpostTransactionForm component.
 */
interface UnpostTransactionFormProps {
  readonly transaction: Transaction;
  readonly redirectUrl: string;
}

/**
 * Component that displays the form for unposting a transaction.
 */
const UnpostTransactionForm = function ({
  transaction,
  redirectUrl,
}: UnpostTransactionFormProps): JSX.Element {
  const [state, action, pending] = useActionState(unpostTransaction, {});

  return (
    <Stack spacing={2}>
      <Stack spacing={2} sx={{ maxWidth: "600px" }}>
        <Typography>
          Are you sure you want to unpost this transaction from all posted
          accounts?
        </Typography>
        <DialogActions>
          <Link href={redirectUrl} tabIndex={-1}>
            <Button variant="outlined">Cancel</Button>
          </Link>
          <Button
            variant="contained"
            loading={pending}
            onClick={() => {
              startTransition(() => {
                action({ transactionId: transaction.id, redirectUrl });
              });
            }}
          >
            Unpost
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

export default UnpostTransactionForm;
