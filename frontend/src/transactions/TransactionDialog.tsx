import { Button, Stack } from "@mui/material";
import { Delete, Edit } from "@mui/icons-material";
import { type JSX, useState } from "react";
import {
  type Transaction,
  TransactionAccountType,
} from "@transactions/ApiTypes";
import DeleteTransactionDialog from "@transactions/DeleteTransactionDialog";
import Dialog from "@framework/dialog/Dialog";
import DialogHeaderButton from "@framework/dialog/DialogHeaderButton";
import TransactionAccountFrame from "@transactions/TransactionAccountFrame";
import TransactionDetailsFrame from "@transactions/TransactionDetailsFrame";
import TransactionFundFrame from "./TransactionFundFrame";

/**
 * Props for the TransactionDialog component.
 */
interface TransactionDialogProps {
  readonly transaction: Transaction;
  readonly setMessage: (message: string | null) => void;
  readonly onClose: (success: boolean) => void;
}

/**
 * Component that presents the user with a dialog to view a Transaction.
 * @param props - Props for the TransactionDialog component.
 * @returns JSX element representing the TransactionDialog.
 */
const TransactionDialog = function ({
  transaction,
  setMessage,
  onClose,
}: TransactionDialogProps): JSX.Element {
  const [childDialog, setChildDialog] = useState<JSX.Element | null>(null);

  const onDelete = function (): void {
    setChildDialog(
      <DeleteTransactionDialog
        transaction={transaction}
        onClose={(success) => {
          setChildDialog(null);
          if (success) {
            setMessage("Transaction deleted successfully.");
            onClose(success);
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
            onClose(false);
          }}
        >
          Close
        </Button>
      }
      headerActions={
        <Stack direction="row" spacing={2}>
          <Edit />
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
