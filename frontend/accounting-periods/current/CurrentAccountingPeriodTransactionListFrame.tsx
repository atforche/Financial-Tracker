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
import {
  getTransactionAccountIds,
  getTransactionDestinationLabel,
  getTransactionFundIds,
  getTransactionSourceLabel,
} from "@/transactions/types";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/transactions/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";

interface CurrentAccountingPeriodTransactionListFrameProps {
  readonly current: CurrentAccountingPeriod;
}

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
        currentSort === AccountingPeriodTrendsTransactionSortOrder.Date
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTrendsTransactionSortOrder.DateDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType) => {
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
      getBodyContent: (transaction) => transaction.description,
      sortType:
        currentSort === AccountingPeriodTrendsTransactionSortOrder.Description
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTrendsTransactionSortOrder.DescriptionDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType) => {
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
      getBodyContent: getTransactionSourceLabel,
      sortType:
        currentSort === AccountingPeriodTrendsTransactionSortOrder.Source
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTrendsTransactionSortOrder.SourceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType) => {
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
      getBodyContent: getTransactionDestinationLabel,
      sortType:
        currentSort === AccountingPeriodTrendsTransactionSortOrder.Destination
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTrendsTransactionSortOrder.DestinationDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType) => {
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
      getBodyContent: (transaction) => formatCurrency(transaction.amount),
      sortType:
        currentSort === AccountingPeriodTrendsTransactionSortOrder.Amount
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTrendsTransactionSortOrder.AmountDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType) => {
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
      getBodyContent: (transaction) => (
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
        borderRadius: 3,
        overflow: "hidden",
      }}
    >
      <Stack spacing={0.5} sx={{ px: 2.5, pt: 2.5 }}>
        <Typography variant="h6">
          {accountingPeriod === null
            ? "Transactions"
            : `Transactions in ${accountingPeriod.name}`}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {accountingPeriod === null
            ? "Review the current accounting period transactions."
            : `Review all transactions currently assigned to ${accountingPeriod.name}.`}
        </Typography>
      </Stack>
      <ListFrame<Transaction>
        columns={columns}
        getId={(transaction) => transaction.id}
        data={current.transactions.items}
        totalCount={current.transactions.totalCount}
        pageParamName={pageParamName}
        searchParamName={searchParamName}
        onRowClick={openTransactionWorkspace}
        initialEmptyState={{
          title: "No transactions in this period",
          description:
            "Add or move transactions into this accounting period to see them here.",
          action: (
            <Button
              variant="outlined"
              onClick={() => {
                router.push(routes.workspace({}));
              }}
            >
              Open Transaction Workspace
            </Button>
          ),
        }}
      />
    </Paper>
  );
};

export default CurrentAccountingPeriodTransactionListFrame;
