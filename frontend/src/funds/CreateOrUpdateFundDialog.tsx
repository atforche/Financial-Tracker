import { type JSX, useState } from "react";
import { Button } from "@mui/material";
import Dialog from "@framework/dialog/Dialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import { ErrorCode } from "@data/api";
import type { Fund } from "@funds/ApiTypes";
import StringEntryField from "@framework/dialog/StringEntryField";
import useCreateOrUpdateFund from "@funds/useCreateOrUpdateFund";

/**
 * Props for the CreateOrUpdateFundDialog component.
 */
interface CreateOrUpdateFundDialogProps {
  readonly fund: Fund | null;
  readonly setFund: ((fund: Fund) => void) | null;
  readonly onClose: (success: boolean) => void;
}

/**
 * Component that presents the user with a dialog to create or update a Fund.
 * @param props - Props for the CreateOrUpdateFundDialog component.
 * @returns JSX element representing the CreateOrUpdateFundDialog.
 */
const CreateOrUpdateFundDialog = function ({
  fund,
  setFund,
  onClose,
}: CreateOrUpdateFundDialogProps): JSX.Element {
  const [fundName, setFundName] = useState<string>(fund?.name ?? "");
  const [fundDescription, setFundDescription] = useState<string>(
    fund?.description ?? "",
  );
  const { isRunning, isSuccess, createdOrUpdatedFund, error, modifyFund } =
    useCreateOrUpdateFund({
      fund,
      fundName,
      fundDescription,
    });
  if (isSuccess) {
    if (setFund && createdOrUpdatedFund) {
      setFund(createdOrUpdatedFund);
    }
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
                (detail) => detail.errorCode === ErrorCode.InvalidFundName,
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
            detailFilter={(detail) =>
              detail.errorCode !== ErrorCode.InvalidFundName
            }
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

export default CreateOrUpdateFundDialog;
