import { Delete, Info, ModeEdit } from "@mui/icons-material";
import type { ApiError } from "@data/ApiError";
import ColumnCell from "@framework/listframe/ColumnCell";
import DeleteFundDialog from "@funds/DeleteFundDialog";
import type { Fund } from "@funds/ApiTypes";
import FundDialog from "@funds/FundDialog";
import { IconButton } from "@mui/material";
import type { JSX } from "react";
import ModifyFundDialog from "@funds/ModifyFundDialog";

/**
 * Props for the FundListFrameActionColumn component.
 * @param fund - The fund associated with the action column.
 * @param isLoading - Indicates if the data is currently loading.
 * @param error - The API error, if any.
 * @param setDialog - Function to set the dialog element.
 * @param setMessage - Function to set the message string.
 * @param refetch - Function to refetch the list of funds.
 */
interface FundListFrameActionColumnProps {
  readonly fund: Fund;
  readonly isLoading: boolean;
  readonly error: ApiError | null;
  readonly setDialog: (dialog: JSX.Element | null) => void;
  readonly setMessage: (message: string | null) => void;
  readonly refetch: () => void;
}

/**
 * Component that renders the action column for a fund in the FundListFrame.
 * @param props - Props for the FundListFrameActionColumn component.
 * @returns JSX element representing the FundListFrameActionColumn.
 */
const FundListFrameActionColumn = function ({
  fund,
  isLoading,
  error,
  setDialog,
  setMessage,
  refetch,
}: FundListFrameActionColumnProps): JSX.Element {
  const onView = function (): void {
    setDialog(
      <FundDialog
        fund={fund}
        onClose={() => {
          setDialog(null);
        }}
      />,
    );
    setMessage(null);
  };
  const onEdit = function (): void {
    setDialog(
      <ModifyFundDialog
        fund={fund}
        onClose={(success) => {
          setDialog(null);
          if (success) {
            setMessage("Fund updated successfully.");
          }
          refetch();
        }}
      />,
    );
    setMessage(null);
  };
  const onDelete = function (): void {
    setDialog(
      <DeleteFundDialog
        fund={fund}
        onClose={(success) => {
          setDialog(null);
          if (success) {
            setMessage("Fund deleted successfully.");
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
          <IconButton
            key="view"
            onClick={(): void => {
              onView();
            }}
          >
            <Info />
          </IconButton>
          <IconButton
            key="edit"
            onClick={(): void => {
              onEdit();
            }}
          >
            <ModeEdit />
          </IconButton>
          <IconButton
            key="delete"
            onClick={(): void => {
              onDelete();
            }}
          >
            <Delete />
          </IconButton>
        </>
      }
      align="right"
      isLoading={isLoading}
      isError={error !== null}
    />
  );
};

export default FundListFrameActionColumn;
