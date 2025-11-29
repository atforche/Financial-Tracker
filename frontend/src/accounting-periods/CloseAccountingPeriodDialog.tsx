import { Button, Typography } from "@mui/material";
import type { AccountingPeriod } from "@accounting-periods/ApiTypes";
import Dialog from "@framework/dialog/Dialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import type { JSX } from "react/jsx-runtime";
import useCloseAccountingPeriod from "@accounting-periods/useCloseAccountingPeriod";

/**
 * Props for the CloseAccountingPeriodDialog component.
 * @param accountingPeriod - Accounting Period to close.
 * @param onClose - Callback to perform when this dialog is closed.
 */
interface CloseAccountingPeriodDialogProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly onClose: (success: boolean) => void;
}

/**
 * Component that presents the user with a dialog to close an Accounting Period.
 * @param props - Props for the CloseAccountingPeriodDialog component.
 * @returns JSX element representing the CloseAccountingPeriodDialog.
 */
const CloseAccountingPeriodDialog = function ({
  accountingPeriod,
  onClose,
}: CloseAccountingPeriodDialogProps): JSX.Element {
  const { isRunning, isSuccess, error, closeAccountingPeriod } =
    useCloseAccountingPeriod({ accountingPeriod });
  if (isSuccess) {
    onClose(true);
  }
  return (
    <Dialog
      title="Close Accounting Period"
      content={
        <>
          <Typography>
            Are you sure you want to close the accounting period &quot;
            {accountingPeriod.year}-{accountingPeriod.month}&quot;?
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
          <Button onClick={closeAccountingPeriod} disabled={isRunning}>
            Close
          </Button>
        </>
      }
    />
  );
};

export default CloseAccountingPeriodDialog;
