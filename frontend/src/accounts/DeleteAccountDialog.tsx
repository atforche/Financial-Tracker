import { Button, Typography } from "@mui/material";
import type { Account } from "@accounts/ApiTypes";
import ApiErrorHandler from "@data/ApiErrorHandler";
import Dialog from "@framework/dialog/Dialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import type { JSX } from "react/jsx-runtime";
import useDeleteAccount from "@accounts/useDeleteAccount";

/**
 * Props for the DeleteAccountDialog component.
 * @param account - Account to delete.
 * @param onClose - Callback to perform when this dialog is closed.
 */
interface DeleteAccountDialogProps {
  readonly account: Account;
  readonly onClose: (success: boolean) => void;
}

/**
 * Component that presents the user with a dialog to delete an Account.
 * @param props - Props for the DeleteAccountDialog component.
 * @returns JSX element representing the DeleteAccountDialog.
 */
const DeleteAccountDialog = function ({
  account,
  onClose,
}: DeleteAccountDialogProps): JSX.Element {
  const { isRunning, isSuccess, error, deleteAccount } = useDeleteAccount({
    account,
  });
  if (isSuccess) {
    onClose(true);
  }
  const errorHandler = error ? new ApiErrorHandler(error) : null;
  return (
    <Dialog
      title="Delete Account"
      content={
        <>
          <Typography>
            Are you sure you want to delete the account &quot;
            {account.name}&quot;?
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
          <Button onClick={deleteAccount} disabled={isRunning}>
            Delete
          </Button>
        </>
      }
    />
  );
};

export default DeleteAccountDialog;
