"use client";

import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
  Typography,
} from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useState,
} from "react";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { Transaction } from "@/transactions/types";
import deleteTransaction from "@/transactions/workspace/deleteTransaction";
import { useRouter } from "next/navigation";

/**
 * Props for the DeleteTransactionForm component.
 */
interface DeleteTransactionFormProps {
  readonly transaction: Transaction;
  readonly redirectUrl: string;
}

/**
 * Component that displays the action for deleting a transaction.
 */
const DeleteTransactionForm = function ({
  transaction,
  redirectUrl,
}: DeleteTransactionFormProps): JSX.Element {
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const [state, action, pending] = useActionState(deleteTransaction, {});

  useEffect(() => {
    if (state.success === true) {
      router.replace(redirectUrl, { scroll: false });
    }
  }, [redirectUrl, router, state.success]);

  return (
    <>
      <Button
        color="error"
        variant="outlined"
        onClick={() => {
          setOpen(true);
        }}
      >
        Delete
      </Button>
      <Dialog
        open={open}
        onClose={
          pending
            ? // eslint-disable-next-line no-undefined
              undefined
            : (): void => {
                setOpen(false);
              }
        }
        fullWidth
        maxWidth="sm"
      >
        <DialogTitle>Delete Transaction</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            <Typography>
              Are you sure you want to delete this transaction?
            </Typography>
            <ErrorAlert
              errorMessage={state.errorTitle ?? null}
              unmappedErrors={state.unmappedErrors ?? null}
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button
            disabled={pending}
            onClick={() => {
              setOpen(false);
            }}
          >
            Cancel
          </Button>
          <Button
            color="error"
            variant="contained"
            loading={pending}
            onClick={() => {
              startTransition(() => {
                action({ transactionId: transaction.id, redirectUrl });
              });
            }}
          >
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
};

export default DeleteTransactionForm;
