import type { AccountingPeriod } from "@accounting-periods/ApiTypes";
import BooleanEntryField from "@framework/dialog/BooleanEntryField";
import { Button } from "@mui/material";
import Dialog from "@framework/dialog/Dialog";
import IntegerEntryField from "@framework/dialog/IntegerEntryField";
import type { JSX } from "react";

/**
 * Props for the AccountingPeriodDialog component.
 * @param accountingPeriod - Accounting Period to display in this dialog.
 * @param onClose - Callback to perform when this dialog is closed.
 */
interface AccountingPeriodDialogProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly onClose: () => void;
}

/**
 * Component that presents the user with a dialog to view an Accounting Period.
 * @param props - Props for the AccountingPeriodDialog component.
 * @returns JSX element representing the AccountingPeriodDialog.
 */
const AccountingPeriodDialog = function ({
  accountingPeriod,
  onClose,
}: AccountingPeriodDialogProps): JSX.Element {
  return (
    <Dialog
      title="View Accounting Period"
      content={
        <>
          <IntegerEntryField label="Year" value={accountingPeriod.year} />
          <IntegerEntryField label="Month" value={accountingPeriod.month} />
          <BooleanEntryField label="Is Open" value={accountingPeriod.isOpen} />
        </>
      }
      actions={<Button onClick={onClose}>Close</Button>}
    />
  );
};

export default AccountingPeriodDialog;
