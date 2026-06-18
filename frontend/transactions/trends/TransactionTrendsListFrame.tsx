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

const getSource = function (transaction: Transaction): string {
  if ("debitAccount" in transaction && transaction.debitAccount !== null) {
    return transaction.debitAccount.accountName;
  }
  if ("sourceLocation" in transaction && transaction.sourceLocation !== null) {
    return transaction.sourceLocation;
  }
  if ("debitFund" in transaction) {
    return transaction.debitFund.fundName;
  }
  return "";
};

const getDestination = function (transaction: Transaction): string {
  if ("creditAccount" in transaction && transaction.creditAccount !== null) {
    return transaction.creditAccount.accountName;
  }
  if (
    "destinationLocation" in transaction &&
    transaction.destinationLocation !== null
  ) {
    return transaction.destinationLocation;
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
 * Props for the TransactionTrendsListFrame component.
 */
interface TransactionTrendsListFrameProps {
  readonly data: Transaction[] | null;
  readonly totalCount: number | null;
}

/**
 * Presents the paged transaction table for the Transactions trends.
 */
const TransactionTrendsListFrame = function ({
  data,
  totalCount,
}: TransactionTrendsListFrameProps): JSX.Element {
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
      name: "description",
      headerContent: "Description",
      getBodyContent: (transaction: Transaction) => transaction.description,
      sortType:
        currentSort === TransactionSortOrder.Description
          ? ColumnSortType.Ascending
          : currentSort === TransactionSortOrder.DescriptionDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(TransactionSortOrder.Description);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(TransactionSortOrder.DescriptionDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 150,
    },
    {
      name: "source",
      headerContent: "Source",
      getBodyContent: getSource,
      sortType:
        currentSort === TransactionSortOrder.Source
          ? ColumnSortType.Ascending
          : currentSort === TransactionSortOrder.SourceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(TransactionSortOrder.Source);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(TransactionSortOrder.SourceDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 170,
    },
    {
      name: "destination",
      headerContent: "Destination",
      getBodyContent: getDestination,
      sortType:
        currentSort === TransactionSortOrder.Destination
          ? ColumnSortType.Ascending
          : currentSort === TransactionSortOrder.DestinationDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(TransactionSortOrder.Destination);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(TransactionSortOrder.DestinationDescending);
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
              "Create a transaction to start building the trends history.",
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
            title: "No transactions match this trends filter",
            description:
              "Try a different transaction type, account name, fund name, or date range to widen the trends scope.",
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

export default TransactionTrendsListFrame;
