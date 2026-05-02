"use client";

import {
  AccountTransactionSortOrder,
  type AccountType,
  isPositiveChangeInBalance,
} from "@/accounts/types";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { AccountViewSearchParams } from "@/accounts/AccountView";
import ArrowForwardIos from "@mui/icons-material/ArrowForwardIos";
import ColumnButton from "@/framework/listframe/ColumnButton";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import type { Transaction } from "@/transactions/types";
import formatCurrency from "@/framework/formatCurrency";
import nameof from "@/framework/data/nameof";
import tryParseEnum from "@/framework/data/tryParseEnum";

/**
 * Interface representing an account for the purpose of displaying transactions.
 */
interface Account {
  id: string;
  type: AccountType;
}

const getDebitAccountId = function (transaction: Transaction): string | null {
  return "debitAccount" in transaction && transaction.debitAccount !== null
    ? transaction.debitAccount.accountId
    : null;
};

const getCreditAccountId = function (transaction: Transaction): string | null {
  return "creditAccount" in transaction && transaction.creditAccount !== null
    ? transaction.creditAccount.accountId
    : null;
};

/**
 * Props for the AccountTransactionListFrame component.
 */
interface AccountTransactionListFrameProps {
  readonly account: Account;
  readonly data: Transaction[] | null;
  readonly totalCount: number | null;
}

/**
 * Calculate the change in balance for the given account and transaction.
 */
const getChangeInBalance = function (
  account: Account,
  transaction: Transaction,
): number {
  const debitAccountId = getDebitAccountId(transaction);
  const creditAccountId = getCreditAccountId(transaction);

  if (debitAccountId === account.id && creditAccountId === account.id) {
    return 0;
  }
  if (debitAccountId === account.id) {
    return -1 * transaction.amount;
  }
  return transaction.amount;
};

/**
 * Component that displays the list of transactions for an account.
 */
const AccountTransactionListFrame = function ({
  account,
  data,
  totalCount,
}: AccountTransactionListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();
  const sortSearchParamName = nameof<AccountViewSearchParams>("sort");

  const setSort = function (sort: AccountTransactionSortOrder | null): void {
    const params = new URLSearchParams(searchParams.toString());
    if (sort === null) {
      params.delete(sortSearchParamName);
    } else {
      params.set(sortSearchParamName, sort);
    }
    router.replace(`${pathname}?${params.toString()}`);
  };

  const currentSort = tryParseEnum(
    AccountTransactionSortOrder,
    searchParams.get(sortSearchParamName) ?? "",
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
      getBodyContent: () => (
        <ColumnButton label="View" icon={<ArrowForwardIos />} onClick={null} />
      ),
      alignment: "right",
      minWidth: 90,
    },
  ];

  return (
    <ListFrame<Transaction>
      columns={columns}
      getId={(transaction) => transaction.id}
      data={data ?? null}
      totalCount={totalCount ?? null}
      pageSearchParamName={nameof<AccountViewSearchParams>("page")}
    />
  );
};

export default AccountTransactionListFrame;
