"use client";

import {
  type AccountingPeriodFund,
  FundTransactionSortOrder,
} from "@/data/fundTypes";
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
  readonly fund: AccountingPeriodFund;
  readonly data: Transaction[] | null;
  readonly totalCount: number | null;
}

/**
 * Calculate the change in balance for the given fund and transaction.
 */
const getChangeInBalance = function (
  fund: AccountingPeriodFund,
  transaction: Transaction,
): number {
  const debitFundAmount =
    transaction.debitAccount?.fundAmounts.find(
      (fundAmount) => fundAmount.fundId === fund.id,
    )?.amount ?? 0;
  const creditFundAmount =
    transaction.creditAccount?.fundAmounts.find(
      (fundAmount) => fundAmount.fundId === fund.id,
    )?.amount ?? 0;
  return creditFundAmount - debitFundAmount;
};

/**
 * Component that displays the list of transactions associated with a fund within an accounting period.
 */
const TransactionListFrame = function ({
  accountingPeriod,
  fund,
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
      getBodyContent: (transaction: Transaction) =>
        getChangeInBalance(fund, transaction) >= 0 ? (
          <span style={{ color: "green" }}>
            {`+ ${formatCurrency(getChangeInBalance(fund, transaction))}`}
          </span>
        ) : (
          <span style={{ color: "red" }}>
            {`- ${formatCurrency(Math.abs(getChangeInBalance(fund, transaction)))}`}
          </span>
        ),
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
