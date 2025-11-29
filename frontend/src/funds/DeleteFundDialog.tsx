import { Button, Typography } from "@mui/material";
import Dialog from "@framework/dialog/Dialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import type { Fund } from "@funds/ApiTypes";
import type { JSX } from "react";
import useDeleteFund from "@funds/useDeleteFund";

/**
 * Props for the DeleteFundDialog component.
 * @param fund - Fund to delete.
 * @param onClose - Callback to perform when this dialog is closed.
 */
interface DeleteFundDialogProps {
  readonly fund: Fund;
  readonly onClose: (success: boolean) => void;
}

/**
 * Component that presents the user with a dialog to delete a Fund.
 * @param props - Props for the DeleteFundDialog component.
 * @returns JSX element representing the DeleteFundDialog.
 */
const DeleteFundDialog = function ({
  fund,
  onClose,
}: DeleteFundDialogProps): JSX.Element {
  const { isRunning, isSuccess, error, deleteFund } = useDeleteFund({
    fund,
  });
  if (isSuccess) {
    onClose(true);
  }
  return (
    <Dialog
      title="Delete Fund"
      content={
        <>
          <Typography>
            Are you sure you want to delete the fund &quot;{fund.name}&quot;?
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
          <Button onClick={deleteFund} disabled={isRunning}>
            Delete
          </Button>
        </>
      }
    />
  );
};

export default DeleteFundDialog;
