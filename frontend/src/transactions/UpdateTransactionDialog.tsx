import { type JSX, useState } from "react";
import dayjs, { type Dayjs } from "dayjs";
import { Button } from "@mui/material";
import CreateOrUpdateTransactionFrame from "@transactions/CreateOrUpdateTransactionFrame";
import Dialog from "@framework/dialog/Dialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import type { FundAmount } from "@funds/ApiTypes";
import type { Transaction } from "@transactions/ApiTypes";
import useUpdateTransaction from "@transactions/useUpdateTransaction";

/**
 * Props for the UpdateTransactionDialog component.
 */
interface UpdateTransactionDialogProps {
  readonly existingTransaction: Transaction;
  readonly setExistingTransaction: (transaction: Transaction) => void;
  readonly onClose: (success: boolean) => void;
}

/**
 * Component that presents the user with a dialog to update a Transaction.
 * @param props - Props for the UpdateTransactionDialog component.
 * @returns JSX element representing the UpdateTransactionDialog.
 */
const UpdateTransactionDialog = function ({
  existingTransaction,
  setExistingTransaction,
  onClose,
}: UpdateTransactionDialogProps): JSX.Element {
  const [date, setDate] = useState<Dayjs | null>(
    dayjs(existingTransaction.date, "YYYY-MM-DD"),
  );
  const [location, setLocation] = useState<string>(
    existingTransaction.location,
  );
  const [description, setDescription] = useState<string>(
    existingTransaction.description,
  );
  const [debitFundAmounts, setDebitFundAmounts] = useState<FundAmount[]>(
    existingTransaction.debitAccount
      ? existingTransaction.debitAccount.fundAmounts
      : [],
  );
  const [creditFundAmounts, setCreditFundAmounts] = useState<FundAmount[]>(
    existingTransaction.creditAccount
      ? existingTransaction.creditAccount.fundAmounts
      : [],
  );
  const { isRunning, isSuccess, updatedTransaction, error, updateTransaction } =
    useUpdateTransaction({
      existingTransaction,
      date,
      location,
      description,
      debitFundAmounts,
      creditFundAmounts,
    });
  if (isSuccess) {
    if (updatedTransaction) {
      setExistingTransaction(updatedTransaction);
    }
    onClose(true);
  }
  return (
    <Dialog
      title="Update Transaction"
      content={
        <>
          <CreateOrUpdateTransactionFrame
            accountingPeriod={{
              id: existingTransaction.accountingPeriodId,
              name: existingTransaction.accountingPeriodName,
            }}
            setAccountingPeriod={null}
            date={date}
            setDate={setDate}
            location={location}
            setLocation={setLocation}
            description={description}
            setDescription={setDescription}
            debitAccount={
              existingTransaction.debitAccount
                ? {
                    id: existingTransaction.debitAccount.accountId,
                    name: existingTransaction.debitAccount.accountName,
                  }
                : null
            }
            setDebitAccount={null}
            debitFundAmounts={debitFundAmounts}
            setDebitFundAmounts={
              existingTransaction.debitAccount ? setDebitFundAmounts : null
            }
            creditAccount={
              existingTransaction.creditAccount
                ? {
                    id: existingTransaction.creditAccount.accountId,
                    name: existingTransaction.creditAccount.accountName,
                  }
                : null
            }
            setCreditAccount={null}
            creditFundAmounts={creditFundAmounts}
            setCreditFundAmounts={
              existingTransaction.creditAccount ? setCreditFundAmounts : null
            }
          />
          <ErrorAlert error={error} />
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
            onClick={updateTransaction}
            disabled={
              isRunning ||
              date === null ||
              location.trim() === "" ||
              description.trim() === "" ||
              ((existingTransaction.debitAccount === null ||
                debitFundAmounts.length === 0) &&
                (existingTransaction.creditAccount === null ||
                  creditFundAmounts.length === 0))
            }
            variant="contained"
            sx={{ margin: "15px" }}
          >
            Update
          </Button>
        </>
      }
    />
  );
};

export default UpdateTransactionDialog;
