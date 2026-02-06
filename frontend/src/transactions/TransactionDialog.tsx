import { Button, Stack } from "@mui/material";
import { Delete, Edit } from "@mui/icons-material";
import {
  type Transaction,
  TransactionAccountType,
} from "@transactions/ApiTypes";
import Dialog from "@framework/dialog/Dialog";
import type { JSX } from "react";
import TransactionAccountFrame from "@transactions/TransactionAccountFrame";
import TransactionDetailsFrame from "@transactions/TransactionDetailsFrame";
import TransactionFundFrame from "./TransactionFundFrame";

/**
 * Props for the TransactionDialog component.
 */
interface TransactionDialogProps {
  readonly transaction: Transaction;
  readonly onClose: (success: boolean) => void;
}

/**
 * Component that presents the user with a dialog to view a Transaction.
 * @param props - Props for the TransactionDialog component.
 * @returns JSX element representing the TransactionDialog.
 */
const TransactionDialog = function ({
  transaction,
  onClose,
}: TransactionDialogProps): JSX.Element {
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
          <Delete />
        </Stack>
      }
    />
  );
};

export default TransactionDialog;
