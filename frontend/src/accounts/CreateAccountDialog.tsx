import { type JSX, useState } from "react";
import { AccountType } from "@accounts/ApiTypes";
import AccountTypeEntryField from "@accounts/AccountTypeEntryField";
import type { AccountingPeriod } from "@accounting-periods/ApiTypes";
import { Button } from "@mui/material";
import DateEntryField from "@framework/dialog/DateEntryField";
import type { Dayjs } from "dayjs";
import Dialog from "@framework/dialog/Dialog";
import type { FundAmount } from "@funds/ApiTypes";
import FundAmountEntryField from "@funds/FundAmountEntryField";
import OpenAccountingPeriodEntryField from "@accounting-periods/OpenAccountingPeriodEntryField";
import StringEntryField from "@framework/dialog/StringEntryField";
import useCreateAccount from "@accounts/useCreateAccount";

/**
 * Props for the CreateAccountDialog component.
 * @param onClose - Callback to perform when this dialog is closed.
 */
interface CreateAccountDialogProps {
  readonly onClose: (success: boolean) => void;
}

/**
 * Component that presents the user with a dialog to create an Account.
 * @param props - Props for the CreateAccountDialog component.
 * @returns JSX element representing the CreateAccountDialog.
 */
const CreateAccountDialog = function ({
  onClose,
}: CreateAccountDialogProps): JSX.Element {
  const [accountName, setAccountName] = useState<string>("");
  const [accountType, setAccountType] = useState<AccountType | null>(
    AccountType.Standard,
  );
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(null);
  const [addDate, setAddDate] = useState<Dayjs | null>(null);
  const [fundAmount, setFundAmount] = useState<FundAmount | null>(null);
  const { isRunning, isSuccess, createAccount } = useCreateAccount({
    name: accountName,
    type: accountType,
    accountingPeriod,
    addDate,
    initialFundAmounts: fundAmount ? [fundAmount] : [],
  });
  if (isSuccess) {
    onClose(true);
  }
  return (
    <Dialog
      title="Add Account"
      content={
        <>
          <StringEntryField
            label="Name"
            value={accountName}
            setValue={setAccountName}
          />
          <AccountTypeEntryField
            label="Type"
            value={accountType}
            setValue={setAccountType}
          />
          <OpenAccountingPeriodEntryField
            label="Initial Accounting Period"
            value={accountingPeriod}
            setValue={setAccountingPeriod}
          />
          <DateEntryField
            label="Date Opened"
            value={addDate}
            setValue={setAddDate}
          />
          <FundAmountEntryField
            label="Opening Balance"
            value={fundAmount}
            setValue={setFundAmount}
          />
        </>
      }
      actions={
        <>
          <Button
            onClick={() => {
              onClose(false);
            }}
            disabled={isRunning}
          >
            Cancel
          </Button>
          <Button onClick={createAccount} disabled={isRunning}>
            Add
          </Button>
        </>
      }
    />
  );
};

export default CreateAccountDialog;
