import { Button, Stack } from "@mui/material";
import { Delete, Edit, Signpost } from "@mui/icons-material";
import { type JSX, useState } from "react";
import {
  type Transaction,
  TransactionAccountType,
} from "@transactions/ApiTypes";
import DeleteTransactionDialog from "@transactions/DeleteTransactionDialog";
import Dialog from "@framework/dialog/Dialog";
import DialogHeaderButton from "@framework/dialog/DialogHeaderButton";
import PostTransactionDialog from "@transactions/PostTransactionDialog";
import TransactionAccountFrame from "@transactions/TransactionAccountFrame";
import TransactionDetailsFrame from "@transactions/TransactionDetailsFrame";
import TransactionFundFrame from "@transactions/TransactionFundFrame";
import UpdateTransactionDialog from "@transactions/UpdateTransactionDialog";

/**
 * Props for the TransactionDialog component.
 */
interface TransactionDialogProps {
  readonly inputTransaction: Transaction;
  readonly setMessage: (message: string | null) => void;
  readonly onClose: (needsRefetch: boolean) => void;
}

/**
 * Component that presents the user with a dialog to view a Transaction.
 * @param props - Props for the TransactionDialog component.
 * @returns JSX element representing the TransactionDialog.
 */
const TransactionDialog = function ({
  inputTransaction,
  setMessage,
  onClose,
}: TransactionDialogProps): JSX.Element {
  const [transaction, setTransaction] = useState<Transaction>(inputTransaction);
  const [needsRefetch, setNeedsRefetch] = useState<boolean>(false);
  const [childDialog, setChildDialog] = useState<JSX.Element | null>(null);

  const onEdit = function (): void {
    setChildDialog(
      <UpdateTransactionDialog
        transaction={transaction}
        setTransaction={setTransaction}
        onClose={(success) => {
          setChildDialog(null);
          if (success) {
            setMessage("Transaction updated successfully.");
            setNeedsRefetch(true);
          }
        }}
      />,
    );
  };

  const onPost = function (): void {
    setChildDialog(
      <PostTransactionDialog
        transaction={transaction}
        setTransaction={setTransaction}
        onClose={(success) => {
          setChildDialog(null);
          if (success) {
            setMessage("Transaction posted successfully.");
            setNeedsRefetch(true);
          }
        }}
      />,
    );
  };

  const onDelete = function (): void {
    setChildDialog(
      <DeleteTransactionDialog
        transaction={transaction}
        onClose={(success) => {
          setChildDialog(null);
          if (success) {
            setMessage("Transaction deleted successfully.");
            onClose(true);
          }
        }}
      />,
    );
    setMessage(null);
  };

  return (
    <Dialog
      title="Transaction Details"
      content={
        <>
          <TransactionDetailsFrame transaction={transaction} />
          <Stack direction="row" spacing={2}>
            {transaction.debitAccount ? (
              <TransactionAccountFrame
                transactionAccount={transaction.debitAccount}
                transactionAccountType={TransactionAccountType.Debit}
              />
            ) : null}
            {transaction.creditAccount ? (
              <TransactionAccountFrame
                transactionAccount={transaction.creditAccount}
                transactionAccountType={TransactionAccountType.Credit}
              />
            ) : null}
          </Stack>
          <TransactionFundFrame transaction={transaction} />
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
            label="Post"
            icon={<Signpost />}
            onClick={onPost}
          />
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

export default TransactionDialog;
