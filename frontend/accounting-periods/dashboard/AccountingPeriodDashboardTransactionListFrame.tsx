"use client";

import {
  type AccountingPeriodDashboard,
  AccountingPeriodTransactionSortOrder,
} from "@/accounting-periods/types";
import { Button, Paper, Stack, Typography } from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import type { Transaction } from "@/transactions/types";
import formatCurrency from "@/framework/formatCurrency";
import tryParseEnum from "@/framework/data/tryParseEnum";

const getDebitFrom = function (transaction: Transaction): string {
  if ("debitAccount" in transaction) {
    return transaction.debitAccount?.accountName ?? "";
  }
  if ("debitFund" in transaction) {
    return transaction.debitFund.fundName;
  }
  return "";
};

const getCreditTo = function (transaction: Transaction): string {
  if ("creditAccount" in transaction) {
    return transaction.creditAccount?.accountName ?? "";
  }
  if ("creditFund" in transaction) {
    return transaction.creditFund.fundName;
  }
  return "";
};

/**
 * Props for the AccountingPeriodDashboardTransactionListFrame component.
 */
interface AccountingPeriodDashboardTransactionListFrameProps {
  readonly dashboard: AccountingPeriodDashboard;
}

/**
 * Presents the paged transaction table for the Accounting Periods dashboard.
 */
const AccountingPeriodDashboardTransactionListFrame = function ({
  dashboard,
}: AccountingPeriodDashboardTransactionListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const sortParamName = "transactionSort";
  const pageParamName = "transactionPage";
  const searchParamName = "transactionSearch";
  const modeParamName = "mode";
  const startAccountingPeriodIdParamName = "startAccountingPeriodId";
  const endAccountingPeriodIdParamName = "endAccountingPeriodId";

  const setSort = function (
    sort: AccountingPeriodTransactionSortOrder | null,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    if (sort === null) {
      params.delete(sortParamName);
    } else {
      params.set(sortParamName, sort);
    }
    params.delete(pageParamName);
    router.replace(`${pathname}?${params.toString()}`, { scroll: false });
  };

  const currentSort = tryParseEnum(
    AccountingPeriodTransactionSortOrder,
    searchParams.get(sortParamName) ?? "",
  );

  const hasActiveFilters =
    searchParams.get(modeParamName) === "accounting-period" ||
    searchParams.has(startAccountingPeriodIdParamName) ||
    searchParams.has(endAccountingPeriodIdParamName);

  const columns: ColumnDefinition<Transaction>[] = [
    {
      name: "date",
      headerContent: "Date",
      getBodyContent: (transaction: Transaction) => transaction.date,
      sortType:
        currentSort === AccountingPeriodTransactionSortOrder.Date
          ? ColumnSortType.Ascending
          : currentSort === AccountingPeriodTransactionSortOrder.DateDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodTransactionSortOrder.Date);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodTransactionSortOrder.DateDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 125,
    },
    {
      name: "accountingPeriod",
      headerContent: "Accounting Period",
      getBodyContent: (transaction: Transaction) =>
        transaction.accountingPeriodName,
      sortType:
        currentSort === AccountingPeriodTransactionSortOrder.AccountingPeriod
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTransactionSortOrder.AccountingPeriodDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodTransactionSortOrder.AccountingPeriod);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(
            AccountingPeriodTransactionSortOrder.AccountingPeriodDescending,
          );
        } else {
          setSort(null);
        }
      },
      minWidth: 165,
    },
    {
      name: "location",
      headerContent: "Location",
      getBodyContent: (transaction: Transaction) => transaction.location,
      sortType:
        currentSort === AccountingPeriodTransactionSortOrder.Location
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTransactionSortOrder.LocationDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodTransactionSortOrder.Location);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodTransactionSortOrder.LocationDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 150,
    },
    {
      name: "debitFrom",
      headerContent: "Debit From",
      getBodyContent: getDebitFrom,
      sortType:
        currentSort === AccountingPeriodTransactionSortOrder.DebitFrom
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTransactionSortOrder.DebitFromDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodTransactionSortOrder.DebitFrom);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodTransactionSortOrder.DebitFromDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 100,
    },
    {
      name: "creditTo",
      headerContent: "Credit To",
      getBodyContent: getCreditTo,
      sortType:
        currentSort === AccountingPeriodTransactionSortOrder.CreditTo
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTransactionSortOrder.CreditToDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodTransactionSortOrder.CreditTo);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodTransactionSortOrder.CreditToDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 100,
    },
    {
      name: "amount",
      headerContent: "Amount",
      getBodyContent: (transaction: Transaction) =>
        formatCurrency(transaction.amount),
      sortType:
        currentSort === AccountingPeriodTransactionSortOrder.Amount
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTransactionSortOrder.AmountDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodTransactionSortOrder.Amount);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodTransactionSortOrder.AmountDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 100,
    },
  ];

  return (
    <Paper
      sx={{
        border: "1px solid",
        borderColor: "divider",
        p: { xs: 2, md: 2.5 },
      }}
    >
      <Stack spacing={2.5}>
        <Typography variant="h5">Transactions</Typography>
        <ListFrame<Transaction>
          columns={columns}
          getId={(transaction) => transaction.id}
          data={dashboard.transactions.items}
          totalCount={dashboard.transactions.totalCount}
          searchParamName={searchParamName}
          pageParamName={pageParamName}
          hasActiveFilters={hasActiveFilters}
          initialEmptyState={{
            title: "No transactions found",
            description:
              "No transactions were included in this accounting period dashboard range.",
            action: (
              <Button
                variant="contained"
                onClick={() => {
                  router.replace(pathname);
                }}
              >
                Reset dashboard
              </Button>
            ),
          }}
          filteredEmptyState={{
            title: "No transactions match this dashboard filter",
            description:
              "Try a different accounting period range to widen the transaction list.",
            action: (
              <Button
                variant="contained"
                onClick={() => {
                  router.replace(pathname);
                }}
              >
                Reset filters
              </Button>
            ),
          }}
        />
      </Stack>
    </Paper>
  );
};

export default AccountingPeriodDashboardTransactionListFrame;
