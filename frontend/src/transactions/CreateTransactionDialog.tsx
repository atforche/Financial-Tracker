import { Button, Stack } from "@mui/material";
import { type JSX, useState } from "react";
import dayjs, { type Dayjs } from "dayjs";
import type { Account } from "@accounts/ApiTypes";
import AccountEntryField from "@accounts/AccountEntryField";
import type { AccountingPeriod } from "@accounting-periods/ApiTypes";
import DateEntryField from "@framework/dialog/DateEntryField";
import Dialog from "@framework/dialog/Dialog";
import type { FundAmount } from "@funds/ApiTypes";
import FundAmountCollectionEntryFrame from "@funds/FundAmountCollectionEntryFrame";
import OpenAccountingPeriodEntryField from "@accounting-periods/OpenAccountingPeriodEntryField";
import StringEntryField from "@framework/dialog/StringEntryField";
import useCreateTransaction from "@transactions/useCreateTransaction";

/**
 * Props for the CreateTransactionDialog component.
 */
interface CreateTransactionDialogProps {
  readonly onClose: (success: boolean) => void;
}

/**
 * Component that presents the user with a dialog to create a Transaction.
 * @param props - Props for the CreateTransactionDialog component.
 * @returns JSX element representing the CreateTransactionDialog.
 */
const CreateTransactionDialog = function ({
  onClose,
}: CreateTransactionDialogProps): JSX.Element {
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(null);
  const [date, setDate] = useState<Dayjs | null>(null);
  const [location, setLocation] = useState<string>("");
  const [description, setDescription] = useState<string>("");
  const [debitAccount, setDebitAccount] = useState<Account | null>(null);
  const [debitFundAmounts, setDebitFundAmounts] = useState<FundAmount[]>([]);
  const [creditAccount, setCreditAccount] = useState<Account | null>(null);
  const [creditFundAmounts, setCreditFundAmounts] = useState<FundAmount[]>([]);
  const { isRunning, isSuccess, createTransaction } = useCreateTransaction({
    accountingPeriod,
    date,
    location,
    description,
    debitAccount,
    debitFundAmounts,
    creditAccount,
    creditFundAmounts,
  });
  if (isSuccess) {
    onClose(true);
  }
  return (
    <Dialog
      title="Add Transaction"
      content={
        <>
          <OpenAccountingPeriodEntryField
            label="Accounting Period"
            value={accountingPeriod}
            setValue={setAccountingPeriod}
          />
          <DateEntryField
            label="Date"
            value={date}
            setValue={setDate}
            minDate={
              accountingPeriod
                ? dayjs(
                    `${accountingPeriod.year}-${accountingPeriod.month}-01`,
                  ).subtract(1, "month")
                : null
            }
            maxDate={
              accountingPeriod
                ? dayjs(`${accountingPeriod.year}-${accountingPeriod.month}-01`)
                    .add(2, "month")
                    .subtract(1, "day")
                : null
            }
          />
          <StringEntryField
            label="Location"
            value={location}
            setValue={setLocation}
          />
          <StringEntryField
            label="Description"
            value={description}
            setValue={setDescription}
          />
          <Stack direction="row" spacing={2}>
            <Stack spacing={2}>
              <AccountEntryField
                label="Debit Account"
                value={debitAccount}
                setValue={setDebitAccount}
              />
              <FundAmountCollectionEntryFrame
                label="Debit Fund Amounts"
                value={debitFundAmounts}
                setValue={setDebitFundAmounts}
              />
            </Stack>
            <Stack spacing={2}>
              <AccountEntryField
                label="Credit Account"
                value={creditAccount}
                setValue={setCreditAccount}
              />
              <FundAmountCollectionEntryFrame
                label="Credit Fund Amounts"
                value={creditFundAmounts}
                setValue={setCreditFundAmounts}
              />
            </Stack>
          </Stack>
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
            onClick={createTransaction}
            disabled={
              isRunning ||
              accountingPeriod === null ||
              date === null ||
              location.trim() === "" ||
              description.trim() === "" ||
              ((debitAccount === null || debitFundAmounts.length === 0) &&
                (creditAccount === null || creditFundAmounts.length === 0))
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

export default CreateTransactionDialog;
