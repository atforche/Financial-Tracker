import type { AccountType, CreateAccountRequest } from "@accounts/ApiTypes";
import { type JSX, useState } from "react";
import dayjs, { type Dayjs } from "dayjs";
import AccountTypeEntryField from "@accounts/AccountTypeEntryField";
import type { AccountingPeriodIdentifier } from "@accounting-periods/ApiTypes";
import ApiErrorHandler from "@data/ApiErrorHandler";
import { Button } from "@mui/material";
import DateEntryField from "@framework/dialog/DateEntryField";
import Dialog from "@framework/dialog/Dialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import FundAmountCollectionEntryFrame from "@funds/FundAmountCollectionEntryFrame";
import type FundAmountEntryModel from "@funds/FundAmountEntryModel";
import OpenAccountingPeriodEntryField from "@accounting-periods/OpenAccountingPeriodEntryField";
import StringEntryField from "@framework/dialog/StringEntryField";
import nameof from "@data/nameof";
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
  const [fundAmounts, setFundAmounts] = useState<FundAmountEntryModel[]>([]);
  const { isRunning, isSuccess, error, createAccount } = useCreateAccount({
    name: accountName,
    type: accountType,
    accountingPeriod,
    addDate,
    initialFundAmounts: fundAmounts.map((entryModel) => ({
      fundId: entryModel.fundId ?? "",
      fundName: entryModel.fundName ?? "",
      amount: entryModel.amount ?? 0,
    })),
  });
  if (isSuccess) {
    onClose(true);
  }
  const errorHandler = error ? new ApiErrorHandler(error) : null;
  return (
    <Dialog
      title="Add Account"
      content={
        <>
          <StringEntryField
            label="Name"
            value={accountName}
            setValue={setAccountName}
            errorHandler={errorHandler}
            errorKey={nameof<CreateAccountRequest>("name")}
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
            errorHandler={errorHandler}
            errorKey={nameof<CreateAccountRequest>("accountingPeriodId")}
          />
          <DateEntryField
            label="Date Opened"
            value={addDate}
            setValue={setAddDate}
            errorHandler={errorHandler}
            errorKey={nameof<CreateAccountRequest>("addDate")}
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
          <ErrorAlert errorHandler={errorHandler} />
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
              addDate === null
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
