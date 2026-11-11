"use client";

import {
  type JSX,
  type ReactNode,
  startTransition,
  useActionState,
} from "react";
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

  /**
   * Renders what opens the confirmation, such as a menu item, in place of the default button.
   */
  readonly trigger?: (openDialog: () => void) => ReactNode;
}

/**
 * Component that displays the action for deleting a transaction.
 */
const DeleteTransactionForm = function ({
  transaction,
  redirectUrl,
  trigger,
}: DeleteTransactionFormProps): JSX.Element {
  const [state, action, pending] = useActionState(deleteTransaction, {});

  return (
    <ConfirmActionDialog
      trigger={(openDialog): ReactNode =>
        trigger === undefined ? (
          <Button color="error" variant="outlined" onClick={openDialog}>
            Delete
          </Button>
        ) : (
          trigger(openDialog)
        )
      }
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
