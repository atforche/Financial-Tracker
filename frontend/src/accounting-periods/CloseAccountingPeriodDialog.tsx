import { Button, Typography } from "@mui/material";
import type { AccountingPeriod } from "@accounting-periods/ApiTypes";
import Dialog from "@framework/dialog/Dialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import type { JSX } from "react/jsx-runtime";
import useCloseAccountingPeriod from "@accounting-periods/useCloseAccountingPeriod";

/**
 * Props for the CloseAccountingPeriodDialog component.
 */
interface CloseAccountingPeriodDialogProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly setAccountingPeriod: (accountingPeriod: AccountingPeriod) => void;
  readonly onClose: (success: boolean) => void;
}

/**
 * Component that presents the user with a dialog to close an Accounting Period.
 * @param props - Props for the CloseAccountingPeriodDialog component.
 * @returns JSX element representing the CloseAccountingPeriodDialog.
 */
const CloseAccountingPeriodDialog = function ({
  accountingPeriod,
  setAccountingPeriod,
  onClose,
}: CloseAccountingPeriodDialogProps): JSX.Element {
  const {
    isRunning,
    isSuccess,
    error,
    updatedAccountingPeriod,
    closeAccountingPeriod,
  } = useCloseAccountingPeriod({ accountingPeriod });
  if (isSuccess) {
    if (updatedAccountingPeriod) {
      setAccountingPeriod(updatedAccountingPeriod);
    }
    onClose(true);
  }
  return (
    <Dialog
      title="Close Accounting Period"
      content={
        <>
          <Typography>
            Are you sure you want to close the accounting period &quot;
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
          <Button onClick={closeAccountingPeriod} disabled={isRunning}>
            Close
          </Button>
        </>
      }
    />
  );
};

export default CloseAccountingPeriodDialog;
