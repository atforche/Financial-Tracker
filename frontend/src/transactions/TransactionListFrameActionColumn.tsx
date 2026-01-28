import type { ApiError } from "@data/ApiError";
import ColumnButton from "@framework/listframe/ColumnButton";
import ColumnCell from "@framework/listframe/ColumnCell";
import { Delete } from "@mui/icons-material";
import DeleteTransactionDialog from "@transactions/DeleteTransactionDialog";
import type { JSX } from "react";
import type { Transaction } from "@transactions/ApiTypes";

/**
 * Props for the TransactionListFrameActionColumn component.
 */
interface TransactionListFrameActionColumnProps {
  readonly transaction: Transaction;
  readonly isLoading: boolean;
  readonly error: ApiError | null;
  readonly setDialog: (dialog: JSX.Element | null) => void;
  readonly setMessage: (message: string | null) => void;
  readonly refetch: () => void;
}

/**
 * Component that renders the action column for a Transaction in the TransactionListFrame.
 * @param props - Props for the TransactionListFrameActionColumn component.
 * @returns JSX element representing the TransactionListFrameActionColumn.
 */
const TransactionListFrameActionColumn = function ({
  transaction,
  isLoading,
  error,
  setDialog,
  setMessage,
  refetch,
}: TransactionListFrameActionColumnProps): JSX.Element {
  const onDelete = function (): void {
    setDialog(
      <DeleteTransactionDialog
        transaction={transaction}
        onClose={(success) => {
          setDialog(null);
          if (success) {
            setMessage("Transaction deleted successfully.");
          }
          refetch();
        }}
      />,
    );
    setMessage(null);
  };
  return (
    <ColumnCell
      key="actions"
      content={
        <ColumnButton label="Delete" icon={<Delete />} onClick={onDelete} />
      }
      align="right"
      isLoading={isLoading}
      isError={error !== null}
    />
  );
};

export default TransactionListFrameActionColumn;
