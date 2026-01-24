import type { Account } from "@accounts/ApiTypes";
import { Button } from "@mui/material";
import Dialog from "@framework/dialog/Dialog";
import type { JSX } from "react";
import StringEntryField from "@framework/dialog/StringEntryField";

/**
 * Props for the AccountDialog component.
 * @param account - Account to display in this dialog.
 * @param onClose - Callback to perform when this dialog is closed.
 */
interface AccountDialogProps {
  readonly account: Account;
  readonly onClose: () => void;
}

/**
 * Component that presents the user with a dialog to view an Account.
 * @param props - Props for the AccountDialog component.
 * @returns JSX element representing the AccountDialog.
 */
const AccountDialog = function ({
  account,
  onClose,
}: AccountDialogProps): JSX.Element {
  return (
    <Dialog
      title="View Account"
      content={
        <>
          <StringEntryField label="Name" value={account.name} />
          <StringEntryField label="Type" value={account.type} />
        </>
      }
      actions={<Button onClick={onClose}>Close</Button>}
    />
  );
};

export default AccountDialog;
