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
  readonly transaction: Transaction;
  readonly setTransaction: (transaction: Transaction) => void;
  readonly onClose: (success: boolean) => void;
}

/**
 * Component that presents the user with a dialog to update a Transaction.
 * @param props - Props for the UpdateTransactionDialog component.
 * @returns JSX element representing the UpdateTransactionDialog.
 */
const UpdateTransactionDialog = function ({
  transaction,
  setTransaction,
  onClose,
}: UpdateTransactionDialogProps): JSX.Element {
  const [date, setDate] = useState<Dayjs | null>(
    dayjs(transaction.date, "YYYY-MM-DD"),
  );
  const [location, setLocation] = useState<string>(transaction.location);
  const [description, setDescription] = useState<string>(
    transaction.description,
  );
  const [debitFundAmounts, setDebitFundAmounts] = useState<FundAmount[]>(
    transaction.debitAccount ? transaction.debitAccount.fundAmounts : [],
  );
  const [creditFundAmounts, setCreditFundAmounts] = useState<FundAmount[]>(
    transaction.creditAccount ? transaction.creditAccount.fundAmounts : [],
  );
  const { isRunning, isSuccess, updatedTransaction, error, updateTransaction } =
    useUpdateTransaction({
      transaction,
      date,
      location,
      description,
      debitFundAmounts,
      creditFundAmounts,
    });
  if (isSuccess) {
    if (updatedTransaction) {
      setTransaction(updatedTransaction);
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
              id: transaction.accountingPeriodId,
              name: transaction.accountingPeriodName,
            }}
            setAccountingPeriod={null}
            date={date}
            setDate={setDate}
            location={location}
            setLocation={setLocation}
            description={description}
            setDescription={setDescription}
            debitAccount={
              transaction.debitAccount
                ? {
                    id: transaction.debitAccount.accountId,
                    name: transaction.debitAccount.accountName,
                  }
                : null
            }
            setDebitAccount={null}
            debitFundAmounts={debitFundAmounts}
            setDebitFundAmounts={
              transaction.debitAccount ? setDebitFundAmounts : null
            }
            creditAccount={
              transaction.creditAccount
                ? {
                    id: transaction.creditAccount.accountId,
                    name: transaction.creditAccount.accountName,
                  }
                : null
            }
            setCreditAccount={null}
            creditFundAmounts={creditFundAmounts}
            setCreditFundAmounts={
              transaction.creditAccount ? setCreditFundAmounts : null
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
              ((transaction.debitAccount === null ||
                debitFundAmounts.length === 0) &&
                (transaction.creditAccount === null ||
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
