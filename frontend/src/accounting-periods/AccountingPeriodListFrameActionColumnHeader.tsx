import { AddCircleOutline } from "@mui/icons-material";
import ColumnHeader from "@framework/listframe/ColumnHeader";
import ColumnHeaderButton from "@framework/listframe/ColumnHeaderButton";
import CreateAccountingPeriodDialog from "@accounting-periods/CreateAccountingPeriodDialog";
import type { JSX } from "react";

/**
 * Props for the AccountingPeriodListFrameActionColumnHeader component.
 * @param setDialog - Function to set the current dialog.
 * @param setMessage - Function to set the current message.
 * @param refetch - Function to refetch the list of Accounting Periods.
 */
interface AccountingPeriodListFrameActionColumnHeaderProps {
  readonly setDialog: (dialog: JSX.Element | null) => void;
  readonly setMessage: (message: string | null) => void;
  readonly refetch: () => void;
}

/**
 * Component for the action column header in the Accounting Period list frame.
 * @param props - Props for the AccountingPeriodListFrameActionColumnHeader component.
 * @returns JSX element representing the AccountingPeriodListFrameActionColumnHeader.
 */
const AccountingPeriodListFrameActionColumnHeader = function ({
  setDialog,
  setMessage,
  refetch,
}: AccountingPeriodListFrameActionColumnHeaderProps): JSX.Element {
  const onAdd = function (): void {
    setDialog(
      <CreateAccountingPeriodDialog
        onClose={(success) => {
          setDialog(null);
          if (success) {
            setMessage("Accounting Period added successfully.");
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

export default AccountingPeriodListFrameActionColumnHeader;
