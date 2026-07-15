"use client";

import { IconButton, Paper, Stack, Typography } from "@mui/material";
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

/**
 * Props for the CurrentTransactionListFrame component.
 */
interface CurrentTransactionListFrameProps {
  readonly title: string;
  readonly description: string;
  readonly data: Transaction[];
  readonly totalCount: number;
  readonly sortParamName: string;
  readonly pageParamName: string;
  readonly searchParamName: string;
  readonly emptyTitle: string;
  readonly emptyDescription: string;
  readonly emptyAction?: JSX.Element | null;
}

/**
 * List frame that displays transactions for the current page.
 */
const CurrentTransactionListFrame = function ({
  title,
  description,
  data,
  totalCount,
  sortParamName,
  pageParamName,
  searchParamName,
  emptyTitle,
  emptyDescription,
  emptyAction,
}: CurrentTransactionListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

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
      minWidth: 160,
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
      minWidth: 160,
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
      minWidth: 140,
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
        <Typography variant="h6">{title}</Typography>
        <Typography variant="body2" color="text.secondary">
          {description}
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
        initialEmptyState={{
          title: emptyTitle,
          description: emptyDescription,
          action: emptyAction ?? null,
        }}
      />
    </Paper>
  );
};

export default CurrentTransactionListFrame;
