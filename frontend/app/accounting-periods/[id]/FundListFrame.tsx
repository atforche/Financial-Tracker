"use client";

import {
  type AccountingPeriodFund,
  AccountingPeriodFundSortOrder,
} from "@/data/fundTypes";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import AddCircleOutline from "@mui/icons-material/AddCircleOutline";
import ArrowForwardIos from "@mui/icons-material/ArrowForwardIos";
import ColumnButton from "@/framework/listframe/ColumnButton";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import IconButton from "@/framework/listframe/IconButton";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import tryParseEnum from "@/data/tryParseEnum";

/**
 * Props for the FundListFrame component.
 */
interface FundListFrameProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly data: AccountingPeriodFund[] | null;
  readonly totalCount: number | null;
}

/**
 * Component that displays the list of funds associated with an accounting period.
 */
const FundListFrame = function ({
  accountingPeriod,
  data,
  totalCount,
}: FundListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const setSort = function (sort: AccountingPeriodFundSortOrder | null): void {
    const params = new URLSearchParams(searchParams.toString());
    if (sort === null) {
      params.delete("fundSort");
    } else {
      params.set("fundSort", sort);
    }
    router.replace(`${pathname}?${params.toString()}`);
  };

  const currentSort = tryParseEnum(
    AccountingPeriodFundSortOrder,
    searchParams.get("fundSort") ?? "",
  );

  const columns: ColumnDefinition<AccountingPeriodFund>[] = [
    {
      name: "name",
      headerContent: "Name",
      getBodyContent: (fund: AccountingPeriodFund) => fund.name,
      sortType:
        currentSort === AccountingPeriodFundSortOrder.Name
          ? ColumnSortType.Ascending
          : currentSort === AccountingPeriodFundSortOrder.NameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodFundSortOrder.Name);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodFundSortOrder.NameDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "type",
      headerContent: "Type",
      getBodyContent: (fund: AccountingPeriodFund) => fund.type,
      sortType:
        currentSort === AccountingPeriodFundSortOrder.Type
          ? ColumnSortType.Ascending
          : currentSort === AccountingPeriodFundSortOrder.TypeDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodFundSortOrder.Type);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodFundSortOrder.TypeDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "openingBalance",
      headerContent: "Opening Balance",
      getBodyContent: (fund: AccountingPeriodFund) =>
        formatCurrency(fund.openingBalance.postedBalance),
      sortType:
        currentSort === AccountingPeriodFundSortOrder.OpeningBalance
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodFundSortOrder.OpeningBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodFundSortOrder.OpeningBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodFundSortOrder.OpeningBalanceDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 150,
    },
    {
      name: "amountAssigned",
      headerContent: "Amount Assigned",
      getBodyContent: (fund: AccountingPeriodFund) =>
        formatCurrency(fund.amountAssigned),
      sortType:
        currentSort === AccountingPeriodFundSortOrder.AmountAssigned
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodFundSortOrder.AmountAssignedDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodFundSortOrder.AmountAssigned);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodFundSortOrder.AmountAssignedDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 150,
    },
    {
      name: "amountSpent",
      headerContent: "Amount Spent",
      getBodyContent: (fund: AccountingPeriodFund) =>
        formatCurrency(fund.amountSpent),
      sortType:
        currentSort === AccountingPeriodFundSortOrder.AmountSpent
          ? ColumnSortType.Ascending
          : currentSort === AccountingPeriodFundSortOrder.AmountSpentDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodFundSortOrder.AmountSpent);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodFundSortOrder.AmountSpentDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 150,
    },
    {
      name: "closingBalance",
      headerContent: "Closing Balance",
      getBodyContent: (fund: AccountingPeriodFund) =>
        formatCurrency(fund.closingBalance.postedBalance),
      sortType:
        currentSort === AccountingPeriodFundSortOrder.ClosingBalance
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodFundSortOrder.ClosingBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodFundSortOrder.ClosingBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodFundSortOrder.ClosingBalanceDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 150,
    },
    {
      name: "goalAmount",
      headerContent: "Goal Amount",
      getBodyContent: (fund: AccountingPeriodFund) =>
        fund.goalAmount === null ? "-" : formatCurrency(fund.goalAmount),
      alignment: "right",
      minWidth: 130,
    },
    {
      name: "isGoalMet",
      headerContent: "Goal Met",
      getBodyContent: (fund: AccountingPeriodFund) =>
        fund.isGoalMet === null ? "-" : fund.isGoalMet ? "Yes" : "No",
      alignment: "center",
      minWidth: 110,
    },
    {
      name: "actions",
      headerContent: (
        <IconButton
          label="Add"
          icon={<AddCircleOutline />}
          onClick={() => {
            router.push(
              `/funds/create?accountingPeriodId=${accountingPeriod.id}`,
            );
          }}
          disabled={!accountingPeriod.isOpen}
        />
      ),
      getBodyContent: (fund: AccountingPeriodFund) => (
        <ColumnButton
          label="View"
          icon={<ArrowForwardIos />}
          onClick={() => {
            router.push(`${pathname}/funds/${fund.id}`);
          }}
        />
      ),
      alignment: "right",
      minWidth: 90,
    },
  ];

  return (
    <ListFrame<AccountingPeriodFund>
      columns={columns}
      getId={(fund) => fund.id}
      data={data ?? null}
      totalCount={totalCount ?? null}
    />
  );
};

export default FundListFrame;
