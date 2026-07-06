"use client";

import { Box, Button, Paper, Stack, Typography } from "@mui/material";
import {
  type Transaction,
  TransactionSortOrder,
} from "@/transactions/transaction";
import {
  getTransactionDestinationLabel,
  getTransactionSourceLabel,
} from "@/transactions/current/helpers";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";
import KeyboardArrowRight from "@mui/icons-material/KeyboardArrowRight";
import ListFrame from "@/framework/listframe/ListFrame";
import type { TransactionWorkspaceSearchParams } from "@/transactions/workspace/TransactionWorkspace";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/transactions/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";

/**
 * Props for the TransactionWorkspaceListFrame component.
 */
interface TransactionWorkspaceListFrameProps {
  readonly data: Transaction[] | null;
  readonly totalCount: number | null;
}

/**
 * Component that displays the top-level transaction ledger.
 */
const TransactionWorkspaceListFrame = function ({
  data,
  totalCount,
}: TransactionWorkspaceListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const accountingPeriodIdsParamName = "accountingPeriodIds";
  const accountIdsParamName = "accountIds";
  const fundIdsParamName = "fundIds";
  const sortParamName = "sort";
  const pageParamName = "page";

  const replaceSearchParams = function (
    update: (params: URLSearchParams) => void,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    update(params);
    const query = params.toString();
    router.replace(query === "" ? pathname : `${pathname}?${query}`, {
      scroll: false,
    });
  };

  const setSort = function (sort: TransactionSortOrder | null): void {
    replaceSearchParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
      params.delete(pageParamName);
    });
  };

  const openTransaction = function (transactionId: string): void {
    const params = new URLSearchParams(searchParams.toString());
    router.push(
      routes.workspaceDetail(transactionId, {
        accountingPeriodIds: params.getAll(accountingPeriodIdsParamName),
        accountIds: params.getAll(accountIdsParamName),
        fundIds: params.getAll(fundIdsParamName),
        sort:
          tryParseEnum(TransactionSortOrder, params.get(sortParamName) ?? "") ??
          null,
        page: params.get(pageParamName),
      } satisfies TransactionWorkspaceSearchParams),
      { scroll: false },
    );
  };

  const currentSort = tryParseEnum(
    TransactionSortOrder,
    searchParams.get(sortParamName) ?? "",
  );
  const createQuery = searchParams.toString();
  const createUrl =
    createQuery === ""
      ? `${pathname}/create`
      : `${pathname}/create?${createQuery}`;

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
      getBodyContent: getTransactionSourceLabel,
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
      getBodyContent: getTransactionDestinationLabel,
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
      name: "open",
      headerContent: "",
      getBodyContent: () => (
        <Box
          sx={{
            alignItems: "center",
            color: "text.secondary",
            display: "flex",
            justifyContent: "center",
            minHeight: 40,
          }}
        >
          <KeyboardArrowRight fontSize="small" />
        </Box>
      ),
      alignment: "center",
      minWidth: 0,
      maxWidth: 0,
    },
  ];

  return (
    <Paper
      sx={{
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        p: { xs: 2, md: 2.5 },
      }}
    >
      <Stack spacing={2.5}>
        <Stack
          direction={{ xs: "column", sm: "row" }}
          spacing={1.5}
          justifyContent="space-between"
          alignItems={{ xs: "stretch", sm: "center" }}
        >
          <Typography variant="h6" color="text.secondary">
            Transactions
          </Typography>
          <Button
            variant="contained"
            onClick={() => {
              router.push(createUrl);
            }}
          >
            Create Transaction
          </Button>
        </Stack>
        <ListFrame<Transaction>
          columns={columns}
          getId={(transaction) => transaction.id}
          data={data ?? null}
          totalCount={totalCount ?? null}
          searchParamName=""
          pageParamName={pageParamName}
          onRowClick={(transaction) => {
            openTransaction(transaction.id);
          }}
          initialEmptyState={{
            title: "No transactions found",
            description: "No transactions have been recorded yet.",
            action: null,
          }}
          filteredEmptyState={{
            title: "No transactions match this search",
            description:
              "Try a different description, amount, date, or account name, or clear the current search to see all matching transactions.",
            action: (
              <Button
                variant="contained"
                onClick={() => {
                  const params = new URLSearchParams(searchParams.toString());
                  params.delete(accountingPeriodIdsParamName);
                  params.delete(accountIdsParamName);
                  params.delete(fundIdsParamName);
                  params.delete(pageParamName);
                  router.replace(`${pathname}?${params.toString()}`, {
                    scroll: false,
                  });
                }}
              >
                Clear search
              </Button>
            ),
          }}
        />
      </Stack>
    </Paper>
  );
};

export default TransactionWorkspaceListFrame;
