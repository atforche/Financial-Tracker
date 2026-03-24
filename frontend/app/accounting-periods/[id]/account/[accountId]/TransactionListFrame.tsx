"use client";

import {
  AccountTransactionSortOrder,
  type AccountingPeriodAccount,
  isPositiveChangeInBalance,
} from "@/data/accountTypes";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import ArrowForwardIos from "@mui/icons-material/ArrowForwardIos";
import ColumnButton from "@/framework/listframe/ColumnButton";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import type { Transaction } from "@/data/transactionTypes";
import formatCurrency from "@/framework/formatCurrency";
import tryParseEnum from "@/data/tryParseEnum";

/**
 * Props for the TransactionListFrame component.
 */
interface TransactionListFrameProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly account: AccountingPeriodAccount;
  readonly data: Transaction[] | null;
  readonly totalCount: number | null;
}

/**
 * Calculate the change in balance for the given account and transaction.
 */
const getChangeInBalance = function (
  account: AccountingPeriodAccount,
  transaction: Transaction,
): number {
  if (
    transaction.debitAccount?.accountId === account.id &&
    transaction.creditAccount?.accountId === account.id
  ) {
    return 0;
  }
  if (transaction.debitAccount?.accountId === account.id) {
    return -1 * transaction.amount;
  }
  return transaction.amount;
};

/**
 * Component that displays the list of transactions associated with an account within an accounting period.
 */
const TransactionListFrame = function ({
  accountingPeriod,
  account,
  data,
  totalCount,
}: TransactionListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const setSort = function (sort: AccountTransactionSortOrder | null): void {
    const params = new URLSearchParams(searchParams.toString());
    if (sort === null) {
      params.delete("transactionSort");
    } else {
      params.set("transactionSort", sort);
    }
    router.replace(`${pathname}?${params.toString()}`);
  };

  const currentSort = tryParseEnum(
    AccountTransactionSortOrder,
    searchParams.get("transactionSort") ?? "",
  );

  const columns: ColumnDefinition<Transaction>[] = [
    {
      name: "date",
      headerContent: "Date",
      getBodyContent: (transaction: Transaction) => transaction.date,
      sortType:
        currentSort === AccountTransactionSortOrder.Date
          ? ColumnSortType.Ascending
          : currentSort === AccountTransactionSortOrder.DateDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountTransactionSortOrder.Date);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountTransactionSortOrder.DateDescending);
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
        currentSort === AccountTransactionSortOrder.Location
          ? ColumnSortType.Ascending
          : currentSort === AccountTransactionSortOrder.LocationDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountTransactionSortOrder.Location);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountTransactionSortOrder.LocationDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "changeInBalance",
      headerContent: "Change in Balance",
      getBodyContent: (transaction: Transaction) =>
        isPositiveChangeInBalance(
          account.type,
          getChangeInBalance(account, transaction),
        ) ? (
          <span style={{ color: "green" }}>
            {`+ ${formatCurrency(getChangeInBalance(account, transaction))}`}
          </span>
        ) : (
          <span style={{ color: "red" }}>
            {`- ${formatCurrency(Math.abs(getChangeInBalance(account, transaction)))}`}
          </span>
        ),
      sortType:
        currentSort === AccountTransactionSortOrder.ChangeInBalance
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountTransactionSortOrder.ChangeInBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountTransactionSortOrder.ChangeInBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountTransactionSortOrder.ChangeInBalanceDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
    },
    {
      name: "actions",
      headerContent: "",
      getBodyContent: (transaction: Transaction) => (
        <ColumnButton
          label="View"
          icon={<ArrowForwardIos />}
          onClick={() => {
            router.push(
              `/accounting-periods/${accountingPeriod.id}/transaction/${transaction.id}`,
            );
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
