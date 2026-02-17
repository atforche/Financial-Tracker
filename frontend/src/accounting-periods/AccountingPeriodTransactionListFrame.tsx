import { AddCircleOutline, ArrowForwardIos } from "@mui/icons-material";
import { type JSX, useState } from "react";
import type { AccountingPeriod } from "@accounting-periods/ApiTypes";
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
import useGetAccountingPeriodTransactions from "@accounting-periods/useGetAccountingPeriodTransactions";

/**
 * Props for the AccountingPeriodTransactionListFrame component.
 */
interface AccountingPeriodTransactionListFrameProps {
  readonly accountingPeriod: AccountingPeriod;
}

/**
 * Component that provides a list of Transactions for an AccountingPeriod and makes the basic create, read, update, and delete
 * operations available on them.
 * @param props - Props for the AccountingPeriodTransactionListFrame component.
 * @returns JSX element representing a list of Transactions with various action buttons.
 */
const AccountingPeriodTransactionListFrame = function ({
  accountingPeriod,
}: AccountingPeriodTransactionListFrameProps): JSX.Element {
  const [childDialog, setChildDialog] = useState<JSX.Element | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const { transactions, isLoading, error, refetch } =
    useGetAccountingPeriodTransactions({
      accountingPeriod,
    });

  const columns: ColumnDefinition<Transaction>[] = [
    {
      name: "date",
      headerContent: "Date",
      getBodyContent: (transaction: Transaction) => transaction.date,
    },
    {
      name: "location",
      headerContent: "Location",
      getBodyContent: (transaction: Transaction) => transaction.location,
    },
    {
      name: "debitAccount",
      headerContent: "Debit Account",
      getBodyContent: (transaction: Transaction) =>
        transaction.debitAccount?.accountName ?? "",
    },
    {
      name: "creditAccount",
      headerContent: "Credit Account",
      getBodyContent: (transaction: Transaction) =>
        transaction.creditAccount?.accountName ?? "",
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
      isLoading={isLoading}
      isError={error !== null}
    >
      {childDialog}
      <SuccessAlert message={message} />
      <ErrorAlert error={error} />
    </ListFrame>
  );
};

export default AccountingPeriodTransactionListFrame;
