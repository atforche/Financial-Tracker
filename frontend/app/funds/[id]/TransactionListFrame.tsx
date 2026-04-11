"use client";

import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import ArrowForwardIos from "@mui/icons-material/ArrowForwardIos";
import ColumnButton from "@/framework/listframe/ColumnButton";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import { FundTransactionSortOrder } from "@/data/fundTypes";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import type { Transaction } from "@/data/transactionTypes";
import formatCurrency from "@/framework/formatCurrency";
import tryParseEnum from "@/data/tryParseEnum";

/**
 * Interface representing a fund for the purpose of displaying transactions.
 */
interface Fund {
  id: string;
}

/**
 * Props for the TransactionListFrame component.
 */
interface TransactionListFrameProps {
  readonly fund: Fund;
  readonly data: Transaction[] | null;
  readonly totalCount: number | null;
  readonly accountingPeriod?: AccountingPeriod | null;
}

/**
 * Gets the change in balance for a fund from a transaction.
 */
const getChangeInBalance = function (
  fund: Fund,
  transaction: Transaction,
): number {
  const previousBalance =
    transaction.previousFundBalances.find((fb) => fb.fundId === fund.id)
      ?.postedBalance ?? 0;
  const newBalance =
    transaction.newFundBalances.find((fb) => fb.fundId === fund.id)
      ?.postedBalance ?? 0;
  return newBalance - previousBalance;
};

/**
 * Gets the URL to view a transaction, including appropriate query parameters to maintain context of the fund.
 */
const getViewTransactionUrl = function (
  transaction: Transaction,
  fund: Fund,
  accountingPeriod: AccountingPeriod | null,
): string {
  if (accountingPeriod !== null) {
    return `/transactions/${transaction.id}?accountingPeriodId=${accountingPeriod.id}&fundId=${fund.id}`;
  }
  return `/transactions/${transaction.id}?fundId=${fund.id}`;
};

/**
 * Component that displays the list of transactions for a fund.
 */
const TransactionListFrame = function ({
  fund,
  data,
  totalCount,
  accountingPeriod,
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
              getViewTransactionUrl(
                transaction,
                fund,
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
