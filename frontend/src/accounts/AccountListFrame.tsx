import { type Account, AccountSortOrder } from "@accounts/ApiTypes";
import { AddCircleOutline, ArrowForwardIos } from "@mui/icons-material";
import { type JSX, useState } from "react";
import AccountDialog from "@accounts/AccountDialog";
import ApiErrorHandler from "@data/ApiErrorHandler";
import ColumnButton from "@framework/listframe/ColumnButton";
import type ColumnDefinition from "@framework/listframe/ColumnDefinition";
import ColumnHeaderButton from "@framework/listframe/ColumnHeaderButton";
import ColumnSortType from "@framework/listframe/ColumnSortType";
import CreateAccountDialog from "@accounts/CreateAccountDialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import ListFrame from "@framework/listframe/ListFrame";
import SuccessAlert from "@framework/alerts/SuccessAlert";
import formatCurrency from "@framework/formatCurrency";
import useGetAllAccounts from "@accounts/useGetAllAccounts";

/**
 * Component that provides a list of Accounts and makes the basic create, read, update, and delete
 * operations available on them.
 * @returns JSX element representing a list of Accounts with various action buttons.
 */
const AccountListFrame = function (): JSX.Element {
  const [dialog, setDialog] = useState<JSX.Element | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [sortBy, setSortBy] = useState<AccountSortOrder | null>(null);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const { accounts, totalCount, isLoading, error, refetch } = useGetAllAccounts(
    { sortBy, page, rowsPerPage },
  );

  const columns: ColumnDefinition<Account>[] = [
    {
      name: "name",
      headerContent: "Name",
      getBodyContent: (account: Account) => account.name,
      sortType:
        sortBy === AccountSortOrder.Name
          ? ColumnSortType.Ascending
          : sortBy === AccountSortOrder.NameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSortBy(AccountSortOrder.Name);
        } else if (sortType === ColumnSortType.Descending) {
          setSortBy(AccountSortOrder.NameDescending);
        } else {
          setSortBy(null);
        }
      },
    },
    {
      name: "type",
      headerContent: "Type",
      getBodyContent: (account: Account) => account.type,
      sortType:
        sortBy === AccountSortOrder.Type
          ? ColumnSortType.Ascending
          : sortBy === AccountSortOrder.TypeDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSortBy(AccountSortOrder.Type);
        } else if (sortType === ColumnSortType.Descending) {
          setSortBy(AccountSortOrder.TypeDescending);
        } else {
          setSortBy(null);
        }
      },
    },
    {
      name: "balance",
      headerContent: "Posted Balance",
      getBodyContent: (account: Account) =>
        formatCurrency(account.currentBalance.postedBalance),
      sortType:
        sortBy === AccountSortOrder.PostedBalance
          ? ColumnSortType.Ascending
          : sortBy === AccountSortOrder.PostedBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSortBy(AccountSortOrder.PostedBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSortBy(AccountSortOrder.PostedBalanceDescending);
        } else {
          setSortBy(null);
        }
      },
    },
    {
      name: "available",
      headerContent: "Available to Spend",
      getBodyContent: (account: Account) =>
        account.currentBalance.availableToSpend === null
          ? ""
          : formatCurrency(account.currentBalance.availableToSpend),
      sortType:
        sortBy === AccountSortOrder.AvailableToSpend
          ? ColumnSortType.Ascending
          : sortBy === AccountSortOrder.AvailableToSpendDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSortBy(AccountSortOrder.AvailableToSpend);
        } else if (sortType === ColumnSortType.Descending) {
          setSortBy(AccountSortOrder.AvailableToSpendDescending);
        } else {
          setSortBy(null);
        }
      },
    },
    {
      name: "actions",
      headerContent: (
        <ColumnHeaderButton
          label="Add"
          icon={<AddCircleOutline />}
          onClick={() => {
            setDialog(
              <CreateAccountDialog
                onClose={(success) => {
                  setDialog(null);
                  if (success) {
                    setMessage("Account added successfully.");
                    refetch();
                  }
                }}
              />,
            );
            setMessage(null);
          }}
        />
      ),
      getBodyContent: (account: Account) => (
        <ColumnButton
          label="View"
          icon={<ArrowForwardIos />}
          onClick={() => {
            setDialog(
              <AccountDialog
                inputAccount={account}
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
      ),
      alignment: "right",
    },
  ];

  const errorHandler = error ? new ApiErrorHandler(error) : null;
  return (
    <ListFrame<Account>
      name="Accounts"
      columns={columns}
      getId={(account: Account) => account.id}
      data={accounts}
      totalCount={totalCount}
      isLoading={isLoading}
      isError={error !== null}
      page={page}
      setPage={setPage}
      rowsPerPage={rowsPerPage}
      setRowsPerPage={setRowsPerPage}
    >
      {dialog}
      <SuccessAlert message={message} />
      <ErrorAlert errorHandler={errorHandler} />
    </ListFrame>
  );
};

export default AccountListFrame;
