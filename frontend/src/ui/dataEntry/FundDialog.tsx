import {
  Button,
  Dialog,
  DialogContent,
  DialogTitle,
  Stack,
} from "@mui/material";
import { type Fund, addFund, getFundById } from "@data/FundRepository";
import { useCallback, useState } from "react";
import DialogMode from "@core/fieldValues/DialogMode";
import StringEntryField from "@ui/framework/dialog/StringEntryField";
import { useQuery } from "@data/useQuery";

const defaultFundName = null;
const defaultFundDescription = null;

/**
 * Props for the FundDialog component.
 * @param {boolean} isOpen - True if this modal should be open, false otherwise.
 * @param {DialogMode} mode - Mode this dialog should open in.
 * @param {Function} onClose - Callback to perform when this modal is closed.
 * @param {string | null} fundId - ID of a Fund to populate the dialog, or null if the Fund is being created.
 */
interface FundDialogProps {
  isOpen: boolean;
  mode: DialogMode;
  onClose: () => void;
  fundId?: string | null;
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
  fundId = null,
}: FundDialogProps): JSX.Element {
  // State for this component
  const [fund, setFund] = useState<Fund | null>(null);
  const [fundName, setFundName] = useState<string | null>(defaultFundName);
  const [fundDescription, setFundDescription] = useState<string | null>(
    defaultFundDescription,
  );

  const fetchFund = useCallback(async () => {
    if (fundId !== null) {
      return getFundById(fundId);
    }
    return null;
  }, [fundId]);

  const { data } = useQuery<Fund | null>({
    queryFunction: fetchFund,
    initialData: null,
  });

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
      description: fundDescription ?? "",
    }).catch(() => null);
    handleClose();
  };

  // For View and Update modes, we must have an already existing Account so throw an error if the key wasn't provided.
  // Otherwise, find the account using the key and update the state.
  if (
    isOpen &&
    fund === null &&
    [DialogMode.View, DialogMode.Update].includes(mode) &&
    data !== null
  ) {
    setFund(data);
    setFundName(data.name);
    setFundDescription(data.description);
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
          <>
            <StringEntryField
              dialogMode={mode}
              label="Name"
              setValue={setFundName}
              value={fundName ?? ""}
            />
            <StringEntryField
              dialogMode={mode}
              label="Description"
              setValue={setFundDescription}
              value={fundDescription ?? ""}
            />
          </>
        </Stack>
        <Stack direction="row" justifyContent="right">
          <Button onClick={handleClose}>Close</Button>
          {mode === DialogMode.Create ? (
            <Button onClick={handleAdd}>Add</Button>
          ) : null}
        </Stack>
      </DialogContent>
    </Dialog>
  );
};

export default FundDialog;
