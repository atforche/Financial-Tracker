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
 */
interface UpdateAccountDialogProps {
  readonly account: Account;
  readonly setAccount: (account: Account) => void;
  readonly onClose: (success: boolean) => void;
}

/**
 * Component that presents the user with a dialog to update an Account.
 * @param props - Props for the UpdateAccountDialog component.
 * @returns JSX element representing the UpdateAccountDialog.
 */
const UpdateAccountDialog = function ({
  account,
  setAccount,
  onClose,
}: UpdateAccountDialogProps): JSX.Element {
  const [accountName, setAccountName] = useState<string>(account.name);
  const { isRunning, isSuccess, updatedAccount, error, updateAccount } =
    useUpdateAccount({
      account,
      name: accountName,
    });
  if (isSuccess) {
    if (updatedAccount) {
      setAccount(updatedAccount);
    }
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
