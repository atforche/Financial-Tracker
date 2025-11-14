import {
  Button,
  Dialog,
  DialogContent,
  DialogTitle,
  Stack,
} from "@mui/material";
import type { Fund } from "@data/FundRepository";
import type { JSX } from "react";
import StringEntryField from "@framework/dialog/StringEntryField";

/**
 * Props for the FundDialog component.
 * @param {Fund} fund - Fund to display in this dialog.
 * @param {() => void} onClose - Callback to perform when this dialog is closed.
 */
interface FundDialogProps {
  readonly fund: Fund;
  readonly onClose: () => void;
}

/**
 * Component that presents the user with a dialog to view a Fund.
 * @param {FundDialogProps} props - Props for the FundDialog component.
 * @returns {JSX.Element} JSX element representing the FundDialog.
 */
const FundDialog = function ({ fund, onClose }: FundDialogProps): JSX.Element {
  return (
    <Dialog open>
      <DialogTitle
        sx={{
          backgroundColor: "primary.main",
          color: "white",
          minWidth: "500px",
        }}
      >
        View Fund
      </DialogTitle>
      <DialogContent>
        <Stack
          direction="column"
          spacing={3}
          sx={{ paddingLeft: "25px", paddingRight: "25px", paddingTop: "25px" }}
        >
          <>
            <StringEntryField label="Name" value={fund.name} />
            <StringEntryField label="Description" value={fund.description} />
          </>
        </Stack>
        <Stack direction="row" justifyContent="right">
          <Button onClick={onClose}>Close</Button>
        </Stack>
      </DialogContent>
    </Dialog>
  );
};

export default FundDialog;
