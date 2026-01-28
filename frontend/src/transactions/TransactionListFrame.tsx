import { type JSX, useState } from "react";
import ColumnCell from "@framework/listframe/ColumnCell";
import ColumnHeader from "@framework/listframe/ColumnHeader";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import ListFrame from "@framework/listframe/ListFrame";
import SuccessAlert from "@framework/alerts/SuccessAlert";
import type { Transaction } from "@transactions/ApiTypes";
import TransactionListFrameActionColumn from "@transactions/TransactionListFrameActionColumn";
import TransactionListFrameActionColumnHeader from "@transactions/TransactionListFrameActionColumnHeader";
import useGetAllTransactions from "@transactions/useGetAllTransactions";

/**
 * Component that provides a list of Transactions and makes the basic create, read, update, and delete
 * operations available on them.
 * @returns JSX element representing a list of Transactions with various action buttons.
 */
const TransactionListFrame = function (): JSX.Element {
  const [dialog, setDialog] = useState<JSX.Element | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const { transactions, isLoading, error, refetch } = useGetAllTransactions();
  const getAmountDisplay = function (transaction: Transaction): string {
    let total = 0.0;
    if (transaction.debitFundAmounts) {
      total = transaction.debitFundAmounts.reduce(
        (sum, fundAmount) => sum + fundAmount.amount,
        0,
      );
    } else if (transaction.creditFundAmounts) {
      total = transaction.creditFundAmounts.reduce(
        (sum, fundAmount) => sum + fundAmount.amount,
        0,
      );
    }
    return `$ ${total.toLocaleString([], {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    })}`;
  };
  return (
    <ListFrame<Transaction>
      name="Transactions"
      headers={[
        <ColumnHeader
          key="accountingPeriod"
          content="Accounting Period"
          minWidth={170}
          align="left"
        />,
        <ColumnHeader key="date" content="Date" minWidth={170} align="left" />,
        <ColumnHeader
          key="location"
          content="Location"
          minWidth={170}
          align="left"
        />,
        <ColumnHeader
          key="description"
          content="Description"
          minWidth={170}
          align="left"
        />,
        <ColumnHeader
          key="debitAccount"
          content="Debit Account"
          minWidth={170}
          align="left"
        />,
        <ColumnHeader
          key="creditAccount"
          content="Credit Account"
          minWidth={170}
          align="left"
        />,
        <ColumnHeader
          key="amount"
          content="Amount"
          minWidth={170}
          align="left"
        />,
        <TransactionListFrameActionColumnHeader
          key="actions"
          setDialog={setDialog}
          setMessage={setMessage}
          refetch={refetch}
        />,
      ]}
      columns={(transaction: Transaction) => [
        <ColumnCell
          key="accountingPeriod"
          content={transaction.accountingPeriodName}
          align="left"
          isLoading={isLoading}
          isError={error !== null}
        />,
        <ColumnCell
          key="date"
          content={transaction.date}
          align="left"
          isLoading={isLoading}
          isError={error !== null}
        />,
        <ColumnCell
          key="location"
          content={transaction.location}
          align="left"
          isLoading={isLoading}
          isError={error !== null}
        />,
        <ColumnCell
          key="description"
          content={transaction.description}
          align="left"
          isLoading={isLoading}
          isError={error !== null}
        />,
        <ColumnCell
          key="debitAccount"
          content={transaction.debitAccountName ?? ""}
          align="left"
          isLoading={isLoading}
          isError={error !== null}
        />,
        <ColumnCell
          key="creditAccount"
          content={transaction.creditAccountName ?? ""}
          align="left"
          isLoading={isLoading}
          isError={error !== null}
        />,
        <ColumnCell
          key="amount"
          content={getAmountDisplay(transaction)}
          align="left"
          isLoading={isLoading}
          isError={error !== null}
        />,
        <TransactionListFrameActionColumn
          key="actions"
          transaction={transaction}
          isLoading={isLoading}
          error={error}
          setDialog={setDialog}
          setMessage={setMessage}
          refetch={refetch}
        />,
      ]}
      getId={(transaction: Transaction) => transaction.id}
      data={transactions}
    >
      {dialog}
      <SuccessAlert message={message} />
      <ErrorAlert error={error} />
    </ListFrame>
  );
};

export default TransactionListFrame;
