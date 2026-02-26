import type { Account, UpdateAccountRequest } from "@accounts/ApiTypes";
import { type JSX, useState } from "react";
import ApiErrorHandler from "@data/ApiErrorHandler";
import { Button } from "@mui/material";
import Dialog from "@framework/dialog/Dialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import StringEntryField from "@framework/dialog/StringEntryField";
import nameof from "@data/nameof";
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
  const errorHandler = error ? new ApiErrorHandler(error) : null;
  return (
    <Dialog
      title="Edit Account"
      content={
        <>
          <StringEntryField
            label="Name"
            value={accountName}
            setValue={setAccountName}
            errorHandler={errorHandler}
            errorKey={nameof<UpdateAccountRequest>("name")}
          />
          <ErrorAlert errorHandler={errorHandler} />
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
