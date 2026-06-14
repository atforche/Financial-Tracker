"use client";

import {
  type AccountingPeriodDashboard,
  AccountingPeriodDashboardTransactionSortOrder,
} from "@/accounting-periods/types";
import { Button, IconButton, Paper, Stack, Typography } from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import type { Transaction } from "@/transactions/types";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/transactions/routes";
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

const getAccountIds = function (transaction: Transaction): string[] {
  const accountIds = new Set<string>();

  if ("debitAccount" in transaction && transaction.debitAccount !== null) {
    accountIds.add(transaction.debitAccount.accountId);
  }
  if ("creditAccount" in transaction && transaction.creditAccount !== null) {
    accountIds.add(transaction.creditAccount.accountId);
  }

  return Array.from(accountIds);
};

const getFundIds = function (transaction: Transaction): string[] {
  const fundIds = new Set<string>();

  if ("debitFund" in transaction) {
    fundIds.add(transaction.debitFund.fundId);
  }
  if ("creditFund" in transaction) {
    fundIds.add(transaction.creditFund.fundId);
  }

  return Array.from(fundIds);
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
    sort: AccountingPeriodDashboardTransactionSortOrder | null,
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
    AccountingPeriodDashboardTransactionSortOrder,
    searchParams.get(sortParamName) ?? "",
  );

  const hasActiveFilters =
    searchParams.get(modeParamName) === "accounting-period" ||
    searchParams.has(startAccountingPeriodIdParamName) ||
    searchParams.has(endAccountingPeriodIdParamName);

  const openTransactionWorkspace = function (transaction: Transaction): void {
    router.push(
      routes.workspace({
        accountingPeriodIds: [transaction.accountingPeriodId],
        accountIds: getAccountIds(transaction),
        fundIds: getFundIds(transaction),
        selectedTransactionId: transaction.id,
      }),
    );
  };

  const columns: ColumnDefinition<Transaction>[] = [
    {
      name: "date",
      headerContent: "Date",
      getBodyContent: (transaction: Transaction) => transaction.date,
      sortType:
        currentSort === AccountingPeriodDashboardTransactionSortOrder.Date
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodDashboardTransactionSortOrder.DateDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodDashboardTransactionSortOrder.Date);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodDashboardTransactionSortOrder.DateDescending);
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
        currentSort ===
        AccountingPeriodDashboardTransactionSortOrder.AccountingPeriod
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodDashboardTransactionSortOrder.AccountingPeriodDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(
            AccountingPeriodDashboardTransactionSortOrder.AccountingPeriod,
          );
        } else if (sortType === ColumnSortType.Descending) {
          setSort(
            AccountingPeriodDashboardTransactionSortOrder.AccountingPeriodDescending,
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
        currentSort === AccountingPeriodDashboardTransactionSortOrder.Location
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodDashboardTransactionSortOrder.LocationDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodDashboardTransactionSortOrder.Location);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(
            AccountingPeriodDashboardTransactionSortOrder.LocationDescending,
          );
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
        currentSort === AccountingPeriodDashboardTransactionSortOrder.DebitFrom
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodDashboardTransactionSortOrder.DebitFromDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodDashboardTransactionSortOrder.DebitFrom);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(
            AccountingPeriodDashboardTransactionSortOrder.DebitFromDescending,
          );
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
        currentSort === AccountingPeriodDashboardTransactionSortOrder.CreditTo
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodDashboardTransactionSortOrder.CreditToDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodDashboardTransactionSortOrder.CreditTo);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(
            AccountingPeriodDashboardTransactionSortOrder.CreditToDescending,
          );
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
        currentSort === AccountingPeriodDashboardTransactionSortOrder.Amount
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodDashboardTransactionSortOrder.AmountDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodDashboardTransactionSortOrder.Amount);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(
            AccountingPeriodDashboardTransactionSortOrder.AmountDescending,
          );
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 100,
    },
    {
      name: "actions",
      headerContent: "",
      getBodyContent: (transaction: Transaction) => (
        <IconButton
          size="small"
          color="primary"
          onClick={(event) => {
            event.stopPropagation();
            openTransactionWorkspace(transaction);
          }}
          aria-label={`Open ${transaction.id}`}
        >
          <ArrowForwardOutlined fontSize="small" color="action" />
        </IconButton>
      ),
      alignment: "right",
      minWidth: 52,
      maxWidth: 52,
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
          onRowClick={(transaction) => {
            openTransactionWorkspace(transaction);
          }}
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
