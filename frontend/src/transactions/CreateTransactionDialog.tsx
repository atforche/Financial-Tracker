import { type JSX, useState } from "react";
import type { AccountIdentifier } from "@accounts/ApiTypes";
import type { AccountingPeriodIdentifier } from "@accounting-periods/ApiTypes";
import { Button } from "@mui/material";
import CreateOrUpdateTransactionFrame from "@transactions/CreateOrUpdateTransactionFrame";
import type { Dayjs } from "dayjs";
import Dialog from "@framework/dialog/Dialog";
import type { FundAmount } from "@funds/ApiTypes";
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
  const [location, setLocation] = useState<string>("");
  const [description, setDescription] = useState<string>("");
  const [debitAccount, setDebitAccount] = useState<AccountIdentifier | null>(
    null,
  );
  const [debitFundAmounts, setDebitFundAmounts] = useState<FundAmount[]>([]);
  const [creditAccount, setCreditAccount] = useState<AccountIdentifier | null>(
    null,
  );
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
        <CreateOrUpdateTransactionFrame
          accountingPeriod={accountingPeriod}
          setAccountingPeriod={setAccountingPeriod}
          date={date}
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
