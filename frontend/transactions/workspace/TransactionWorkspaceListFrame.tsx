"use client";

import { Button, Checkbox, Paper, Stack, Typography } from "@mui/material";
import { type Transaction, TransactionSortOrder } from "@/transactions/types";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";
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
    router.replace(`${pathname}?${params.toString()}`, { scroll: false });
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

  const currentSort = tryParseEnum(
    TransactionSortOrder,
    searchParams.get(sortParamName) ?? "",
  );

  const columns: ColumnDefinition<Transaction>[] = [
    {
      name: "selected",
      headerContent: "",
      getBodyContent: (transaction) => (
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
      ),
      alignment: "center",
      minWidth: 0,
      maxWidth: 0,
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
      name: "accountingPeriod",
      headerContent: "Accounting Period",
      getBodyContent: (transaction: Transaction) =>
        transaction.accountingPeriodName,
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
        <Typography variant="h6" color="text.secondary">
          Transactions
        </Typography>
        <ListFrame<Transaction>
          columns={columns}
          getId={(transaction) => transaction.id}
          data={data ?? null}
          totalCount={totalCount ?? null}
          searchParamName=""
          pageParamName={pageParamName}
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
