import { AddCircleOutline } from "@mui/icons-material";
import ColumnHeader from "@framework/listframe/ColumnHeader";
import ColumnHeaderButton from "@framework/listframe/ColumnHeaderButton";
import CreateTransactionDialog from "@transactions/CreateTransactionDialog";
import type { JSX } from "react";

/**
 * Props for the TransactionListFrameActionColumnHeader component.
 */
interface TransactionListFrameActionColumnHeaderProps {
  readonly setDialog: (dialog: JSX.Element | null) => void;
  readonly setMessage: (message: string | null) => void;
  readonly refetch: () => void;
}

/**
 * Component for the action column header in the Transaction list frame.
 * @param props - Props for the TransactionListFrameActionColumnHeader component.
 * @returns JSX element representing the TransactionListFrameActionColumnHeader.
 */
const TransactionListFrameActionColumnHeader = function ({
  setDialog,
  setMessage,
  refetch,
}: TransactionListFrameActionColumnHeaderProps): JSX.Element {
  const onAdd = function (): void {
    setDialog(
      <CreateTransactionDialog
        onClose={(success) => {
          setDialog(null);
          if (success) {
            setMessage("Transaction added successfully.");
          }
          refetch();
        }}
      />,
    );
    setMessage(null);
  };
  return (
    <ColumnHeader
      key="actions"
      content={
        <ColumnHeaderButton
          label="Add"
          icon={<AddCircleOutline />}
          onClick={onAdd}
        />
      }
      minWidth={125}
      align="right"
    />
  );
};

export default TransactionListFrameActionColumnHeader;
