import {
  ComboBoxEntryField,
  type ComboBoxOption,
} from "@framework/dialog/ComboBoxEntryField";
import { type JSX, useState } from "react";
import dayjs, { type Dayjs } from "dayjs";
import type { AccountIdentifier } from "@accounts/ApiTypes";
import ApiErrorHandler from "@data/ApiErrorHandler";
import { Button } from "@mui/material";
import DateEntryField from "@framework/dialog/DateEntryField";
import Dialog from "@framework/dialog/Dialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import type { Transaction } from "@transactions/ApiTypes";
import usePostTransaction from "@transactions/usePostTransaction";

/**
 * Props for the PostTransactionDialog component.
 */
interface PostTransactionDialogProps {
  readonly transaction: Transaction;
  readonly setTransaction: (transaction: Transaction) => void;
  readonly onClose: (success: boolean) => void;
}

/**
 * Gets the account options for posting the transaction.
 * @param transaction - The transaction to get the account options for.
 * @returns An array of ComboBoxOption<AccountIdentifier> representing the account options for posting the transaction.
 */
const getAccountOptions = function (
  transaction: Transaction,
): ComboBoxOption<AccountIdentifier>[] {
  const options: ComboBoxOption<AccountIdentifier>[] = [];
  if (transaction.debitAccount) {
    options.push({
      label: transaction.debitAccount.accountName,
      value: {
        id: transaction.debitAccount.accountId,
        name: transaction.debitAccount.accountName,
      },
    });
  }
  if (
    transaction.creditAccount &&
    transaction.creditAccount.accountId !== transaction.debitAccount?.accountId
  ) {
    options.push({
      label: transaction.creditAccount.accountName,
      value: {
        id: transaction.creditAccount.accountId,
        name: transaction.creditAccount.accountName,
      },
    });
  }
  return options;
};

/**
 * Component that presents the user with a dialog to post a Transaction.
 * @param props - Props for the PostTransactionDialog component.
 * @returns JSX element representing the PostTransactionDialog.
 */
const PostTransactionDialog = function ({
  transaction,
  setTransaction,
  onClose,
}: PostTransactionDialogProps): JSX.Element {
  const [account, setAccount] = useState<AccountIdentifier | null>(
    getAccountOptions(transaction).length === 1
      ? (getAccountOptions(transaction)[0]?.value ?? null)
      : null,
  );
  const [date, setDate] = useState<Dayjs | null>(null);
  const defaultDate = dayjs(transaction.accountingPeriodName, "MMMM YYYY");

  const { isRunning, isSuccess, updatedTransaction, error, postTransaction } =
    usePostTransaction({
      transaction,
      account,
      date: date ?? defaultDate,
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
      title="Post Transaction"
      content={
        <>
          <ComboBoxEntryField<AccountIdentifier>
            label="Account"
            options={getAccountOptions(transaction)}
            value={
              account === null
                ? { label: "", value: null }
                : { label: account.name, value: account }
            }
            setValue={(newValue) => {
              setAccount(newValue.value);
            }}
          />
          <DateEntryField
            label="Posted Date"
            value={date ?? defaultDate}
            setValue={setDate}
            minDate={dayjs(transaction.accountingPeriodName, "MMMM YYYY")
              .subtract(1, "month")
              .startOf("month")}
            maxDate={dayjs(transaction.accountingPeriodName, "MMMM YYYY")
              .add(2, "month")
              .subtract(1, "day")}
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
            onClick={postTransaction}
            disabled={isRunning || account === null}
            variant="contained"
            sx={{ margin: "15px" }}
          >
            Post
          </Button>
        </>
      }
    />
  );
};

export default PostTransactionDialog;
