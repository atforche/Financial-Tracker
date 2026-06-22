"use client";

import { Button, IconButton, Paper, Stack, Typography } from "@mui/material";
import {
  type Transaction,
  TransactionSortOrder,
  getTransactionAccountIds,
  getTransactionDestinationLabel,
  getTransactionFundIds,
  getTransactionSourceLabel,
} from "@/transactions/types";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/transactions/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";

interface TransactionTrendsListFrameProps {
  readonly data: Transaction[] | null;
  readonly totalCount: number | null;
}

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
        accountIds: getTransactionAccountIds(transaction),
        fundIds: getTransactionFundIds(transaction),
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
      getBodyContent: (transaction) => transaction.date,
      sortType:
        currentSort === TransactionSortOrder.Date
          ? ColumnSortType.Ascending
          : currentSort === TransactionSortOrder.DateDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType) => {
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
      getBodyContent: (transaction) => transaction.accountingPeriodName,
      sortType:
        currentSort === TransactionSortOrder.AccountingPeriod
          ? ColumnSortType.Ascending
          : currentSort === TransactionSortOrder.AccountingPeriodDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType) => {
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
      getBodyContent: (transaction) => transaction.description,
      sortType:
        currentSort === TransactionSortOrder.Description
          ? ColumnSortType.Ascending
          : currentSort === TransactionSortOrder.DescriptionDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType) => {
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
      getBodyContent: getTransactionSourceLabel,
      sortType:
        currentSort === TransactionSortOrder.Source
          ? ColumnSortType.Ascending
          : currentSort === TransactionSortOrder.SourceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType) => {
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
      getBodyContent: getTransactionDestinationLabel,
      sortType:
        currentSort === TransactionSortOrder.Destination
          ? ColumnSortType.Ascending
          : currentSort === TransactionSortOrder.DestinationDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType) => {
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
      getBodyContent: (transaction) => formatCurrency(transaction.amount),
      sortType:
        currentSort === TransactionSortOrder.Amount
          ? ColumnSortType.Ascending
          : currentSort === TransactionSortOrder.AmountDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType) => {
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
        borderRadius: 3,
        overflow: "hidden",
      }}
    >
      <Stack spacing={0.5} sx={{ px: 2.5, pt: 2.5 }}>
        <Typography variant="h6">Matching Transactions</Typography>
        <Typography variant="body2" color="text.secondary">
          {hasActiveFilters
            ? "Transactions matching the selected trend filters."
            : "Transactions across the selected trend range."}
        </Typography>
      </Stack>
      <ListFrame<Transaction>
        columns={columns}
        getId={(transaction) => transaction.id}
        data={data}
        totalCount={totalCount}
        pageParamName={pageParamName}
        searchParamName={searchParamName}
        onRowClick={openTransactionWorkspace}
        hasActiveFilters={hasActiveFilters}
        initialEmptyState={{
          title: "No matching transactions",
          description:
            "Try widening the date range or clearing some filters to see more results.",
          action: null,
        }}
        filteredEmptyState={{
          title: "No matching transactions",
          description:
            "Try widening the date range or clearing some filters to see more results.",
          action: hasActiveFilters ? (
            <Button
              variant="outlined"
              onClick={() => {
                const params = new URLSearchParams(searchParams.toString());
                params.delete(transactionTypeParamName);
                params.delete(accountNameParamName);
                params.delete(fundNameParamName);
                params.delete(startAccountingPeriodIdParamName);
                params.delete(endAccountingPeriodIdParamName);
                params.delete(startDateParamName);
                params.delete(endDateParamName);
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

export default TransactionTrendsListFrame;
