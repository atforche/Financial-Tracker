import { Button, Typography } from "@mui/material";
import type { AccountingPeriod } from "@accounting-periods/ApiTypes";
import Dialog from "@framework/dialog/Dialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import type { JSX } from "react/jsx-runtime";
import useDeleteAccountingPeriod from "@accounting-periods/useDeleteAccountingPeriod";

/**
 * Props for the DeleteAccountingPeriodDialog component.
 * @param accountingPeriod - Accounting Period to delete.
 * @param onClose - Callback to perform when this dialog is closed.
 */
interface DeleteAccountingPeriodDialogProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly onClose: (success: boolean) => void;
}

/**
 * Component that presents the user with a dialog to delete an Accounting Period.
 * @param props - Props for the DeleteAccountingPeriodDialog component.
 * @returns JSX element representing the DeleteAccountingPeriodDialog.
 */
const DeleteAccountingPeriodDialog = function ({
  accountingPeriod,
  onClose,
}: DeleteAccountingPeriodDialogProps): JSX.Element {
  const { isRunning, isSuccess, error, deleteAccountingPeriod } =
    useDeleteAccountingPeriod({ accountingPeriod });
  if (isSuccess) {
    onClose(true);
  }
  return (
    <Dialog
      title="Delete Accounting Period"
      content={
        <>
          <Typography>
            Are you sure you want to delete the accounting period &quot;
            {accountingPeriod.name}&quot;?
          </Typography>
          <ErrorAlert error={error} />
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
          <Button onClick={deleteAccountingPeriod} disabled={isRunning}>
            Delete
          </Button>
        </>
      }
    />
  );
};

export default DeleteAccountingPeriodDialog;
