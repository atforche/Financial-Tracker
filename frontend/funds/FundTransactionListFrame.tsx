"use client";

import routes, { withQuery } from "@/framework/routes";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import ArrowForwardIos from "@mui/icons-material/ArrowForwardIos";
import ColumnButton from "@/framework/listframe/ColumnButton";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import { FundTransactionSortOrder } from "@/funds/types";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import type { Transaction } from "@/transactions/types";
import formatCurrency from "@/framework/formatCurrency";
import tryParseEnum from "@/framework/data/tryParseEnum";

/**
 * Interface representing a fund for the purpose of displaying transactions.
 */
interface Fund {
  id: string;
}

interface FundBalanceLike {
  readonly fundId: string;
  readonly postedBalance: number;
}

type TransactionWithFundBalances = Transaction & {
  readonly previousFundBalances?: FundBalanceLike[];
  readonly newFundBalances?: FundBalanceLike[];
};

/**
 * Props for the TransactionListFrame component.
 */
interface TransactionListFrameProps {
  readonly fund: Fund;
  readonly accountingPeriodId?: string | null;
  readonly data: Transaction[] | null;
  readonly totalCount: number | null;
}

/**
 * Gets the change in balance for a fund from a transaction.
 */
const getChangeInBalance = function (
  fund: Fund,
  transaction: Transaction,
): number {
  const transactionWithFundBalances =
    transaction as TransactionWithFundBalances;
  const previousBalance =
    transactionWithFundBalances.previousFundBalances?.find(
      (fundBalance) => fundBalance.fundId === fund.id,
    )?.postedBalance ?? 0;
  const newBalance =
    transactionWithFundBalances.newFundBalances?.find(
      (fundBalance) => fundBalance.fundId === fund.id,
    )?.postedBalance ?? 0;
  return newBalance - previousBalance;
};

/**
 * Component that displays the list of transactions for a fund.
 */
const FundTransactionListFrame = function ({
  fund,
  accountingPeriodId = null,
  data,
  totalCount,
}: TransactionListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const setSort = function (sort: FundTransactionSortOrder | null): void {
    const params = new URLSearchParams(searchParams.toString());
    if (sort === null) {
      params.delete("transactionSort");
    } else {
      params.set("transactionSort", sort);
    }
    router.replace(`${pathname}?${params.toString()}`);
  };

  const currentSort = tryParseEnum(
    FundTransactionSortOrder,
    searchParams.get("transactionSort") ?? "",
  );

  const columns: ColumnDefinition<Transaction>[] = [
    {
      name: "date",
      headerContent: "Date",
      getBodyContent: (transaction: Transaction) => transaction.date,
      sortType:
        currentSort === FundTransactionSortOrder.Date
          ? ColumnSortType.Ascending
          : currentSort === FundTransactionSortOrder.DateDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(FundTransactionSortOrder.Date);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(FundTransactionSortOrder.DateDescending);
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
        currentSort === FundTransactionSortOrder.Location
          ? ColumnSortType.Ascending
          : currentSort === FundTransactionSortOrder.LocationDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(FundTransactionSortOrder.Location);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(FundTransactionSortOrder.LocationDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "changeInBalance",
      headerContent: "Change in Balance",
      getBodyContent: (transaction: Transaction): JSX.Element => {
        const change = getChangeInBalance(fund, transaction);
        return change >= 0 ? (
          <span style={{ color: "green" }}>
            {`+ ${formatCurrency(change)}`}
          </span>
        ) : (
          <span style={{ color: "red" }}>
            {`- ${formatCurrency(Math.abs(change))}`}
          </span>
        );
      },
      sortType:
        currentSort === FundTransactionSortOrder.ChangeInBalance
          ? ColumnSortType.Ascending
          : currentSort === FundTransactionSortOrder.ChangeInBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(FundTransactionSortOrder.ChangeInBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(FundTransactionSortOrder.ChangeInBalanceDescending);
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
              withQuery(routes.transactions.detail(transaction.id), {
                accountingPeriodId,
                fundId: fund.id,
              }),
            );
          }}
        />
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
    />
  );
};

export default FundTransactionListFrame;
