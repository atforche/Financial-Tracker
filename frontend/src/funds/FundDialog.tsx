import { Button } from "@mui/material";
import Dialog from "@framework/dialog/Dialog";
import type { Fund } from "@funds/ApiTypes";
import type { JSX } from "react";
import StringEntryField from "@framework/dialog/StringEntryField";

/**
 * Props for the FundDialog component.
 * @param fund - Fund to display in this dialog.
 * @param onClose - Callback to perform when this dialog is closed.
 */
interface FundDialogProps {
  readonly fund: Fund;
  readonly onClose: () => void;
}

/**
 * Component that presents the user with a dialog to view a Fund.
 * @param props - Props for the FundDialog component.
 * @returns JSX element representing the FundDialog.
 */
const FundDialog = function ({ fund, onClose }: FundDialogProps): JSX.Element {
  return (
    <Dialog
      title="View Fund"
      content={
        <>
          <StringEntryField label="Name" value={fund.name} />
          <StringEntryField label="Description" value={fund.description} />
        </>
      }
      actions={<Button onClick={onClose}>Close</Button>}
    />
  );
};

export default FundDialog;
