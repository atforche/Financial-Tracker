import { AddCircleOutline } from "@mui/icons-material";
import ColumnHeader from "@framework/listframe/ColumnHeader";
import ColumnHeaderButton from "@framework/listframe/ColumnHeaderButton";
import CreateAccountDialog from "@accounts/CreateAccountDialog";
import type { JSX } from "react";

/**
 * Props for the AccountListFrameActionColumnHeader component.
 * @param setDialog - Function to set the current dialog.
 * @param setMessage - Function to set the current message.
 * @param refetch - Function to refetch the list of Accounts.
 */
interface AccountListFrameActionColumnHeaderProps {
  readonly setDialog: (dialog: JSX.Element | null) => void;
  readonly setMessage: (message: string | null) => void;
  readonly refetch: () => void;
}

/**
 * Component for the action column header in the Account list frame.
 * @param props - Props for the AccountListFrameActionColumnHeader component.
 * @returns JSX element representing the AccountListFrameActionColumnHeader.
 */
const AccountListFrameActionColumnHeader = function ({
  setDialog,
  setMessage,
  refetch,
}: AccountListFrameActionColumnHeaderProps): JSX.Element {
  const onAdd = function (): void {
    setDialog(
      <CreateAccountDialog
        onClose={(success) => {
          setDialog(null);
          if (success) {
            setMessage("Account added successfully.");
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

export default AccountListFrameActionColumnHeader;
