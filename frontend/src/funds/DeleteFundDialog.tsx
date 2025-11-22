import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Typography,
} from "@mui/material";
import { type JSX, useCallback } from "react";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import type { Fund } from "@funds/ApiTypes";
import useDeleteFund from "@funds/useDeleteFund";

/**
 * Props for the DeleteFundDialog component.
 * @param {Fund} fund - Fund to display to delete.
 * @param {(boolean) => void} onClose - Callback to perform when this dialog is closed.
 */
interface DeleteFundDialogProps {
  readonly fund: Fund;
  readonly onClose: (success: boolean) => void;
}

/**
 * Component that presents the user with a dialog to delete a Fund.
 * @param {DeleteFundDialogProps} props - Props for the DeleteFundDialog component.
 * @returns {JSX.Element} JSX element representing the DeleteFundDialog.
 */
const DeleteFundDialog = function ({
  fund,
  onClose,
}: DeleteFundDialogProps): JSX.Element {
  const { isRunning, isSuccess, error, deleteFund } = useDeleteFund({
    fund,
  });

  const onCancel = useCallback((): void => {
    onClose(false);
  }, [onClose]);

  if (isSuccess) {
    onClose(true);
  }

  return (
    <Dialog open>
      <DialogTitle
        sx={{
          backgroundColor: "primary.main",
          color: "white",
          minWidth: "500px",
        }}
      >
        Delete Fund
      </DialogTitle>
      <DialogContent>
        <Typography>
          Are you sure you want to delete the fund &quot;{fund.name}&quot;?
        </Typography>
      </DialogContent>
      <DialogActions>
        <Button onClick={onCancel} disabled={isRunning}>
          Cancel
        </Button>
        <Button onClick={deleteFund} loading={isRunning}>
          Delete
        </Button>
      </DialogActions>
      <ErrorAlert error={error} />
    </Dialog>
  );
};

export default DeleteFundDialog;
