import { Balance, Delete, Info } from "@mui/icons-material";
import type { AccountingPeriod } from "@accounting-periods/ApiTypes";
import AccountingPeriodDialog from "@accounting-periods/AccountingPeriodDialog";
import type { ApiError } from "@data/ApiError";
import CloseAccountingPeriodDialog from "@accounting-periods/CloseAccountingPeriodDialog";
import ColumnButton from "@framework/listframe/ColumnButton";
import ColumnCell from "@framework/listframe/ColumnCell";
import DeleteAccountingPeriodDialog from "@accounting-periods/DeleteAccountingPeriodDialog";
import type { JSX } from "react";

/**
 * Props for the AccountingPeriodListFrameActionColumn component.
 * @param accountingPeriod - The Accounting Period associated with the action column.
 * @param isLoading - Indicates if the data is currently loading.
 * @param error - The API error, if any.
 * @param setDialog - Function to set the dialog element.
 * @param setMessage - Function to set the message string.
 * @param refetch - Function to refetch the list of Accounting Periods.
 */
interface AccountingPeriodListFrameActionColumnProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly isLoading: boolean;
  readonly error: ApiError | null;
  readonly setDialog: (dialog: JSX.Element | null) => void;
  readonly setMessage: (message: string | null) => void;
  readonly refetch: () => void;
}

/**
 * Component that renders the action column for an Accounting Period in the AccountingPeriodListFrame.
 * @param props - Props for the AccountingPeriodListFrameActionColumn component.
 * @returns JSX element representing the AccountingPeriodListFrameActionColumn.
 */
const AccountingPeriodListFrameActionColumn = function ({
  accountingPeriod,
  isLoading,
  error,
  setDialog,
  setMessage,
  refetch,
}: AccountingPeriodListFrameActionColumnProps): JSX.Element {
  const onView = function (): void {
    setDialog(
      <AccountingPeriodDialog
        accountingPeriod={accountingPeriod}
        onClose={() => {
          setDialog(null);
        }}
      />,
    );
    setMessage(null);
  };
  const onClose = function (): void {
    setDialog(
      <CloseAccountingPeriodDialog
        accountingPeriod={accountingPeriod}
        onClose={(success) => {
          setDialog(null);
          if (success) {
            setMessage("Accounting Period closed successfully.");
          }
          refetch();
        }}
      />,
    );
    setMessage(null);
  };
  const onDelete = function (): void {
    setDialog(
      <DeleteAccountingPeriodDialog
        accountingPeriod={accountingPeriod}
        onClose={(success) => {
          setDialog(null);
          if (success) {
            setMessage("Accounting Period deleted successfully.");
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
        <>
          <ColumnButton label="View" icon={<Info />} onClick={onView} />
          {accountingPeriod.isOpen ? (
            <ColumnButton label="Close" icon={<Balance />} onClick={onClose} />
          ) : null}
          <ColumnButton label="Delete" icon={<Delete />} onClick={onDelete} />
        </>
      }
      align="right"
      isLoading={isLoading}
      isError={error !== null}
    />
  );
};

export default AccountingPeriodListFrameActionColumn;
