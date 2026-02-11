import { Balance, Delete } from "@mui/icons-material";
import { Button, Stack } from "@mui/material";
import { type JSX, useState } from "react";
import type { AccountingPeriod } from "@accounting-periods/ApiTypes";
import CaptionedFrame from "@framework/dialog/CaptionedFrame";
import CaptionedValue from "@framework/dialog/CaptionedValue";
import CloseAccountingPeriodDialog from "@accounting-periods/CloseAccountingPeriodDialog";
import DeleteAccountingPeriodDialog from "@accounting-periods/DeleteAccountingPeriodDialog";
import Dialog from "@framework/dialog/Dialog";
import DialogHeaderButton from "@framework/dialog/DialogHeaderButton";

/**
 * Props for the AccountingPeriodDialog component.
 */
interface AccountingPeriodDialogProps {
  readonly inputAccountingPeriod: AccountingPeriod;
  readonly setMessage: (message: string | null) => void;
  readonly onClose: (needsRefetch: boolean) => void;
}

/**
 * Component that presents the user with a dialog to view an Accounting Period.
 * @param props - Props for the AccountingPeriodDialog component.
 * @returns JSX element representing the AccountingPeriodDialog.
 */
const AccountingPeriodDialog = function ({
  inputAccountingPeriod,
  setMessage,
  onClose,
}: AccountingPeriodDialogProps): JSX.Element {
  const [accountingPeriod, setAccountingPeriod] = useState<AccountingPeriod>(
    inputAccountingPeriod,
  );
  const [needsRefetch, setNeedsRefetch] = useState<boolean>(false);
  const [childDialog, setChildDialog] = useState<JSX.Element | null>(null);

  const closeAccountingPeriod = function (): void {
    setChildDialog(
      <CloseAccountingPeriodDialog
        accountingPeriod={accountingPeriod}
        setAccountingPeriod={setAccountingPeriod}
        onClose={(success) => {
          setChildDialog(null);
          if (success) {
            setMessage("Accounting Period closed successfully.");
            setNeedsRefetch(true);
          }
        }}
      />,
    );
  };

  const onDelete = function (): void {
    setChildDialog(
      <DeleteAccountingPeriodDialog
        accountingPeriod={accountingPeriod}
        onClose={(success) => {
          setChildDialog(null);
          if (success) {
            setMessage("Accounting Period deleted successfully.");
            onClose(true);
          }
        }}
      />,
    );
  };

  return (
    <Dialog
      title="Accounting Period Details"
      content={
        <>
          <CaptionedFrame caption="Details">
            <CaptionedValue caption="Name" value={accountingPeriod.name} />
            <CaptionedValue
              caption="Is Open"
              value={accountingPeriod.isOpen ? "Yes" : "No"}
            />
          </CaptionedFrame>
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
          <DialogHeaderButton
            label="Close"
            icon={<Balance />}
            onClick={closeAccountingPeriod}
          />
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

export default AccountingPeriodDialog;
