"use client";

import {
  AccountTransactionSortOrder,
  type AccountType,
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
 * Interface representing an account for the purpose of displaying transactions.
 */
interface Account {
  id: string;
  type: AccountType;
}

/**
 * Props for the TransactionListFrame component.
 */
interface TransactionListFrameProps {
  readonly account: Account;
  readonly data: Transaction[] | null;
  readonly totalCount: number | null;
  readonly accountingPeriod?: AccountingPeriod | null;
}

/**
 * Calculate the change in balance for the given account and transaction.
 */
const getChangeInBalance = function (
  account: Account,
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
 * Gets the URL to view a transaction, including appropriate query parameters to maintain context of the account and accounting period if applicable.
 */
const getViewTransactionUrl = function (
  transaction: Transaction,
  account: Account,
  accountingPeriod: AccountingPeriod | null,
): string {
  if (accountingPeriod !== null) {
    return `/transactions/${transaction.id}?accountingPeriodId=${accountingPeriod.id}&accountId=${account.id}`;
  }
  return `/transactions/${transaction.id}?accountId=${account.id}`;
};

/**
 * Component that displays the list of transactions for an account.
 */
const TransactionListFrame = function ({
  account,
  data,
  totalCount,
  accountingPeriod,
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
              getViewTransactionUrl(
                transaction,
                account,
                accountingPeriod ?? null,
              ),
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
