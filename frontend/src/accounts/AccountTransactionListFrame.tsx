import { AddCircleOutline, ArrowForwardIos } from "@mui/icons-material";
import { type JSX, useState } from "react";
import type { Account } from "@accounts/ApiTypes";
import ColumnButton from "@framework/listframe/ColumnButton";
import type ColumnDefinition from "@framework/listframe/ColumnDefinition";
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
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(5);
  const { transactions, totalCount, isLoading, error, refetch } =
    useGetAccountTransactions({
      account,
      page,
      rowsPerPage,
    });

  const columns: ColumnDefinition<Transaction>[] = [
    {
      name: "date",
      headerContent: "Date",
      getBodyContent: (transaction: Transaction) =>
        transaction.debitAccount?.accountId === account.id
          ? (transaction.debitAccount.postedDate ?? "Pending")
          : (transaction.creditAccount?.postedDate ?? "Pending"),
    },
    {
      name: "location",
      headerContent: "Location",
      getBodyContent: (transaction: Transaction) => transaction.location,
    },
    {
      name: "type",
      headerContent: "Type",
      getBodyContent: (transaction: Transaction) =>
        getTransactionType(transaction, account.id),
    },
    {
      name: "amount",
      headerContent: "Amount",
      getBodyContent: (transaction: Transaction) =>
        formatCurrency(transaction.amount),
    },
    {
      name: "actions",
      headerContent: (
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
      ),
      getBodyContent: (transaction: Transaction) => (
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
      ),
    },
  ];

  return (
    <ListFrame<Transaction>
      name="Transactions"
      columns={columns}
      getId={(transaction: Transaction) => transaction.id}
      data={transactions}
      totalCount={totalCount}
      isLoading={isLoading}
      isError={error !== null}
      page={page}
      setPage={setPage}
      rowsPerPage={rowsPerPage}
      setRowsPerPage={setRowsPerPage}
    >
      {childDialog}
      <SuccessAlert message={message} />
      <ErrorAlert error={error} />
    </ListFrame>
  );
};

export default AccountTransactionListFrame;
