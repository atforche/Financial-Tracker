import { Button, Typography } from "@mui/material";
import ApiErrorHandler from "@data/ApiErrorHandler";
import Dialog from "@framework/dialog/Dialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import type { JSX } from "react/jsx-runtime";
import type { Transaction } from "@transactions/ApiTypes";
import useDeleteTransaction from "@transactions/useDeleteTransaction";

/**
 * Props for the DeleteTransactionDialog component.
 * @param transaction - Transaction to delete.
 * @param onClose - Callback to perform when this dialog is closed.
 */
interface DeleteTransactionDialogProps {
  readonly transaction: Transaction;
  readonly onClose: (success: boolean) => void;
}

/**
 * Component that presents the user with a dialog to delete a Transaction.
 * @param props - Props for the DeleteTransactionDialog component.
 * @returns JSX element representing the DeleteTransactionDialog.
 */
const DeleteTransactionDialog = function ({
  transaction,
  onClose,
}: DeleteTransactionDialogProps): JSX.Element {
  const { isRunning, isSuccess, error, deleteTransaction } =
    useDeleteTransaction({ transaction });
  if (isSuccess) {
    onClose(true);
  }
  const errorHandler = error === null ? null : new ApiErrorHandler(error);
  return (
    <Dialog
      title="Delete Transaction"
      content={
        <>
          <Typography>
            Are you sure you want to delete this transaction?
          </Typography>
          <ErrorAlert errorHandler={errorHandler} />
        </>
      }
      actions={
        <>
          <Button
            onClick={() => {
              onClose(false);
            }}
            disabled={isRunning}
          >
            Cancel
          </Button>
          <Button onClick={deleteTransaction} disabled={isRunning}>
            Delete
          </Button>
        </>
      }
      onClose={() => {
        onClose(false);
      }}
    />
  );
};

export default DeleteTransactionDialog;
