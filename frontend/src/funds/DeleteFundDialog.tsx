import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Typography,
} from "@mui/material";
import { type Fund, deleteFund } from "@data/FundRepository";
import { type JSX, useCallback } from "react";
import { useApiRequest } from "@data/useApiRequest";

/**
 * Props for the DeleteFundDialog component.
 * @param {Fund} fund - Fund to display to delete.
 * @param {() => void} onClose - Callback to perform when this dialog is closed.
 */
interface DeleteFundDialogProps {
  readonly fund: Fund;
  readonly onClose: () => void;
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
  const apiRequestFunction = useCallback(async () => {
    await deleteFund(fund);
  }, [fund]);

  const { isRunning, isSuccess, execute } = useApiRequest({
    apiRequestFunction,
  });

  if (isSuccess) {
    onClose();
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
        <Button onClick={onClose} disabled={isRunning}>
          Cancel
        </Button>
        <Button onClick={execute} loading={isRunning}>
          Delete
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default DeleteFundDialog;
