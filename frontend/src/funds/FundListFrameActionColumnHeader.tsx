import { AddCircleOutline } from "@mui/icons-material";
import ColumnHeader from "@framework/listframe/ColumnHeader";
import ColumnHeaderButton from "@framework/listframe/ColumnHeaderButton";
import type { JSX } from "react";
import ModifyFundDialog from "@funds/ModifyFundDialog";

/**
 * Props for the FundListFrameActionColumnHeader component.
 * @param setDialog - Function to set the current dialog.
 * @param setMessage - Function to set the current message.
 * @param refetch - Function to refetch the list of funds.
 */
interface FundListFrameActionColumnHeaderProps {
  readonly setDialog: (dialog: JSX.Element | null) => void;
  readonly setMessage: (message: string | null) => void;
  readonly refetch: () => void;
}

/**
 * Component for the action column header in the fund list frame.
 * @param props - Props for the FundListFrameActionColumnHeader component.
 * @returns JSX element representing the FundListFrameActionColumnHeader.
 */
const FundListFrameActionColumnHeader = function ({
  setDialog,
  setMessage,
  refetch,
}: FundListFrameActionColumnHeaderProps): JSX.Element {
  const onAdd = function (): void {
    setDialog(
      <ModifyFundDialog
        fund={null}
        onClose={(success) => {
          setDialog(null);
          if (success) {
            setMessage("Fund added successfully.");
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

export default FundListFrameActionColumnHeader;
