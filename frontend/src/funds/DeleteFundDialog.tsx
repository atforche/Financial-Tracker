import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Typography,
} from "@mui/material";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import type { Fund } from "@funds/ApiTypes";
import type { JSX } from "react";
import useDeleteFund from "@funds/useDeleteFund";

/**
 * Props for the DeleteFundDialog component.
 * @param fund - Fund to display to delete.
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
        <Button
          onClick={() => {
            onClose(false);
          }}
          disabled={isRunning}
        >
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
