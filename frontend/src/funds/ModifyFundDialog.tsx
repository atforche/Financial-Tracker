import { type JSX, useState } from "react";
import { Button } from "@mui/material";
import Dialog from "@framework/dialog/Dialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import type { Fund } from "@funds/ApiTypes";
import StringEntryField from "@framework/dialog/StringEntryField";
import useModifyFund from "@funds/useModifyFund";

/**
 * Props for the ModifyFundDialog component.
 * @param fund - Fund to display in this dialog, or null if a Fund is being added.
 * @param onClose - Callback to perform when this dialog is closed.
 */
interface ModifyFundDialogProps {
  readonly fund: Fund | null;
  readonly onClose: (success: boolean) => void;
}

/**
 * Component that presents the user with a dialog to create or update a Fund.
 * @param props - Props for the ModifyFundDialog component.
 * @returns JSX element representing the ModifyFundDialog.
 */
const ModifyFundDialog = function ({
  fund,
  onClose,
}: ModifyFundDialogProps): JSX.Element {
  const [fundName, setFundName] = useState<string>(fund?.name ?? "");
  const [fundDescription, setFundDescription] = useState<string>(
    fund?.description ?? "",
  );
  const { isRunning, isSuccess, error, modifyFund } = useModifyFund({
    fund,
    fundName,
    fundDescription,
  });
  if (isSuccess) {
    onClose(true);
  }
  return (
    <Dialog
      title={fund === null ? "Add Fund" : "Edit Fund"}
      content={
        <>
          <StringEntryField
            label="Name"
            value={fundName}
            setValue={setFundName}
            error={
              error?.details.find(
                (detail) => detail.errorCode === "InvalidFundName",
              ) ?? null
            }
          />
          <StringEntryField
            label="Description"
            value={fundDescription}
            setValue={setFundDescription}
          />
          <ErrorAlert
            error={error}
            detailFilter={(detail) => detail.errorCode !== "InvalidFundName"}
          />
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
          <Button onClick={modifyFund} disabled={isRunning}>
            {fund === null ? "Add" : "Update"}
          </Button>
        </>
      }
    />
  );
};

export default ModifyFundDialog;
