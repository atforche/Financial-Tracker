import {
  Button,
  Dialog,
  DialogContent,
  DialogTitle,
  Stack,
} from "@mui/material";
import { type Fund, addFund, updateFund } from "@data/FundRepository";
import StringEntryField from "@framework/dialog/StringEntryField";
import { useState } from "react";

/**
 * Props for the ModifyFundDialog component.
 * @param {Fund | null} fund - Fund to display in this dialog, or null if a Fund is being added.
 * @param {Function} onClose - Callback to perform when this dialog is closed.
 */
interface ModifyFundDialogProps {
  fund: Fund | null;
  onClose: () => void;
}

/**
 * Component that presents the user with a dialog to create or update a Fund.
 * @param {ModifyFundDialogProps} props - Props for the ModifyFundDialog component.
 * @returns {JSX.Element} JSX element representing the ModifyFundDialog.
 * @throws An error if the Fund ID is provided and doesn't point to a valid fund.
 */
const ModifyFundDialog = function ({
  fund,
  onClose,
}: ModifyFundDialogProps): JSX.Element {
  const [fundName, setFundName] = useState<string>(fund?.name ?? "");
  const [fundDescription, setFundDescription] = useState<string>(
    fund?.description ?? "",
  );

  return (
    <Dialog open={true}>
      <DialogTitle
        sx={{
          backgroundColor: "primary.main",
          color: "white",
          minWidth: "500px",
        }}
      >
        {fund === null ? "Add Fund" : "Edit Fund"}
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
              if (fund === null) {
                addFund({ name: fundName, description: fundDescription })
                  .then(() => {
                    onClose();
                  })
                  .catch(() => null);
              } else {
                updateFund(fund, {
                  name: fundName,
                  description: fundDescription,
                })
                  .then(() => {
                    onClose();
                  })
                  .catch(() => null);
              }
            }}
          >
            {fund === null ? "Add" : "Update"}
          </Button>
        </Stack>
      </DialogContent>
    </Dialog>
  );
};

export default ModifyFundDialog;
