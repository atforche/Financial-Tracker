import type { Account, AccountId } from "@data/AccountModels";
import {
  Button,
  Dialog,
  DialogContent,
  DialogTitle,
  Stack,
  Typography,
} from "@mui/material";
import {
  addAccount,
  deleteAccount,
  getAccountById,
  updateAccount,
} from "@data/AccountRepository";
import { useCallback, useState } from "react";
import AccountType from "@core/fieldValues/AccountType";
import BooleanEntryField from "@ui/framework/BooleanEntryField";
import DialogMode from "@core/fieldValues/DialogMode";
import FieldValueEntryField from "@ui/framework/FieldValueEntryField";
import StringEntryField from "@ui/framework/StringEntryField";
import useData from "@data/useData";

const defaultAccountName = "";
const defaultAccountType = AccountType.Standard;
const defaultIsAccountActive = true;

/**
 * Props for the AccountDialog component.
 * @param {boolean} isOpen - True if this modal should be open, false otherwise.
 * @param {DialogMode} mode - Mode this dialog should open in.
 * @param {Function} onClose - Callback to perform when this modal is closed.
 * @param {AccountId} accountId - ID of an Account to populate the dialog, or null if an Account is being created.
 */
interface AccountDialogProps {
  isOpen: boolean;
  mode: DialogMode;
  onClose: () => void;
  accountId?: AccountId | null;
}

/**
 * Component that presents the user with a dialog to create, view, update, or delete an Account.
 * @param {AccountDialogProps} props - Props for the AccountDialog component.
 * @returns {JSX.Element} JSX element representing the AccountDialog.
 * @throws An error in the following cases:
 * 1) A key was expected but not provided
 * 2) The provided key didn't point to a valid Account.
 */
const AccountDialog = function ({
  isOpen,
  mode,
  onClose,
  accountId = null,
}: AccountDialogProps): JSX.Element {
  // State for this component
  const [isFound, setIsFound] = useState(false);
  const [accountName, setAccountName] = useState<string | null>(
    defaultAccountName,
  );
  const [accountType, setAccountType] =
    useState<AccountType>(defaultAccountType);
  const [isAccountActive, setIsAccountActive] = useState<boolean>(
    defaultIsAccountActive,
  );

  // Event handlers for this component
  const handleClose = function (): void {
    setIsFound(false);
    setAccountName(defaultAccountName);
    setAccountType(defaultAccountType);
    setIsAccountActive(defaultIsAccountActive);
    onClose();
  };
  const handleAdd = function (): void {
    if (accountName === null) {
      throw new Error("Account name cannot be blank");
    }
    addAccount({
      name: accountName,
      type: accountType,
      isActive: isAccountActive,
    }).catch(() => {
      setIsFound(false);
    });
    handleClose();
  };
  const handleUpdate = function (): void {
    if (accountId === null) {
      throw new Error("Key must be defined");
    }
    updateAccount(accountId, {
      name: accountName,
      isActive: isAccountActive,
    }).catch(() => {
      setIsFound(false);
    });
    handleClose();
  };
  const handleDelete = function (): void {
    if (accountId === null) {
      throw new Error("Key must be defined");
    }
    deleteAccount(accountId).catch(() => {
      setIsFound(false);
    });
    handleClose();
  };

  // For View and Update modes, we must have an already existing Account so throw an error if the key wasn't provided.
  // Otherwise, find the account using the key and update the state.
  const fetchCallback = useCallback(
    async (): Promise<Account | null> =>
      accountId !== null ? getAccountById(accountId) : Promise.resolve(null),
    [accountId],
  );
  // TODO - add updateStateCallback to handle updating the state once the fetch is complete
  const existingAccount = useData(fetchCallback);

  if (!isFound && existingAccount !== null) {
    setIsFound(true);
    setAccountName(existingAccount.name);
    setAccountType(existingAccount.type);
    setIsAccountActive(existingAccount.isActive);
  }

  return (
    <Dialog open={isOpen}>
      <DialogTitle
        sx={{
          backgroundColor: "primary.main",
          color: "white",
          minWidth: "500px",
        }}
      >
        {mode.toString()} Account
      </DialogTitle>
      <DialogContent>
        <Stack
          direction="column"
          spacing={3}
          sx={{ paddingLeft: "25px", paddingRight: "25px", paddingTop: "25px" }}
        >
          {mode !== DialogMode.Delete ? (
            <>
              <StringEntryField
                dialogMode={mode}
                label="Account Name"
                setValue={setAccountName}
                value={accountName}
              />
              <FieldValueEntryField<AccountType>
                dialogMode={mode}
                label="Account Type"
                setValue={setAccountType}
                value={accountType}
                fieldValueCollection={AccountType.Collection}
                isReadOnly={() => mode !== DialogMode.Create}
              />
              <BooleanEntryField
                dialogMode={mode}
                label="Is Active?"
                setValue={setIsAccountActive}
                value={isAccountActive}
              />
            </>
          ) : (
            <Typography>
              Are you sure you want to delete this Account?
            </Typography>
          )}
        </Stack>
        <Stack direction="row" justifyContent="right">
          <Button onClick={handleClose}>Close</Button>
          {mode === DialogMode.Create ? (
            <Button onClick={handleAdd}>Add</Button>
          ) : null}
          {mode === DialogMode.Update ? (
            <Button onClick={handleUpdate}>Update</Button>
          ) : null}
          {mode === DialogMode.Delete ? (
            <Button onClick={handleDelete}>Delete</Button>
          ) : null}
        </Stack>
      </DialogContent>
    </Dialog>
  );
};

export default AccountDialog;
