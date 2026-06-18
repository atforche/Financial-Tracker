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

  if ("debitFund" in transaction) {
    fundIds.add(transaction.debitFund.fundId);
  }
  if ("creditFund" in transaction) {
    fundIds.add(transaction.creditFund.fundId);
  }
  if ("fundAssignments" in transaction) {
    transaction.fundAssignments.forEach((fundAssignment) => {
      fundIds.add(fundAssignment.fundId);
    });
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
      name: "description",
      headerContent: "Description",
      getBodyContent: (transaction: Transaction) => transaction.description,
      sortType:
        currentSort === AccountingPeriodTrendsTransactionSortOrder.Description
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTrendsTransactionSortOrder.DescriptionDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodTrendsTransactionSortOrder.Description);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(
            AccountingPeriodTrendsTransactionSortOrder.DescriptionDescending,
          );
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
        currentSort === AccountingPeriodTrendsTransactionSortOrder.Source
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTrendsTransactionSortOrder.SourceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodTrendsTransactionSortOrder.Source);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodTrendsTransactionSortOrder.SourceDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 100,
    },
    {
      name: "destination",
      headerContent: "Destination",
      getBodyContent: getDestination,
      sortType:
        currentSort === AccountingPeriodTrendsTransactionSortOrder.Destination
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTrendsTransactionSortOrder.DestinationDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodTrendsTransactionSortOrder.Destination);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(
            AccountingPeriodTrendsTransactionSortOrder.DestinationDescending,
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
          initialEmptyState={{
            title: "No transactions found",
            description:
              accountingPeriod === null
                ? "No current accounting period is available for transaction tracking."
                : `No transactions were recorded for ${accountingPeriod.name}.`,
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
            title: "No transactions match this search",
            description:
              "Try a different description, account, fund, or amount to find the transaction you need.",
            action: (
              <Button
                variant="contained"
                onClick={() => {
                  router.replace(pathname);
                }}
              >
                Reset search
              </Button>
            ),
          }}
        />
      </Stack>
    </Paper>
  );
};

export default CurrentAccountingPeriodTransactionListFrame;
