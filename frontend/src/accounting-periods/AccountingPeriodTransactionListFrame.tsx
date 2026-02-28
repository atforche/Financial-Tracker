import {
  type AccountingPeriod,
  AccountingPeriodTransactionSortOrder,
} from "@accounting-periods/ApiTypes";
import { AddCircleOutline, ArrowForwardIos } from "@mui/icons-material";
import { type JSX, useState } from "react";
import ApiErrorHandler from "@data/ApiErrorHandler";
import ColumnButton from "@framework/listframe/ColumnButton";
import type ColumnDefinition from "@framework/listframe/ColumnDefinition";
import ColumnHeaderButton from "@framework/listframe/ColumnHeaderButton";
import ColumnSortType from "@framework/listframe/ColumnSortType";
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
  const [sortBy, setSortBy] =
    useState<AccountingPeriodTransactionSortOrder | null>(null);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(5);
  const { transactions, isLoading, error, refetch } =
    useGetAccountingPeriodTransactions({
      accountingPeriod,
      sortBy,
      page,
      rowsPerPage,
    });

  const columns: ColumnDefinition<Transaction>[] = [
    {
      name: "date",
      headerContent: "Date",
      getBodyContent: (transaction: Transaction) => transaction.date,
      sortType:
        sortBy === AccountingPeriodTransactionSortOrder.Date
          ? ColumnSortType.Ascending
          : sortBy === AccountingPeriodTransactionSortOrder.DateDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSortBy(AccountingPeriodTransactionSortOrder.Date);
        } else if (sortType === ColumnSortType.Descending) {
          setSortBy(AccountingPeriodTransactionSortOrder.DateDescending);
        } else {
          setSortBy(null);
        }
      },
    },
    {
      name: "location",
      headerContent: "Location",
      getBodyContent: (transaction: Transaction) => transaction.location,
      sortType:
        sortBy === AccountingPeriodTransactionSortOrder.Location
          ? ColumnSortType.Ascending
          : sortBy === AccountingPeriodTransactionSortOrder.LocationDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSortBy(AccountingPeriodTransactionSortOrder.Location);
        } else if (sortType === ColumnSortType.Descending) {
          setSortBy(AccountingPeriodTransactionSortOrder.LocationDescending);
        } else {
          setSortBy(null);
        }
      },
    },
    {
      name: "debitAccount",
      headerContent: "Debit Account",
      getBodyContent: (transaction: Transaction) =>
        transaction.debitAccount?.accountName ?? "",
      sortType:
        sortBy === AccountingPeriodTransactionSortOrder.DebitAccount
          ? ColumnSortType.Ascending
          : sortBy ===
              AccountingPeriodTransactionSortOrder.DebitAccountDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSortBy(AccountingPeriodTransactionSortOrder.DebitAccount);
        } else if (sortType === ColumnSortType.Descending) {
          setSortBy(
            AccountingPeriodTransactionSortOrder.DebitAccountDescending,
          );
        } else {
          setSortBy(null);
        }
      },
    },
    {
      name: "creditAccount",
      headerContent: "Credit Account",
      getBodyContent: (transaction: Transaction) =>
        transaction.creditAccount?.accountName ?? "",
      sortType:
        sortBy === AccountingPeriodTransactionSortOrder.CreditAccount
          ? ColumnSortType.Ascending
          : sortBy ===
              AccountingPeriodTransactionSortOrder.CreditAccountDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSortBy(AccountingPeriodTransactionSortOrder.CreditAccount);
        } else if (sortType === ColumnSortType.Descending) {
          setSortBy(
            AccountingPeriodTransactionSortOrder.CreditAccountDescending,
          );
        } else {
          setSortBy(null);
        }
      },
    },
    {
      name: "amount",
      headerContent: "Amount",
      getBodyContent: (transaction: Transaction) =>
        formatCurrency(transaction.amount),
      sortType:
        sortBy === AccountingPeriodTransactionSortOrder.Amount
          ? ColumnSortType.Ascending
          : sortBy === AccountingPeriodTransactionSortOrder.AmountDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSortBy(AccountingPeriodTransactionSortOrder.Amount);
        } else if (sortType === ColumnSortType.Descending) {
          setSortBy(AccountingPeriodTransactionSortOrder.AmountDescending);
        } else {
          setSortBy(null);
        }
      },
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
                      refetch();
                    }
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

  const errorHandler = error ? new ApiErrorHandler(error) : null;
  return (
    <ListFrame<Transaction>
      name="Transactions"
      columns={columns}
      getId={(transaction: Transaction) => transaction.id}
      data={transactions}
      totalCount={transactions ? transactions.length : null}
      isLoading={isLoading}
      isError={error !== null}
      page={page}
      setPage={setPage}
      rowsPerPage={rowsPerPage}
      setRowsPerPage={setRowsPerPage}
    >
      {childDialog}
      <SuccessAlert message={message} />
      <ErrorAlert errorHandler={errorHandler} />
    </ListFrame>
  );
};

export default AccountingPeriodTransactionListFrame;
