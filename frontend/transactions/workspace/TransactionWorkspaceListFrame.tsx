"use client";

import {
  Box,
  Button,
  Checkbox,
  Paper,
  Stack,
  Typography,
  useMediaQuery,
  useTheme,
} from "@mui/material";
import {
  type Transaction,
  TransactionSortOrder,
} from "@/transactions/transaction";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";
import KeyboardArrowRight from "@mui/icons-material/KeyboardArrowRight";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import tryParseEnum from "@/framework/data/tryParseEnum";

/**
 * Props for the TransactionWorkspaceListFrame component.
 */
interface TransactionWorkspaceListFrameProps {
  readonly data: Transaction[] | null;
  readonly totalCount: number | null;
  readonly selectedTransactionId: string | null;
}

/**
 * Component that displays the top-level transaction ledger.
 */
const TransactionWorkspaceListFrame = function ({
  data,
  totalCount,
  selectedTransactionId,
}: TransactionWorkspaceListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();
  const theme = useTheme();
  const shouldOpenDetailsPage = useMediaQuery(theme.breakpoints.down("xl"));

  const accountingPeriodIdsParamName = "accountingPeriodIds";
  const accountIdsParamName = "accountIds";
  const fundIdsParamName = "fundIds";
  const selectedTransactionIdParamName = "selectedTransactionId";
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

  const toggleSelection = function (transactionId: string): void {
    replaceSearchParams((params) => {
      const currentlySelectedTransactionId = params.get(
        selectedTransactionIdParamName,
      );
      if (currentlySelectedTransactionId === transactionId) {
        params.delete(selectedTransactionIdParamName);
        return;
      }
      params.set(selectedTransactionIdParamName, transactionId);
    });
  };

  const openTransaction = function (transactionId: string): void {
    const params = new URLSearchParams(searchParams.toString());
    params.set(selectedTransactionIdParamName, transactionId);
    const query = params.toString();
    router.push(
      query === ""
        ? `${pathname}/${transactionId}`
        : `${pathname}/${transactionId}?${query}`,
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
      name: "selected",
      headerContent: "",
      getBodyContent: (transaction) => (
        <Box sx={{ display: "flex", justifyContent: "center" }}>
          <Checkbox
            checked={selectedTransactionId === transaction.id}
            onClick={(event) => {
              event.stopPropagation();
              toggleSelection(transaction.id);
            }}
            slotProps={{
              input: {
                "aria-label": `Select ${transaction.id}`,
              },
            }}
          />
        </Box>
      ),
      alignment: "center",
      minWidth: 0,
      maxWidth: 0,
      sx: { display: { xs: "none", xl: "table-cell" } },
    },
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
      sx: { display: { xs: "table-cell", xl: "none" } },
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
            if (shouldOpenDetailsPage) {
              openTransaction(transaction.id);
              return;
            }
            toggleSelection(transaction.id);
          }}
          isRowSelected={(transaction) =>
            transaction.id === selectedTransactionId
          }
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
