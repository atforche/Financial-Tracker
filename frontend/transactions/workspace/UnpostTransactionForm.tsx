"use client";

import { Button, Stack, Typography } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useState,
} from "react";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { Transaction } from "@/transactions/types";
import unpostTransaction from "@/transactions/workspace/unpostTransaction";
import { useRouter } from "next/navigation";

/**
 * Props for the UnpostTransactionForm component.
 */
interface UnpostTransactionFormProps {
  readonly transaction: Transaction;
  readonly redirectUrl: string;
}

/**
 * Component that displays the action for unposting a transaction.
 */
const UnpostTransactionForm = function ({
  transaction,
  redirectUrl,
}: UnpostTransactionFormProps): JSX.Element {
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const [state, action, pending] = useActionState(unpostTransaction, {});

  useEffect(() => {
    if (state.success === true) {
      router.replace(redirectUrl, { scroll: false });
    }
  }, [redirectUrl, router, state.success]);

  return (
    <>
      <Button
        variant="outlined"
        onClick={() => {
          setOpen(true);
        }}
      >
        Unpost
      </Button>
      <Dialog
        open={open}
        onClose={
          pending
            ? undefined
            : (): void => {
                setOpen(false);
              }
        }
        fullWidth
        maxWidth="sm"
        title="Unpost Transaction"
        actions={
          <>
            <Button
              disabled={pending}
              onClick={() => {
                setOpen(false);
              }}
            >
              Cancel
            </Button>
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
          </>
        }
      >
        <Stack spacing={2}>
          <Typography>
            Are you sure you want to unpost this transaction from all posted
            accounts?
          </Typography>
          <ErrorAlert
            errorMessage={state.errorTitle ?? null}
            unmappedErrors={state.unmappedErrors ?? null}
          />
        </Stack>
      </Dialog>
    </>
  );
};

export default UnpostTransactionForm;
