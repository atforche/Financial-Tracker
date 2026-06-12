"use client";

import { Button, IconButton, Paper, Stack, Typography } from "@mui/material";
import { type Transaction, TransactionSortOrder } from "@/transactions/types";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
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
  const debitFund = "debitFund" in transaction ? transaction.debitFund : null;
  const creditFund =
    "creditFund" in transaction ? transaction.creditFund : null;

  if (debitFund !== null) {
    fundIds.add(debitFund.fundId);
  }
  if (creditFund !== null) {
    fundIds.add(creditFund.fundId);
  }

  return Array.from(fundIds);
};

/**
 * Props for the TransactionDashboardListFrame component.
 */
interface TransactionDashboardListFrameProps {
  readonly data: Transaction[] | null;
  readonly totalCount: number | null;
}

/**
 * Presents the paged transaction table for the Transactions dashboard.
 */
const TransactionDashboardListFrame = function ({
  data,
  totalCount,
}: TransactionDashboardListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const sortParamName = "sort";
  const pageParamName = "page";
  const searchParamName = "transactionSearch";
  const modeParamName = "mode";
  const transactionTypeParamName = "transactionType";
  const accountNameParamName = "accountName";
  const fundNameParamName = "fundName";
  const startAccountingPeriodIdParamName = "startAccountingPeriodId";
  const endAccountingPeriodIdParamName = "endAccountingPeriodId";
  const startDateParamName = "startDate";
  const endDateParamName = "endDate";

  const setSort = function (sort: TransactionSortOrder | null): void {
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
    TransactionSortOrder,
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

  const hasActiveFilters =
    searchParams.get(modeParamName) === "date" ||
    searchParams.getAll(transactionTypeParamName).length > 0 ||
    searchParams.getAll(accountNameParamName).length > 0 ||
    searchParams.getAll(fundNameParamName).length > 0 ||
    searchParams.has(startAccountingPeriodIdParamName) ||
    searchParams.has(endAccountingPeriodIdParamName) ||
    searchParams.has(startDateParamName) ||
    searchParams.has(endDateParamName);

  const columns: ColumnDefinition<Transaction>[] = [
    {
      name: "date",
      headerContent: "Date",
      getBodyContent: (transaction: Transaction) => transaction.date,
      sortType:
        currentSort === TransactionSortOrder.Date
          ? ColumnSortType.Ascending
          : currentSort === TransactionSortOrder.DateDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(TransactionSortOrder.Date);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(TransactionSortOrder.DateDescending);
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
        currentSort === TransactionSortOrder.AccountingPeriod
          ? ColumnSortType.Ascending
          : currentSort === TransactionSortOrder.AccountingPeriodDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(TransactionSortOrder.AccountingPeriod);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(TransactionSortOrder.AccountingPeriodDescending);
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
        currentSort === TransactionSortOrder.Location
          ? ColumnSortType.Ascending
          : currentSort === TransactionSortOrder.LocationDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(TransactionSortOrder.Location);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(TransactionSortOrder.LocationDescending);
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
        currentSort === TransactionSortOrder.DebitFrom
          ? ColumnSortType.Ascending
          : currentSort === TransactionSortOrder.DebitFromDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(TransactionSortOrder.DebitFrom);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(TransactionSortOrder.DebitFromDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 170,
    },
    {
      name: "creditTo",
      headerContent: "Credit To",
      getBodyContent: getCreditTo,
      sortType:
        currentSort === TransactionSortOrder.CreditTo
          ? ColumnSortType.Ascending
          : currentSort === TransactionSortOrder.CreditToDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(TransactionSortOrder.CreditTo);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(TransactionSortOrder.CreditToDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 170,
    },
    {
      name: "amount",
      headerContent: "Amount",
      getBodyContent: (transaction: Transaction) =>
        formatCurrency(transaction.amount),
      sortType:
        currentSort === TransactionSortOrder.Amount
          ? ColumnSortType.Ascending
          : currentSort === TransactionSortOrder.AmountDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(TransactionSortOrder.Amount);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(TransactionSortOrder.AmountDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 150,
    },
    {
      name: "actions",
      headerContent: "",
      getBodyContent: (transaction) => (
        <IconButton
          size="small"
          color="primary"
          onClick={(event) => {
            event.stopPropagation();
            openTransactionWorkspace(transaction);
          }}
          aria-label={`Open transaction ${transaction.id}`}
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
          data={data ?? null}
          totalCount={totalCount ?? null}
          searchParamName={searchParamName}
          pageParamName={pageParamName}
          hasActiveFilters={hasActiveFilters}
          onRowClick={(transaction) => {
            openTransactionWorkspace(transaction);
          }}
          initialEmptyState={{
            title: "No transactions have been recorded",
            description:
              "Create a transaction to start building the dashboard history.",
            action: (
              <Button
                variant="contained"
                onClick={() => {
                  router.push(routes.workspace({ action: "create" }));
                }}
              >
                Create transaction
              </Button>
            ),
          }}
          filteredEmptyState={{
            title: "No transactions match this dashboard filter",
            description:
              "Try a different transaction type, account name, fund name, or date range to widen the dashboard scope.",
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

export default TransactionDashboardListFrame;
