import { AddCircleOutline, ArrowForwardIos } from "@mui/icons-material";
import { type JSX, useState } from "react";
import type { Account } from "@accounts/ApiTypes";
import ColumnButton from "@framework/listframe/ColumnButton";
import ColumnCell from "@framework/listframe/ColumnCell";
import ColumnHeader from "@framework/listframe/ColumnHeader";
import ColumnHeaderButton from "@framework/listframe/ColumnHeaderButton";
import CreateTransactionDialog from "@transactions/CreateTransactionDialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import ListFrame from "@framework/listframe/ListFrame";
import { Stack } from "@mui/material";
import SuccessAlert from "@framework/alerts/SuccessAlert";
import type { Transaction } from "@transactions/ApiTypes";
import TransactionDialog from "@transactions/TransactionDialog";
import formatCurrency from "@framework/formatCurrency";
import useGetAccountTransactions from "@accounts/useGetAccountTransactions";

/**
 * Props for the AccountTransactionListFrame component.
 */
interface AccountTransactionListFrameProps {
  readonly account: Account;
}

/**
 * Helper function to determine the type of a Transaction (debit, credit, or transfer) based on the perspective of a given Account.
 * @param transaction - The Transaction for which to determine the type.
 * @param accountId - The ID of the Account for which to determine the Transaction type.
 * @returns A string representing the Transaction type ("Debit", "Credit", or "Transfer").
 */
const getTransactionType = function (
  transaction: Transaction,
  accountId: string,
): string {
  if (
    transaction.debitAccount?.accountId === accountId &&
    transaction.creditAccount?.accountId === accountId
  ) {
    return "Transfer";
  }
  return transaction.debitAccount?.accountId === accountId ? "Debit" : "Credit";
};

/**
 * Component that provides a list of Transactions for an Account and makes the basic create, read, update, and delete
 * operations available on them.
 * @param props - Props for the AccountTransactionListFrame component.
 * @returns JSX element representing a list of Transactions with various action buttons.
 */
const AccountTransactionListFrame = function ({
  account,
}: AccountTransactionListFrameProps): JSX.Element {
  const [childDialog, setChildDialog] = useState<JSX.Element | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const { transactions, isLoading, error, refetch } = useGetAccountTransactions(
    {
      account,
    },
  );
  return (
    <ListFrame<Transaction>
      name="Transactions"
      headers={[
        <ColumnHeader key="date" content="Date" maxWidth={100} align="left" />,
        <ColumnHeader key="location" content="Location" align="left" />,
        <ColumnHeader key="type" content="Type" align="left" />,
        <ColumnHeader key="amount" content="Amount" align="left" />,
        <ColumnHeader
          key="actions"
          content={
            <Stack direction="row">
              <ColumnHeaderButton
                label="Add"
                icon={<AddCircleOutline />}
                onClick={() => {
                  setChildDialog(
                    <CreateTransactionDialog
                      onClose={(success) => {
                        setChildDialog(null);
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
            </Stack>
          }
          maxWidth={150}
          align="right"
        />,
      ]}
      columns={(transaction: Transaction) => [
        <ColumnCell
          key="date"
          content={
            (transaction.debitAccount?.accountId === account.id
              ? transaction.debitAccount.postedDate
              : transaction.creditAccount?.postedDate) ?? "Pending"
          }
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
          key="type"
          content={getTransactionType(transaction, account.id)}
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
                setChildDialog(
                  <TransactionDialog
                    inputTransaction={transaction}
                    setMessage={setMessage}
                    onClose={(needsRefetch) => {
                      setChildDialog(null);
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
      {childDialog}
      <SuccessAlert message={message} />
      <ErrorAlert error={error} />
    </ListFrame>
  );
};

export default AccountTransactionListFrame;
