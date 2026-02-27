import { Button, Stack } from "@mui/material";
import { Delete, Edit } from "@mui/icons-material";
import { type JSX, useState } from "react";
import type { Account } from "@accounts/ApiTypes";
import AccountTransactionListFrame from "@accounts/AccountTransactionListFrame";
import CaptionedFrame from "@framework/dialog/CaptionedFrame";
import CaptionedValue from "@framework/dialog/CaptionedValue";
import DeleteAccountDialog from "@accounts/DeleteAccountDialog";
import Dialog from "@framework/dialog/Dialog";
import DialogHeaderButton from "@framework/dialog/DialogHeaderButton";
import UpdateAccountDialog from "@accounts/UpdateAccountDialog";
import formatCurrency from "@framework/formatCurrency";

/**
 * Props for the AccountDialog component.
 */
interface AccountDialogProps {
  readonly inputAccount: Account;
  readonly setMessage: (message: string | null) => void;
  readonly onClose: (needsRefetch: boolean) => void;
}

/**
 * Component that presents the user with a dialog to view an Account.
 * @param props - Props for the AccountDialog component.
 * @returns JSX element representing the AccountDialog.
 */
const AccountDialog = function ({
  inputAccount,
  setMessage,
  onClose,
}: AccountDialogProps): JSX.Element {
  const [account, setAccount] = useState<Account>(inputAccount);
  const [needsRefetch, setNeedsRefetch] = useState<boolean>(false);
  const [childDialog, setChildDialog] = useState<JSX.Element | null>(null);

  const onEdit = function (): void {
    setChildDialog(
      <UpdateAccountDialog
        account={account}
        setAccount={setAccount}
        onClose={(success) => {
          setChildDialog(null);
          if (success) {
            setMessage("Account updated successfully.");
            setNeedsRefetch(true);
          }
        }}
      />,
    );
  };

  const onDelete = function (): void {
    setChildDialog(
      <DeleteAccountDialog
        account={account}
        onClose={(success) => {
          setChildDialog(null);
          if (success) {
            setMessage("Account deleted successfully.");
            onClose(true);
          }
        }}
      />,
    );
  };

  return (
    <Dialog
      title="Account Details"
      content={
        <>
          <CaptionedFrame caption="Details">
            <CaptionedValue caption="Name" value={account.name} />
            <CaptionedValue caption="Type" value={account.type} />
            <CaptionedValue
              caption="Posted Balance"
              value={formatCurrency(account.currentBalance.postedBalance)}
            />
            {account.currentBalance.availableToSpend !== null && (
              <CaptionedValue
                caption="Available to Spend"
                value={formatCurrency(account.currentBalance.availableToSpend)}
              />
            )}
          </CaptionedFrame>
          {account.currentBalance.fundBalances.length > 0 && (
            <CaptionedFrame caption="Posted Balance By Fund">
              {account.currentBalance.fundBalances.map((fundAmount) => (
                <CaptionedValue
                  key={fundAmount.fundName}
                  caption={fundAmount.fundName}
                  value={formatCurrency(fundAmount.amount)}
                />
              ))}
            </CaptionedFrame>
          )}
          <AccountTransactionListFrame account={account} />
          {childDialog}
        </>
      }
      actions={
        <Button
          onClick={() => {
            onClose(needsRefetch);
          }}
        >
          Close
        </Button>
      }
      headerActions={
        <Stack direction="row" spacing={2}>
          <DialogHeaderButton label="Edit" icon={<Edit />} onClick={onEdit} />
          <DialogHeaderButton
            label="Delete"
            icon={<Delete />}
            onClick={onDelete}
          />
        </Stack>
      }
    />
  );
};

export default AccountDialog;
