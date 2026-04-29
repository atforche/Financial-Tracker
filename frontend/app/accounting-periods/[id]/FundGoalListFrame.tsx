"use client";

import {
  AccountingPeriodFundGoalSortOrder,
  type FundGoal,
} from "@/data/fundTypes";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import ArrowForwardIos from "@mui/icons-material/ArrowForwardIos";
import ColumnButton from "@/framework/listframe/ColumnButton";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/framework/routes";
import tryParseEnum from "@/data/tryParseEnum";

/**
 * Props for the FundGoalListFrame component.
 */
interface FundGoalListFrameProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly data: FundGoal[] | null;
  readonly totalCount: number | null;
}

/**
 * Component that displays the list of fund goals associated with an accounting period.
 */
const FundGoalListFrame = function ({
  accountingPeriod,
  data,
  totalCount,
}: FundGoalListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const setSort = function (
    sort: AccountingPeriodFundGoalSortOrder | null,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    if (sort === null) {
      params.delete("fundGoalSort");
    } else {
      params.set("fundGoalSort", sort);
    }
    router.replace(`${pathname}?${params.toString()}`);
  };

  const currentSort = tryParseEnum(
    AccountingPeriodFundGoalSortOrder,
    searchParams.get("fundGoalSort") ?? "",
  );

  const columns: ColumnDefinition<FundGoal>[] = [
    {
      name: "name",
      headerContent: "Name",
      getBodyContent: (fundGoal: FundGoal) => fundGoal.fundName,
      sortType:
        currentSort === AccountingPeriodFundGoalSortOrder.Name
          ? ColumnSortType.Ascending
          : currentSort === AccountingPeriodFundGoalSortOrder.NameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodFundGoalSortOrder.Name);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodFundGoalSortOrder.NameDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "goalType",
      headerContent: "Goal Type",
      getBodyContent: (fundGoal: FundGoal) => fundGoal.goalType,
      sortType:
        currentSort === AccountingPeriodFundGoalSortOrder.Type
          ? ColumnSortType.Ascending
          : currentSort === AccountingPeriodFundGoalSortOrder.TypeDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodFundGoalSortOrder.Type);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodFundGoalSortOrder.TypeDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "goalAmount",
      headerContent: "Goal Amount",
      getBodyContent: (fundGoal: FundGoal) =>
        formatCurrency(fundGoal.goalAmount),
      sortType:
        currentSort === AccountingPeriodFundGoalSortOrder.GoalAmount
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodFundGoalSortOrder.GoalAmountDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodFundGoalSortOrder.GoalAmount);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodFundGoalSortOrder.GoalAmountDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 150,
    },
    {
      name: "remainingAmountToAssign",
      headerContent: "Remaining Amount to Assign",
      getBodyContent: (fundGoal: FundGoal) =>
        formatCurrency(fundGoal.remainingAmountToAssign),
      sortType:
        currentSort ===
        AccountingPeriodFundGoalSortOrder.RemainingAmountToAssign
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodFundGoalSortOrder.RemainingAmountToAssignDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodFundGoalSortOrder.RemainingAmountToAssign);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(
            AccountingPeriodFundGoalSortOrder.RemainingAmountToAssignDescending,
          );
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 150,
    },
    {
      name: "isAssignmentGoalMet",
      headerContent: "Assignment Goal Met",
      getBodyContent: (fundGoal: FundGoal) =>
        fundGoal.isAssignmentGoalMet ? "Yes" : "No",
      sortType:
        currentSort === AccountingPeriodFundGoalSortOrder.IsAssignmentGoalMet
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodFundGoalSortOrder.IsAssignmentGoalMetDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodFundGoalSortOrder.IsAssignmentGoalMet);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(
            AccountingPeriodFundGoalSortOrder.IsAssignmentGoalMetDescending,
          );
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "remainingAmountToSpend",
      headerContent: "Remaining Amount to Spend",
      getBodyContent: (fundGoal: FundGoal) =>
        formatCurrency(fundGoal.remainingAmountToSpend),
      sortType:
        currentSort === AccountingPeriodFundGoalSortOrder.RemainingAmountToSpend
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodFundGoalSortOrder.RemainingAmountToSpendDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodFundGoalSortOrder.RemainingAmountToSpend);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(
            AccountingPeriodFundGoalSortOrder.RemainingAmountToSpendDescending,
          );
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 150,
    },
    {
      name: "isSpendingGoalMet",
      headerContent: "Spending Goal Met",
      getBodyContent: (fundGoal: FundGoal) =>
        fundGoal.isSpendingGoalMet ? "Yes" : "No",
      sortType:
        currentSort === AccountingPeriodFundGoalSortOrder.IsSpendingGoalMet
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodFundGoalSortOrder.IsSpendingGoalMetDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodFundGoalSortOrder.IsSpendingGoalMet);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(
            AccountingPeriodFundGoalSortOrder.IsSpendingGoalMetDescending,
          );
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "actions",
      headerContent: "",
      getBodyContent: (fundGoal: FundGoal) => (
        <ColumnButton
          label="View"
          icon={<ArrowForwardIos />}
          onClick={() => {
            router.push(
              routes.accountingPeriods.fundDetail(
                accountingPeriod.id,
                fundGoal.fundId,
              ),
            );
          }}
        />
      ),
      alignment: "right",
      minWidth: 90,
    },
  ];

  return (
    <ListFrame<FundGoal>
      columns={columns}
      getId={(fundGoal) => fundGoal.id}
      data={data ?? null}
      totalCount={totalCount ?? null}
    />
  );
};

export default FundGoalListFrame;
