import {
  Button,
  Dialog,
  DialogContent,
  DialogTitle,
  Stack,
} from "@mui/material";
import { type Fund, addFund, getFundById } from "@data/FundRepository";
import { useCallback, useState } from "react";
import StringEntryField from "@framework/dialog/StringEntryField";
import { useQuery } from "@data/useQuery";

/**
 * Props for the ModifyFundDialog component.
 * @param {string | null} fundId - ID of a Fund to display in this dialog, or null if a Fund is being added.
 * @param {Function} onClose - Callback to perform when this dialog is closed.
 */
interface ModifyFundDialogProps {
  fundId: string | null;
  onClose: () => void;
}

/**
 * Component that presents the user with a dialog to create or update a Fund.
 * @param {ModifyFundDialogProps} props - Props for the ModifyFundDialog component.
 * @returns {JSX.Element} JSX element representing the ModifyFundDialog.
 * @throws An error if the Fund ID is provided and doesn't point to a valid fund.
 */
const ModifyFundDialog = function ({
  fundId,
  onClose,
}: ModifyFundDialogProps): JSX.Element {
  const [fundName, setFundName] = useState<string>("");
  const [fundDescription, setFundDescription] = useState<string>("");

  const fetchFund = useCallback(
    async () => (fundId !== null ? getFundById(fundId) : null),
    [fundId],
  );
  const { data } = useQuery<Fund | null>({
    queryFunction: fetchFund,
    initialData: null,
  });

  if (data !== null) {
    setFundName(data.name);
    setFundDescription(data.description);
  }

  return (
    <Dialog open={true}>
      <DialogTitle
        sx={{
          backgroundColor: "primary.main",
          color: "white",
          minWidth: "500px",
        }}
      >
        {fundId === null ? "Add Fund" : "Edit Fund"}
      </DialogTitle>
      <DialogContent>
        <Stack
          direction="column"
          spacing={3}
          sx={{ paddingLeft: "25px", paddingRight: "25px", paddingTop: "25px" }}
        >
          <>
            <StringEntryField
              label="Name"
              value={fundName}
              setValue={setFundName}
            />
            <StringEntryField
              label="Description"
              value={fundDescription}
              setValue={setFundDescription}
            />
          </>
        </Stack>
        <Stack direction="row" justifyContent="right">
          <Button onClick={onClose}>Cancel</Button>
          <Button
            onClick={() => {
              addFund({ name: fundName, description: fundDescription }).catch(
                () => null,
              );
              onClose();
            }}
          >
            Add
          </Button>
        </Stack>
      </DialogContent>
    </Dialog>
  );
};

export default ModifyFundDialog;
