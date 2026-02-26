import { type JSX, useState } from "react";
import dayjs, { type Dayjs } from "dayjs";
import ApiErrorHandler from "@data/ApiErrorHandler";
import { Button } from "@mui/material";
import CreateOrUpdateTransactionFrame from "@transactions/CreateOrUpdateTransactionFrame";
import Dialog from "@framework/dialog/Dialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import type FundAmountEntryModel from "@funds/FundAmountEntryModel";
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
  const [debitFundAmounts, setDebitFundAmounts] = useState<
    FundAmountEntryModel[]
  >(transaction.debitAccount ? transaction.debitAccount.fundAmounts : []);
  const [creditFundAmounts, setCreditFundAmounts] = useState<
    FundAmountEntryModel[]
  >(transaction.creditAccount ? transaction.creditAccount.fundAmounts : []);
  const { isRunning, isSuccess, updatedTransaction, error, updateTransaction } =
    useUpdateTransaction({
      transaction,
      date,
      location,
      description,
      debitFundAmounts: debitFundAmounts.map((entryModel) => ({
        fundId: entryModel.fundId ?? "",
        fundName: entryModel.fundName ?? "",
        amount: entryModel.amount ?? 0,
      })),
      creditFundAmounts: creditFundAmounts.map((entryModel) => ({
        fundId: entryModel.fundId ?? "",
        fundName: entryModel.fundName ?? "",
        amount: entryModel.amount ?? 0,
      })),
    });
  if (isSuccess) {
    if (updatedTransaction) {
      setTransaction(updatedTransaction);
    }
    onClose(true);
  }
  const errorHandler = error === null ? null : new ApiErrorHandler(error);
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
