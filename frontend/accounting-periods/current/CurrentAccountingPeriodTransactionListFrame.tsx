"use client";

import {
  AccountingPeriodTrendsTransactionSortOrder,
  type CurrentAccountingPeriod,
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

interface CurrentAccountingPeriodTransactionListFrameProps {
  readonly current: CurrentAccountingPeriod;
}

/**
 * Presents the paged transaction table for the current Accounting Period.
 */
const CurrentAccountingPeriodTransactionListFrame = function ({
  current,
}: CurrentAccountingPeriodTransactionListFrameProps): JSX.Element {
  const accountingPeriod = current.accountingPeriod ?? null;
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const sortParamName = "transactionSort";
  const pageParamName = "transactionPage";
  const searchParamName = "transactionSearch";

  const setSort = function (
    sort: AccountingPeriodTrendsTransactionSortOrder | null,
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
    AccountingPeriodTrendsTransactionSortOrder,
    searchParams.get(sortParamName) ?? "",
  );

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
        currentSort === AccountingPeriodTrendsTransactionSortOrder.Date
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTrendsTransactionSortOrder.DateDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodTrendsTransactionSortOrder.Date);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodTrendsTransactionSortOrder.DateDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 125,
    },
    {
      name: "location",
      headerContent: "Location",
      getBodyContent: (transaction: Transaction) => transaction.location,
      sortType:
        currentSort === AccountingPeriodTrendsTransactionSortOrder.Location
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTrendsTransactionSortOrder.LocationDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodTrendsTransactionSortOrder.Location);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(
            AccountingPeriodTrendsTransactionSortOrder.LocationDescending,
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
        currentSort === AccountingPeriodTrendsTransactionSortOrder.DebitFrom
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTrendsTransactionSortOrder.DebitFromDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodTrendsTransactionSortOrder.DebitFrom);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(
            AccountingPeriodTrendsTransactionSortOrder.DebitFromDescending,
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
        currentSort === AccountingPeriodTrendsTransactionSortOrder.CreditTo
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTrendsTransactionSortOrder.CreditToDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodTrendsTransactionSortOrder.CreditTo);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(
            AccountingPeriodTrendsTransactionSortOrder.CreditToDescending,
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
        currentSort === AccountingPeriodTrendsTransactionSortOrder.Amount
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTrendsTransactionSortOrder.AmountDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodTrendsTransactionSortOrder.Amount);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodTrendsTransactionSortOrder.AmountDescending);
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
          data={current.transactions.items}
          onRowClick={(transaction) => {
            openTransactionWorkspace(transaction);
          }}
          totalCount={current.transactions.totalCount}
          searchParamName={searchParamName}
          pageParamName={pageParamName}
          hasActiveFilters={false}
          initialEmptyState={{
            title:
              accountingPeriod === null
                ? "No accounting period available"
                : "No transactions found",
            description:
              accountingPeriod === null
                ? "Create an accounting period to view a current transaction snapshot."
                : `No transactions were included in ${accountingPeriod.name}.`,
            action:
              searchParams.has(sortParamName) ||
              searchParams.has(pageParamName) ? (
                <Button
                  variant="contained"
                  onClick={() => {
                    router.replace(pathname);
                  }}
                >
                  Reset sorting
                </Button>
              ) : null,
          }}
        />
      </Stack>
    </Paper>
  );
};

export default CurrentAccountingPeriodTransactionListFrame;
