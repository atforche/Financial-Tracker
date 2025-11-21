import { type ApiError, isApiError } from "@data/ApiError";
import {
  Button,
  Dialog,
  DialogContent,
  DialogTitle,
  Stack,
} from "@mui/material";
import { type Fund, addFund, updateFund } from "@data/FundRepository";
import { type JSX, useCallback, useState } from "react";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import StringEntryField from "@framework/dialog/StringEntryField";
import { useApiRequest } from "@data/useApiRequest";

/**
 * Props for the ModifyFundDialog component.
 * @param {Fund | null} fund - Fund to display in this dialog, or null if a Fund is being added.
 * @param {(boolean) => void} onClose - Callback to perform when this dialog is closed.
 */
interface ModifyFundDialogProps {
  readonly fund: Fund | null;
  readonly onClose: (success: boolean) => void;
}

/**
 * Component that presents the user with a dialog to create or update a Fund.
 * @param {ModifyFundDialogProps} props - Props for the ModifyFundDialog component.
 * @returns {JSX.Element} JSX element representing the ModifyFundDialog.
 */
const ModifyFundDialog = function ({
  fund,
  onClose,
}: ModifyFundDialogProps): JSX.Element {
  const [fundName, setFundName] = useState<string>(fund?.name ?? "");
  const [fundDescription, setFundDescription] = useState<string>(
    fund?.description ?? "",
  );

  const apiRequestFunction = useCallback<
    () => Promise<ApiError | null>
  >(async () => {
    if (fund === null) {
      return addFund({ name: fundName, description: fundDescription }).then(
        (result) => (isApiError(result) ? result : null),
      );
    }
    return updateFund(fund, {
      name: fundName,
      description: fundDescription,
    }).then((result) => (isApiError(result) ? result : null));
  }, [fund, fundName, fundDescription]);

  const { isRunning, isSuccess, error, execute } = useApiRequest({
    apiRequestFunction,
  });

  const onCancel = useCallback((): void => {
    onClose(false);
  }, [onClose]);

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
          </>
        </Stack>
        <Stack direction="row" justifyContent="right">
          <Button onClick={onCancel} disabled={isRunning}>
            Cancel
          </Button>
          <Button onClick={execute} loading={isRunning}>
            {fund === null ? "Add" : "Update"}
          </Button>
        </Stack>
      </DialogContent>
      <ErrorAlert error={error} />
    </Dialog>
  );
};

export default ModifyFundDialog;
