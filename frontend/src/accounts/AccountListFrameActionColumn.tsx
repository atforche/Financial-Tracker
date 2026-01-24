import { Delete, Info, ModeEdit } from "@mui/icons-material";
import type { Account } from "@accounts/ApiTypes";
import AccountDialog from "@accounts/AccountDialog";
import type { ApiError } from "@data/ApiError";
import ColumnButton from "@framework/listframe/ColumnButton";
import ColumnCell from "@framework/listframe/ColumnCell";
import DeleteAccountDialog from "@accounts/DeleteAccountDialog";
import type { JSX } from "react";
import UpdateAccountDialog from "@accounts/UpdateAccountDialog";

/**
 * Props for the AccountPeriodListFrameActionColumn component.
 * @param account - The Account associated with the action column.
 * @param isLoading - Indicates if the data is currently loading.
 * @param error - The API error, if any.
 * @param setDialog - Function to set the dialog element.
 * @param setMessage - Function to set the message string.
 * @param refetch - Function to refetch the list of Accounting Periods.
 */
interface AccountListFrameActionColumnProps {
  readonly account: Account;
  readonly isLoading: boolean;
  readonly error: ApiError | null;
  readonly setDialog: (dialog: JSX.Element | null) => void;
  readonly setMessage: (message: string | null) => void;
  readonly refetch: () => void;
}

/**
 * Component that renders the action column for an Account in the AccountListFrame.
 * @param props - Props for the AccountListFrameActionColumn component.
 * @returns JSX element representing the AccountListFrameActionColumn.
 */
const AccountListFrameActionColumn = function ({
  account,
  isLoading,
  error,
  setDialog,
  setMessage,
  refetch,
}: AccountListFrameActionColumnProps): JSX.Element {
  const onView = function (): void {
    setDialog(
      <AccountDialog
        account={account}
        onClose={() => {
          setDialog(null);
        }}
      />,
    );
    setMessage(null);
  };
  const onEdit = function (): void {
    setDialog(
      <UpdateAccountDialog
        account={account}
        onClose={(success) => {
          setDialog(null);
          if (success) {
            setMessage("Account updated successfully.");
          }
          refetch();
        }}
      />,
    );
    setMessage(null);
  };
  const onDelete = function (): void {
    setDialog(
      <DeleteAccountDialog
        account={account}
        onClose={(success) => {
          setDialog(null);
          if (success) {
            setMessage("Account deleted successfully.");
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
          <ColumnButton label="Edit" icon={<ModeEdit />} onClick={onEdit} />
          <ColumnButton label="Delete" icon={<Delete />} onClick={onDelete} />
        </>
      }
      align="right"
      isLoading={isLoading}
      isError={error !== null}
    />
  );
};

export default AccountListFrameActionColumn;
