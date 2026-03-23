"use client";

import {
  AccountingPeriodTransactionSortOrder,
  type Transaction,
} from "@/data/transactionTypes";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import AddCircleOutline from "@mui/icons-material/AddCircleOutline";
import ArrowForwardIos from "@mui/icons-material/ArrowForwardIos";
import ColumnButton from "@/framework/listframe/ColumnButton";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnHeaderButton from "@/framework/listframe/ColumnHeaderButton";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import tryParseEnum from "@/data/tryParseEnum";

/**
 * Props for the TransactionListFrame component.
 */
interface TransactionListFrameProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly data: Transaction[] | null;
  readonly totalCount: number | null;
}

/**
 * Component that displays the list of transactions associated with an accounting period.
 */
const TransactionListFrame = function ({
  accountingPeriod,
  data,
  totalCount,
}: TransactionListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const setSort = function (
    sort: AccountingPeriodTransactionSortOrder | null,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    if (sort === null) {
      params.delete("transactionSort");
    } else {
      params.set("transactionSort", sort);
    }
    router.replace(`${pathname}?${params.toString()}`);
  };

  const currentSort = tryParseEnum(
    AccountingPeriodTransactionSortOrder,
    searchParams.get("transactionSort") ?? "",
  );

  const columns: ColumnDefinition<Transaction>[] = [
    {
      name: "date",
      headerContent: "Date",
      getBodyContent: (transaction: Transaction) => transaction.date,
      sortType:
        currentSort === AccountingPeriodTransactionSortOrder.Date
          ? ColumnSortType.Ascending
          : currentSort === AccountingPeriodTransactionSortOrder.DateDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodTransactionSortOrder.Date);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodTransactionSortOrder.DateDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 125,
    },
    {
      name: "location",
      headerContent: "Location",
      getBodyContent: (transaction: Transaction) => transaction.location,
      sortType:
        currentSort === AccountingPeriodTransactionSortOrder.Location
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTransactionSortOrder.LocationDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodTransactionSortOrder.Location);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodTransactionSortOrder.LocationDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "debitAccount",
      headerContent: "Debit From",
      getBodyContent: (transaction: Transaction) =>
        transaction.debitAccount ? (
          <span style={{ color: "red" }}>
            {transaction.debitAccount.accountName}
          </span>
        ) : null,
      sortType:
        currentSort === AccountingPeriodTransactionSortOrder.DebitAccount
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTransactionSortOrder.DebitAccountDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodTransactionSortOrder.DebitAccount);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodTransactionSortOrder.DebitAccountDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 125,
    },
    {
      name: "creditAccount",
      headerContent: "Credit To",
      getBodyContent: (transaction: Transaction) =>
        transaction.creditAccount ? (
          <span style={{ color: "green" }}>
            {transaction.creditAccount.accountName}
          </span>
        ) : null,
      sortType:
        currentSort === AccountingPeriodTransactionSortOrder.CreditAccount
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTransactionSortOrder.CreditAccountDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodTransactionSortOrder.CreditAccount);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodTransactionSortOrder.CreditAccountDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 125,
    },
    {
      name: "amount",
      headerContent: "Amount",
      getBodyContent: (transaction: Transaction) =>
        formatCurrency(transaction.amount),
      sortType:
        currentSort === AccountingPeriodTransactionSortOrder.Amount
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodTransactionSortOrder.AmountDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodTransactionSortOrder.Amount);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodTransactionSortOrder.AmountDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
    },
    {
      name: "actions",
      headerContent: (
        <ColumnHeaderButton
          label="Add"
          icon={<AddCircleOutline />}
          onClick={() => {
            router.push(`${pathname}/transaction/create`);
          }}
          disabled={!accountingPeriod.isOpen}
        />
      ),
      getBodyContent: (transaction: Transaction) => (
        <ColumnButton
          label="View"
          icon={<ArrowForwardIos />}
          onClick={() => {
            router.push(`${pathname}/transaction/${transaction.id}`);
          }}
        />
      ),
      alignment: "right",
    },
  ];

  return (
    <ListFrame<Transaction>
      columns={columns}
      getId={(transaction) => transaction.id}
      data={data ?? null}
      totalCount={totalCount ?? null}
    />
  );
};

export default TransactionListFrame;
