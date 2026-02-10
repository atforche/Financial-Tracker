import { type JSX, useState } from "react";
import dayjs, { type Dayjs } from "dayjs";
import type { AccountType } from "@accounts/ApiTypes";
import AccountTypeEntryField from "@accounts/AccountTypeEntryField";
import type { AccountingPeriodIdentifier } from "@accounting-periods/ApiTypes";
import { Button } from "@mui/material";
import DateEntryField from "@framework/dialog/DateEntryField";
import Dialog from "@framework/dialog/Dialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import { ErrorCode } from "@data/api";
import type { FundAmount } from "@funds/ApiTypes";
import FundAmountCollectionEntryFrame from "@funds/FundAmountCollectionEntryFrame";
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
  const [accountType, setAccountType] = useState<AccountType | null>(null);
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriodIdentifier | null>(null);
  const [addDate, setAddDate] = useState<Dayjs | null>(null);
  const [fundAmounts, setFundAmounts] = useState<FundAmount[]>([]);
  const { isRunning, isSuccess, error, createAccount } = useCreateAccount({
    name: accountName,
    type: accountType,
    accountingPeriod,
    addDate,
    initialFundAmounts: fundAmounts,
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
            error={
              error?.details.find(
                (detail) => detail.errorCode === ErrorCode.InvalidAccountName,
              ) ?? null
            }
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
            error={
              error?.details.find(
                (detail) =>
                  detail.errorCode === ErrorCode.InvalidAccountingPeriod,
              ) ?? null
            }
          />
          <DateEntryField
            label="Date Opened"
            value={addDate}
            setValue={setAddDate}
            error={
              error?.details.find(
                (detail) => detail.errorCode === ErrorCode.InvalidEventDate,
              ) ?? null
            }
            minDate={
              accountingPeriod
                ? dayjs(accountingPeriod.name, "MMMM YYYY").subtract(1, "month")
                : null
            }
            maxDate={
              accountingPeriod
                ? dayjs(accountingPeriod.name, "MMMM YYYY")
                    .add(2, "month")
                    .subtract(1, "day")
                : null
            }
          />
          <FundAmountCollectionEntryFrame
            label="Opening Balance"
            value={fundAmounts}
            setValue={setFundAmounts}
          />
          <ErrorAlert
            error={error}
            detailFilter={(detail) =>
              detail.errorCode !== ErrorCode.InvalidAccountName &&
              detail.errorCode !== ErrorCode.InvalidAccountingPeriod &&
              detail.errorCode !== ErrorCode.InvalidEventDate
            }
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
          <Button
            onClick={createAccount}
            disabled={
              isRunning ||
              accountName.trim() === "" ||
              accountType === null ||
              accountingPeriod === null ||
              addDate === null ||
              fundAmounts.length === 0
            }
            variant="contained"
            sx={{ margin: "15px" }}
          >
            Add
          </Button>
        </>
      }
    />
  );
};

export default CreateAccountDialog;
