import { AddCircleOutline, ArrowForwardIos } from "@mui/icons-material";
import { type JSX, useState } from "react";
import type { AccountingPeriod } from "@accounting-periods/ApiTypes";
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
import useGetAllTransactions from "@transactions/useGetAllTransactions";

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
  const { transactions, isLoading, error, refetch } = useGetAllTransactions({
    accountingPeriodId: accountingPeriod.id,
  });
  return (
    <ListFrame<Transaction>
      name="Transactions"
      headers={[
        <ColumnHeader key="date" content="Date" maxWidth={100} align="left" />,
        <ColumnHeader key="location" content="Location" align="left" />,
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

export default AccountingPeriodTransactionListFrame;
