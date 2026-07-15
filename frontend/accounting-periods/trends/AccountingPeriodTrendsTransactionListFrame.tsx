"use client";

import { Button, IconButton, Paper, Stack, Typography } from "@mui/material";
import {
  type Transaction,
  TransactionSort,
  type TransactionSortValue,
} from "@/transactions/transaction";
import {
  getTransactionAccountIds,
  getTransactionFundIds,
} from "@/transactions/postingHelpers";
import {
  getTransactionDestinationLabel,
  getTransactionSourceLabel,
} from "@/transactions/current/helpers";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/transactions/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";

interface AccountingPeriodTrendsTransactionListFrameProps {
  readonly transactions: Transaction[];
  readonly totalCount: number;
}

/**
 * List frame that displays transactions for the accounting period trends page.
 */
const AccountingPeriodTrendsTransactionListFrame = function ({
  transactions,
  totalCount,
}: AccountingPeriodTrendsTransactionListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const sortParamName = "transactionSort";
  const pageParamName = "transactionPage";
  const searchParamName = "transactionSearch";
  const modeParamName = "mode";
  const startAccountingPeriodIdParamName = "startAccountingPeriodId";
  const endAccountingPeriodIdParamName = "endAccountingPeriodId";

  const setSort = function (sort: TransactionSortValue | null): void {
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
    TransactionSort,
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
        accountIds: getTransactionAccountIds(transaction),
        fundIds: getTransactionFundIds(transaction),
        selectedTransactionId: transaction.id,
      }),
    );
  };

  const columns: ColumnDefinition<Transaction>[] = [
    {
      name: "date",
      headerContent: "Date",
      getBodyContent: (transaction) => transaction.date,
      sortType:
        currentSort === TransactionSort.Date
          ? ColumnSortType.Ascending
          : currentSort === TransactionSort.DateDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(TransactionSort.Date);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(TransactionSort.DateDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 125,
    },
    {
      name: "accountingPeriod",
      headerContent: "Accounting Period",
      getBodyContent: (transaction) => transaction.accountingPeriodName,
      sortType:
        currentSort === TransactionSort.AccountingPeriod
          ? ColumnSortType.Ascending
          : currentSort === TransactionSort.AccountingPeriodDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(TransactionSort.AccountingPeriod);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(TransactionSort.AccountingPeriodDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 165,
    },
    {
      name: "description",
      headerContent: "Description",
      getBodyContent: (transaction) => transaction.description,
      sortType:
        currentSort === TransactionSort.Description
          ? ColumnSortType.Ascending
          : currentSort === TransactionSort.DescriptionDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(TransactionSort.Description);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(TransactionSort.DescriptionDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 150,
    },
    {
      name: "source",
      headerContent: "Source",
      getBodyContent: getTransactionSourceLabel,
      sortType:
        currentSort === TransactionSort.Source
          ? ColumnSortType.Ascending
          : currentSort === TransactionSort.SourceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(TransactionSort.Source);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(TransactionSort.SourceDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 100,
    },
    {
      name: "destination",
      headerContent: "Destination",
      getBodyContent: getTransactionDestinationLabel,
      sortType:
        currentSort === TransactionSort.Destination
          ? ColumnSortType.Ascending
          : currentSort === TransactionSort.DestinationDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(TransactionSort.Destination);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(TransactionSort.DestinationDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 100,
    },
    {
      name: "amount",
      headerContent: "Amount",
      getBodyContent: (transaction) => formatCurrency(transaction.amount),
      sortType:
        currentSort === TransactionSort.Amount
          ? ColumnSortType.Ascending
          : currentSort === TransactionSort.AmountDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(TransactionSort.Amount);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(TransactionSort.AmountDescending);
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
        borderRadius: 3,
        overflow: "hidden",
      }}
    >
      <Stack spacing={0.5} sx={{ px: 2.5, pt: 2.5 }}>
        <Typography variant="h6">
          Transactions Across Selected Periods
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {hasActiveFilters
            ? "Transactions matching the current accounting period trend filters."
            : "Transactions across the selected accounting period range."}
        </Typography>
      </Stack>
      <ListFrame<Transaction>
        columns={columns}
        getId={(transaction) => transaction.id}
        data={transactions}
        totalCount={totalCount}
        pageParamName={pageParamName}
        searchParamName={searchParamName}
        onRowClick={openTransactionWorkspace}
        hasActiveFilters={hasActiveFilters}
        initialEmptyState={{
          title: "No transactions found",
          description:
            "Try broadening the accounting period range to bring more transactions into view.",
          action: null,
        }}
        filteredEmptyState={{
          title: "No transactions found",
          description:
            "Try broadening the accounting period range to bring more transactions into view.",
          action: hasActiveFilters ? (
            <Button
              variant="outlined"
              onClick={() => {
                const params = new URLSearchParams(searchParams.toString());
                params.delete(startAccountingPeriodIdParamName);
                params.delete(endAccountingPeriodIdParamName);
                params.delete(pageParamName);
                router.replace(`${pathname}?${params.toString()}`, {
                  scroll: false,
                });
              }}
            >
              Clear Filters
            </Button>
          ) : null,
        }}
      />
    </Paper>
  );
};

export default AccountingPeriodTrendsTransactionListFrame;
