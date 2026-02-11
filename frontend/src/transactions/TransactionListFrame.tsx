import { AddCircleOutline, ArrowForwardIos } from "@mui/icons-material";
import { type JSX, useState } from "react";
import ColumnButton from "@framework/listframe/ColumnButton";
import ColumnCell from "@framework/listframe/ColumnCell";
import ColumnHeader from "@framework/listframe/ColumnHeader";
import ColumnHeaderButton from "@framework/listframe/ColumnHeaderButton";
import CreateTransactionDialog from "@transactions/CreateTransactionDialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import ListFrame from "@framework/listframe/ListFrame";
import SuccessAlert from "@framework/alerts/SuccessAlert";
import type { Transaction } from "@transactions/ApiTypes";
import TransactionDialog from "@transactions/TransactionDialog";
import formatCurrency from "@framework/formatCurrency";
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
  return (
    <ListFrame<Transaction>
      name="Transactions"
      headers={[
        <ColumnHeader
          key="accountingPeriod"
          content="Accounting Period"
          maxWidth={125}
          align="left"
        />,
        <ColumnHeader key="date" content="Date" align="left" />,
        <ColumnHeader key="location" content="Location" align="left" />,
        <ColumnHeader key="description" content="Description" align="left" />,
        <ColumnHeader
          key="debitAccount"
          content="Debit Account"
          align="left"
        />,
        <ColumnHeader
          key="creditAccount"
          content="Credit Account"
          align="left"
        />,
        <ColumnHeader key="amount" content="Amount" align="left" />,
        <ColumnHeader
          key="actions"
          content={
            <ColumnHeaderButton
              label="Add"
              icon={<AddCircleOutline />}
              onClick={() => {
                setDialog(
                  <CreateTransactionDialog
                    onClose={(success) => {
                      setDialog(null);
                      if (success) {
                        setMessage("Transaction added successfully.");
                      }
                      refetch();
                    }}
                  />,
                );
                setMessage(null);
              }}
            />
          }
          align="right"
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
          content={transaction.debitAccount?.accountName ?? ""}
          align="left"
          isLoading={isLoading}
          isError={error !== null}
        />,
        <ColumnCell
          key="creditAccount"
          content={transaction.creditAccount?.accountName ?? ""}
          align="left"
          isLoading={isLoading}
          isError={error !== null}
        />,
        <ColumnCell
          key="amount"
          content={formatCurrency(transaction.amount)}
          align="left"
          isLoading={isLoading}
          isError={error !== null}
        />,
        <ColumnCell
          key="view"
          content={
            <ColumnButton
              label="View"
              icon={<ArrowForwardIos />}
              onClick={() => {
                setDialog(
                  <TransactionDialog
                    inputTransaction={transaction}
                    setMessage={setMessage}
                    onClose={(needsRefetch) => {
                      setDialog(null);
                      if (needsRefetch) {
                        refetch();
                      }
                    }}
                  />,
                );
                setMessage(null);
              }}
            />
          }
          align="right"
          isLoading={isLoading}
          isError={error !== null}
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
