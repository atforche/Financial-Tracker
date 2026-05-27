"use client";

import { AddCircleOutline, ArrowForwardIos } from "@mui/icons-material";
import { type Transaction, TransactionSortOrder } from "@/transactions/types";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Button } from "@mui/material";
import ColumnButton from "@/framework/listframe/ColumnButton";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import IconButton from "@/framework/listframe/IconButton";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import type { TransactionsViewSearchParams } from "@/transactions/TransactionsView";
import formatCurrency from "@/framework/formatCurrency";
import nameof from "@/framework/data/nameof";
import routes from "@/transactions/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";

/**
 * Props for the TransactionListFrame component.
 */
interface TransactionListFrameProps {
  readonly data: Transaction[] | null;
  readonly totalCount: number | null;
  readonly createActionHref: string;
  readonly createActionLabel: string;
  readonly selectedAccountingPeriodName: string | null;
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
const TransactionListFrame = function ({
  data,
  totalCount,
  createActionHref,
  createActionLabel,
  selectedAccountingPeriodName,
}: TransactionListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const searchParamName = nameof<TransactionsViewSearchParams>("search");
  const sortParamName = nameof<TransactionsViewSearchParams>("sort");
  const pageParamName = nameof<TransactionsViewSearchParams>("page");

  const setSort = function (sort: TransactionSortOrder | null): void {
    const params = new URLSearchParams(searchParams.toString());
    if (sort === null) {
      params.delete(sortParamName);
    } else {
      params.set(sortParamName, sort);
    }
    params.delete(pageParamName);
    router.replace(`${pathname}?${params.toString()}`);
  };

  const currentSort = tryParseEnum(
    TransactionSortOrder,
    searchParams.get(sortParamName) ?? "",
  );

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
      headerContent: (
        <IconButton
          label={createActionLabel}
          icon={<AddCircleOutline />}
          onClick={() => {
            router.push(createActionHref);
          }}
        />
      ),
      getBodyContent: (transaction: Transaction) => (
        <ColumnButton
          label="View"
          icon={<ArrowForwardIos />}
          onClick={() => {
            router.push(routes.detail({ id: transaction.id }, {}));
          }}
        />
      ),
      alignment: "right",
      minWidth: 90,
    },
  ];

  const initialEmptyDescription =
    selectedAccountingPeriodName !== null
      ? `No transactions have been recorded for ${selectedAccountingPeriodName} yet.`
      : "No transactions have been recorded yet.";

  return (
    <ListFrame<Transaction>
      columns={columns}
      getId={(transaction) => transaction.id}
      data={data ?? null}
      totalCount={totalCount ?? null}
      searchParamName={searchParamName}
      pageParamName={pageParamName}
      initialEmptyState={{
        title: "No transactions found",
        description: initialEmptyDescription,
        action: (
          <Button
            variant="contained"
            onClick={() => {
              router.push(createActionHref);
            }}
          >
            {createActionLabel}
          </Button>
        ),
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
              params.delete(searchParamName);
              params.delete(pageParamName);
              router.replace(`${pathname}?${params.toString()}`);
            }}
          >
            Clear search
          </Button>
        ),
      }}
    />
  );
};

export default TransactionListFrame;
