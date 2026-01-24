import { type JSX, useState } from "react";
import type { Account } from "@accounts/ApiTypes";
import { Button } from "@mui/material";
import Dialog from "@framework/dialog/Dialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import { ErrorCode } from "@data/api";
import StringEntryField from "@framework/dialog/StringEntryField";
import useUpdateAccount from "@accounts/useUpdateAccount";

/**
 * Props for the UpdateAccountDialog component.
 * @param account - Account to display in this dialog.
 * @param onClose - Callback to perform when this dialog is closed.
 */
interface UpdateAccountDialogProps {
  readonly account: Account;
  readonly onClose: (success: boolean) => void;
}

/**
 * Component that presents the user with a dialog to update an Account.
 * @param props - Props for the UpdateAccountDialog component.
 * @returns JSX element representing the UpdateAccountDialog.
 */
const UpdateAccountDialog = function ({
  account,
  onClose,
}: UpdateAccountDialogProps): JSX.Element {
  const [accountName, setAccountName] = useState<string>(account.name);
  const { isRunning, isSuccess, error, updateAccount } = useUpdateAccount({
    account,
    name: accountName,
  });
  if (isSuccess) {
    onClose(true);
  }
  return (
    <Dialog
      title="Edit Account"
      content={
        <>
          <StringEntryField
            label="Name"
            value={accountName}
            setValue={setAccountName}
            error={
              error?.details.find(
                (detail) => detail.errorCode === ErrorCode.InvalidFundName,
              ) ?? null
            }
          />
          <StringEntryField label="Type" value={account.type} />
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
          <Button onClick={updateAccount} disabled={isRunning}>
            Update
          </Button>
        </>
      }
    />
  );
};

export default UpdateAccountDialog;
