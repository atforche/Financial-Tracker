"use client";

import {
  type AccountingPeriod,
  AccountingPeriodGoalSortOrder,
} from "@/accounting-periods/types";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { AccountingPeriodViewSearchParams } from "@/accounting-periods/AccountingPeriodView";
import ArrowForwardIos from "@mui/icons-material/ArrowForwardIos";
import ColumnButton from "@/framework/listframe/ColumnButton";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { Goal } from "@/goals/types";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import nameof from "@/framework/data/nameof";
import routes from "@/accounting-periods/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";

/**
 * Props for the AccountingPeriodGoalListFrame component.
 */
interface AccountingPeriodGoalListFrameProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly data: Goal[] | null;
  readonly totalCount: number | null;
}

/**
 * Component that displays the list of goals associated with an accounting period.
 */
const AccountingPeriodGoalListFrame = function ({
  accountingPeriod,
  data,
  totalCount,
}: AccountingPeriodGoalListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();
  const sortSearchParamName =
    nameof<AccountingPeriodViewSearchParams>("goalSort");

  const setSort = function (sort: AccountingPeriodGoalSortOrder | null): void {
    const params = new URLSearchParams(searchParams.toString());
    if (sort === null) {
      params.delete(sortSearchParamName);
    } else {
      params.set(sortSearchParamName, sort);
    }
    router.replace(`${pathname}?${params.toString()}`);
  };

  const currentSort = tryParseEnum(
    AccountingPeriodGoalSortOrder,
    searchParams.get(sortSearchParamName) ?? "",
  );

  const columns: ColumnDefinition<Goal>[] = [
    {
      name: "name",
      headerContent: "Name",
      getBodyContent: (goal: Goal) => goal.fundName,
      sortType:
        currentSort === AccountingPeriodGoalSortOrder.Name
          ? ColumnSortType.Ascending
          : currentSort === AccountingPeriodGoalSortOrder.NameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodGoalSortOrder.Name);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodGoalSortOrder.NameDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "goalType",
      headerContent: "Goal Type",
      getBodyContent: (goal: Goal) => goal.goalType,
      sortType:
        currentSort === AccountingPeriodGoalSortOrder.Type
          ? ColumnSortType.Ascending
          : currentSort === AccountingPeriodGoalSortOrder.TypeDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodGoalSortOrder.Type);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodGoalSortOrder.TypeDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "goalAmount",
      headerContent: "Goal Amount",
      getBodyContent: (goal: Goal) => formatCurrency(goal.goalAmount),
      sortType:
        currentSort === AccountingPeriodGoalSortOrder.GoalAmount
          ? ColumnSortType.Ascending
          : currentSort === AccountingPeriodGoalSortOrder.GoalAmountDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodGoalSortOrder.GoalAmount);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodGoalSortOrder.GoalAmountDescending);
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
      getBodyContent: (goal: Goal) =>
        formatCurrency(goal.remainingAmountToAssign),
      sortType:
        currentSort === AccountingPeriodGoalSortOrder.RemainingAmountToAssign
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodGoalSortOrder.RemainingAmountToAssignDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodGoalSortOrder.RemainingAmountToAssign);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(
            AccountingPeriodGoalSortOrder.RemainingAmountToAssignDescending,
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
      getBodyContent: (goal: Goal) => (goal.isAssignmentGoalMet ? "Yes" : "No"),
      sortType:
        currentSort === AccountingPeriodGoalSortOrder.IsAssignmentGoalMet
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodGoalSortOrder.IsAssignmentGoalMetDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodGoalSortOrder.IsAssignmentGoalMet);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodGoalSortOrder.IsAssignmentGoalMetDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "remainingAmountToSpend",
      headerContent: "Remaining Amount to Spend",
      getBodyContent: (goal: Goal) =>
        formatCurrency(goal.remainingAmountToSpend),
      sortType:
        currentSort === AccountingPeriodGoalSortOrder.RemainingAmountToSpend
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodGoalSortOrder.RemainingAmountToSpendDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodGoalSortOrder.RemainingAmountToSpend);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(
            AccountingPeriodGoalSortOrder.RemainingAmountToSpendDescending,
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
      getBodyContent: (goal: Goal) => (goal.isSpendingGoalMet ? "Yes" : "No"),
      sortType:
        currentSort === AccountingPeriodGoalSortOrder.IsSpendingGoalMet
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodGoalSortOrder.IsSpendingGoalMetDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodGoalSortOrder.IsSpendingGoalMet);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodGoalSortOrder.IsSpendingGoalMetDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "actions",
      headerContent: "",
      getBodyContent: (goal: Goal) => (
        <ColumnButton
          label="View"
          icon={<ArrowForwardIos />}
          onClick={() => {
            router.push(
              routes.fundDetail(
                {
                  id: accountingPeriod.id,
                  fundId: goal.fundId,
                },
                {},
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
    <ListFrame<Goal>
      columns={columns}
      getId={(goal) => goal.id}
      data={data ?? null}
      totalCount={totalCount ?? null}
      pageSearchParamName={nameof<AccountingPeriodViewSearchParams>("page")}
    />
  );
};

export default AccountingPeriodGoalListFrame;
