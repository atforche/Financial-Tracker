import { Button, Stack } from "@mui/material";
import { Delete, Edit } from "@mui/icons-material";
import { type JSX, useState } from "react";
import CaptionedFrame from "@framework/dialog/CaptionedFrame";
import CaptionedValue from "@framework/dialog/CaptionedValue";
import CreateOrUpdateFundDialog from "@funds/CreateOrUpdateFundDialog";
import DeleteFundDialog from "@funds/DeleteFundDialog";
import Dialog from "@framework/dialog/Dialog";
import DialogHeaderButton from "@framework/dialog/DialogHeaderButton";
import type { Fund } from "@funds/ApiTypes";
import FundTransactionListFrame from "@funds/FundTransactionListFrame";
import formatCurrency from "@framework/formatCurrency";

/**
 * Props for the FundDialog component.
 */
interface FundDialogProps {
  readonly inputFund: Fund;
  readonly setMessage: (message: string | null) => void;
  readonly onClose: (needsRefetch: boolean) => void;
}

/**
 * Component that presents the user with a dialog to view a Fund.
 * @param props - Props for the FundDialog component.
 * @returns JSX element representing the FundDialog.
 */
const FundDialog = function ({
  inputFund,
  setMessage,
  onClose,
}: FundDialogProps): JSX.Element {
  const [fund, setFund] = useState<Fund>(inputFund);
  const [needsRefetch, setNeedsRefetch] = useState<boolean>(false);
  const [childDialog, setChildDialog] = useState<JSX.Element | null>(null);

  const onEdit = function (): void {
    setChildDialog(
      <CreateOrUpdateFundDialog
        fund={fund}
        setFund={setFund}
        onClose={(success) => {
          setChildDialog(null);
          if (success) {
            setMessage("Fund updated successfully.");
            setNeedsRefetch(true);
          }
        }}
      />,
    );
  };

  const onDelete = function (): void {
    setChildDialog(
      <DeleteFundDialog
        fund={fund}
        onClose={(success) => {
          setChildDialog(null);
          if (success) {
            setMessage("Fund deleted successfully.");
            onClose(true);
          }
        }}
      />,
    );
  };

  return (
    <Dialog
      title="Fund Details"
      content={
        <>
          <CaptionedFrame caption="Details">
            <CaptionedValue caption="Name" value={fund.name} />
            <CaptionedValue caption="Description" value={fund.description} />
            <CaptionedValue
              caption="Posted Balance"
              value={formatCurrency(fund.currentBalance.postedBalance)}
            />
          </CaptionedFrame>
          <CaptionedFrame caption="Posted Balance By Account">
            {fund.currentBalance.accountBalances.map((accountAmount) => (
              <CaptionedValue
                key={accountAmount.accountName}
                caption={accountAmount.accountName}
                value={formatCurrency(accountAmount.amount)}
              />
            ))}
          </CaptionedFrame>
          <FundTransactionListFrame fund={fund} />
          {childDialog}
        </>
      }
      actions={
        <Button
          onClick={() => {
            onClose(needsRefetch);
          }}
        >
          Close
        </Button>
      }
      headerActions={
        <Stack direction="row" spacing={2}>
          <DialogHeaderButton label="Edit" icon={<Edit />} onClick={onEdit} />
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

export default FundDialog;
