import {
  type AccountingPeriod,
  AccountingPeriodSortOrder,
} from "@accounting-periods/ApiTypes";
import { AddCircleOutline, ArrowForwardIos } from "@mui/icons-material";
import { type JSX, useState } from "react";
import AccountingPeriodDialog from "@accounting-periods/AccountingPeriodDialog";
import ApiErrorHandler from "@data/ApiErrorHandler";
import { Checkbox } from "@mui/material";
import ColumnButton from "@framework/listframe/ColumnButton";
import type ColumnDefinition from "@framework/listframe/ColumnDefinition";
import ColumnHeaderButton from "@framework/listframe/ColumnHeaderButton";
import ColumnSortType from "@framework/listframe/ColumnSortType";
import CreateAccountingPeriodDialog from "@accounting-periods/CreateAccountingPeriodDialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import ListFrame from "@framework/listframe/ListFrame";
import SuccessAlert from "@framework/alerts/SuccessAlert";
import useGetAllAccountingPeriods from "@accounting-periods/useGetAllAccountingPeriods";

/**
 * Component that provides a list of Accounting Period and makes the basic create, read, update, and delete
 * operations available on them.
 * @returns JSX element representing a list of Accounting Period with various action buttons.
 */
const AccountingPeriodListFrame = function (): JSX.Element {
  const [dialog, setDialog] = useState<JSX.Element | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [sortBy, setSortBy] = useState<AccountingPeriodSortOrder | null>(null);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const { accountingPeriods, totalCount, isLoading, error, refetch } =
    useGetAllAccountingPeriods({ sortBy, page, rowsPerPage });

  const columns: ColumnDefinition<AccountingPeriod>[] = [
    {
      name: "period",
      headerContent: "Period",
      getBodyContent: (accountingPeriod: AccountingPeriod) =>
        accountingPeriod.name,
      sortType:
        sortBy === AccountingPeriodSortOrder.Date
          ? ColumnSortType.Ascending
          : sortBy === AccountingPeriodSortOrder.DateDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSortBy(AccountingPeriodSortOrder.Date);
        } else if (sortType === ColumnSortType.Descending) {
          setSortBy(AccountingPeriodSortOrder.DateDescending);
        } else {
          setSortBy(null);
        }
      },
    },
    {
      name: "isOpen",
      headerContent: "Is Open",
      getBodyContent: (accountingPeriod: AccountingPeriod) => (
        <Checkbox checked={accountingPeriod.isOpen} />
      ),
      sortType:
        sortBy === AccountingPeriodSortOrder.IsOpen
          ? ColumnSortType.Ascending
          : sortBy === AccountingPeriodSortOrder.IsOpenDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSortBy(AccountingPeriodSortOrder.IsOpen);
        } else if (sortType === ColumnSortType.Descending) {
          setSortBy(AccountingPeriodSortOrder.IsOpenDescending);
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
              <CreateAccountingPeriodDialog
                onClose={(success) => {
                  setDialog(null);
                  if (success) {
                    setMessage("Accounting Period added successfully.");
                    refetch();
                  }
                }}
              />,
            );
            setMessage(null);
          }}
        />
      ),
      getBodyContent: (accountingPeriod: AccountingPeriod) => (
        <ColumnButton
          label="View"
          icon={<ArrowForwardIos />}
          onClick={() => {
            setDialog(
              <AccountingPeriodDialog
                inputAccountingPeriod={accountingPeriod}
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
    <ListFrame<AccountingPeriod>
      name="Accounting Periods"
      columns={columns}
      getId={(accountingPeriod: AccountingPeriod) => accountingPeriod.id}
      data={accountingPeriods}
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

export default AccountingPeriodListFrame;
