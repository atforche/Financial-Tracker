"use client";

import { type JSX, startTransition, useActionState } from "react";
import { Button } from "@mui/material";
import ConfirmActionDialog from "@/framework/dialog/ConfirmActionDialog";
import type { Transaction } from "@/transactions/types";
import deleteTransaction from "@/transactions/workspace/deleteTransaction";

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
  const [state, action, pending] = useActionState(deleteTransaction, {});

  return (
    <ConfirmActionDialog
      trigger={(openDialog) => (
        <Button color="error" variant="outlined" onClick={openDialog}>
          Delete
        </Button>
      )}
      title="Delete Transaction"
      confirmationCopy="Are you sure you want to delete this transaction?"
      confirmLabel="Delete"
      confirmButtonProps={{ color: "error" }}
      pending={pending}
      errorTitle={state.errorTitle}
      unmappedErrors={state.unmappedErrors}
      onConfirm={() => {
        startTransition(() => {
          action({ transactionId: transaction.id, redirectUrl });
        });
      }}
    />
  );
};

export default DeleteTransactionForm;
