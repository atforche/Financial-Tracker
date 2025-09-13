import {
  Button,
  Dialog,
  DialogContent,
  DialogTitle,
  Stack,
  Typography,
} from "@mui/material";
import type { Fund, FundKey } from "@data/Fund";
import {
  addFund,
  deleteFund,
  getFundByKey,
  updateFund,
} from "@data/FundRepository";
import DialogMode from "@core/fieldValues/DialogMode";
import StringEntryField from "@ui/framework/dialog/StringEntryField";
import { useState } from "react";

const defaultFundName = null;
const defaultFundDescription = null;

/**
 * Props for the FundDialog component.
 * @param {boolean} isOpen - True if this modal should be open, false otherwise.
 * @param {DialogMode} mode - Mode this dialog should open in.
 * @param {Function} onClose - Callback to perform when this modal is closed.
 * @param {FundKey} fundKey - Key of a Fund to populate the dialog, or null if the Fund is being created.
 */
interface FundDialogProps {
  isOpen: boolean;
  mode: DialogMode;
  onClose: () => void;
  fundKey?: FundKey | null;
}

/**
 * Component that presents the user with a dialog to create, view, update, or delete an Fund.
 * @param {AccountDialogProps} props - Props for the FundDialog component.
 * @returns {JSX.Element} JSX element representing the FundDialog.
 * @throws An error in the following cases:
 * 1) A key was expected but not provided
 * 2) The provided key didn't point to a valid Account.
 */
const FundDialog = function ({
  isOpen,
  mode,
  onClose,
  fundKey = null,
}: FundDialogProps): JSX.Element {
  // State for this component
  const [fund, setFund] = useState<Fund | null>(null);
  const [fundName, setFundName] = useState<string | null>(defaultFundName);
  const [fundDescription, setFundDescription] = useState<string | null>(
    defaultFundDescription,
  );

  // Event handlers for this component
  const handleClose = function (): void {
    setFund(null);
    setFundName(defaultFundName);
    setFundDescription(defaultFundDescription);
    onClose();
  };
  const handleAdd = function (): void {
    if (fundName === null) {
      throw new Error("Fund name cannot be blank");
    }
    addFund({
      name: fundName,
      description: fundDescription,
    }).catch(() => null);
    handleClose();
  };
  const handleUpdate = function (): void {
    if (fundKey === null) {
      throw new Error("Key must be defined");
    }
    updateFund(fundKey, { name: fundName, description: fundDescription });
    handleClose();
  };
  const handleDelete = function (): void {
    if (fundKey === null) {
      throw new Error("Key must be defined");
    }
    deleteFund(fundKey);
    handleClose();
  };

  // For View and Update modes, we must have an already existing Account so throw an error if the key wasn't provided.
  // Otherwise, find the account using the key and update the state.
  if (
    isOpen &&
    fund === null &&
    [DialogMode.View, DialogMode.Update].includes(mode)
  ) {
    if (fundKey === null) {
      throw new Error("Key must be defined");
    }
    const existingFund = getFundByKey(fundKey);
    if (existingFund === null) {
      throw new Error("Invalid key for account");
    }
    setFund(existingFund);
    setFundName(existingFund.name);
    setFundDescription(existingFund.description);
  }

  return (
    <Dialog open={isOpen}>
      <DialogTitle
        sx={{
          backgroundColor: "primary.main",
          color: "white",
          minWidth: "500px",
        }}
      >
        {mode.toString()} Account
      </DialogTitle>
      <DialogContent>
        <Stack
          direction="column"
          spacing={3}
          sx={{ paddingLeft: "25px", paddingRight: "25px", paddingTop: "25px" }}
        >
          {mode !== DialogMode.Delete ? (
            <>
              <StringEntryField
                dialogMode={mode}
                label="Name"
                setValue={setFundName}
                value={fundName}
              />
              <StringEntryField
                dialogMode={mode}
                label="Description"
                setValue={setFundDescription}
                value={fundDescription}
              />
            </>
          ) : (
            <Typography>
              Are you sure you want to delete this Account?
            </Typography>
          )}
        </Stack>
        <Stack direction="row" justifyContent="right">
          <Button onClick={handleClose}>Close</Button>
          {mode === DialogMode.Create ? (
            <Button onClick={handleAdd}>Add</Button>
          ) : null}
          {mode === DialogMode.Update ? (
            <Button onClick={handleUpdate}>Update</Button>
          ) : null}
          {mode === DialogMode.Delete ? (
            <Button onClick={handleDelete}>Delete</Button>
          ) : null}
        </Stack>
      </DialogContent>
    </Dialog>
  );
};

export default FundDialog;
