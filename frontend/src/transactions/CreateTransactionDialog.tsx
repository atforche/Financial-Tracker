import { type JSX, useState } from "react";
import dayjs, { type Dayjs } from "dayjs";
import type { AccountIdentifier } from "@accounts/ApiTypes";
import type { AccountingPeriodIdentifier } from "@accounting-periods/ApiTypes";
import { Button } from "@mui/material";
import CreateOrUpdateTransactionFrame from "@transactions/CreateOrUpdateTransactionFrame";
import Dialog from "@framework/dialog/Dialog";
import type FundAmountEntryModel from "@funds/FundAmountEntryModel";
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
    useState<AccountingPeriodIdentifier | null>(null);
  const [date, setDate] = useState<Dayjs | null>(null);
  const defaultDate = accountingPeriod
    ? dayjs(accountingPeriod.name, "MMMM YYYY")
    : null;
  const [location, setLocation] = useState<string>("");
  const [description, setDescription] = useState<string>("");
  const [debitAccount, setDebitAccount] = useState<AccountIdentifier | null>(
    null,
  );
  const [debitFundAmounts, setDebitFundAmounts] = useState<
    FundAmountEntryModel[]
  >([]);
  const [creditAccount, setCreditAccount] = useState<AccountIdentifier | null>(
    null,
  );
  const [creditFundAmounts, setCreditFundAmounts] = useState<
    FundAmountEntryModel[]
  >([]);
  const { isRunning, isSuccess, createTransaction } = useCreateTransaction({
    accountingPeriod,
    date: date ?? defaultDate,
    location,
    description,
    debitAccount,
    debitFundAmounts: debitFundAmounts.map((entryModel) => ({
      fundId: entryModel.fundId ?? "",
      fundName: entryModel.fundName ?? "",
      amount: entryModel.amount ?? 0,
    })),
    creditAccount,
    creditFundAmounts: creditFundAmounts.map((entryModel) => ({
      fundId: entryModel.fundId ?? "",
      fundName: entryModel.fundName ?? "",
      amount: entryModel.amount ?? 0,
    })),
  });
  if (isSuccess) {
    onClose(true);
  }
  return (
    <Dialog
      title="Add Transaction"
      content={
        <CreateOrUpdateTransactionFrame
          accountingPeriod={accountingPeriod}
          setAccountingPeriod={setAccountingPeriod}
          date={date ?? defaultDate}
          setDate={setDate}
          location={location}
          setLocation={setLocation}
          description={description}
          setDescription={setDescription}
          debitAccount={debitAccount}
          setDebitAccount={setDebitAccount}
          debitFundAmounts={debitFundAmounts}
          setDebitFundAmounts={setDebitFundAmounts}
          creditAccount={creditAccount}
          setCreditAccount={setCreditAccount}
          creditFundAmounts={creditFundAmounts}
          setCreditFundAmounts={setCreditFundAmounts}
        />
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
              (date === null && defaultDate === null) ||
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
