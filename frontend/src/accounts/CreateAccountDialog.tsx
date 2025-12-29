import { type JSX, useState } from "react";
import { AccountType } from "@accounts/ApiTypes";
import { Button } from "@mui/material";
import ComboBoxEntryField from "@framework/dialog/ComboBoxEntryField";
import Dialog from "@framework/dialog/Dialog";
import StringEntryField from "@framework/dialog/StringEntryField";

/**
 * Props for the CreateAccountDialog component.
 * @param onClose - Callback to perform when this dialog is closed.
 */
interface CreateAccountDialogProps {
  readonly onClose: (success: boolean) => void;
}

/**
 * Component that presents the user with a dialog to create an Account.
 * @param props - Props for the CreateAccountDialog component.
 * @returns JSX element representing the CreateAccountDialog.
 */
const CreateAccountDialog = function ({
  onClose,
}: CreateAccountDialogProps): JSX.Element {
  const [accountName, setAccountName] = useState<string>("");
  const [accountType, setAccountType] = useState<AccountType>(
    AccountType.Standard,
  );

  return (
    <Dialog
      title="Add Account"
      content={
        <>
          <StringEntryField
            label="Name"
            value={accountName}
            setValue={setAccountName}
          />
          <ComboBoxEntryField<AccountType>
            label="Type"
            options={Object.values(AccountType)}
            getOptionLabel={(option: AccountType) => option}
            value={accountType}
            setValue={setAccountType}
          />
        </>
      }
      actions={
        <>
          <Button
            onClick={() => {
              onClose(false);
            }}
            disabled={false}
          >
            Cancel
          </Button>
          <Button disabled={false}>Add</Button>
        </>
      }
    />
  );
};

export default CreateAccountDialog;
