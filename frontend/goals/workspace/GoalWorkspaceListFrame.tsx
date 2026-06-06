"use client";

import { Button, Checkbox, Paper, Stack, Typography } from "@mui/material";
import { type Goal, GoalSortOrder } from "@/goals/types";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import tryParseEnum from "@/framework/data/tryParseEnum";

/**
 * Props for the GoalWorkspaceListFrame component.
 */
interface GoalWorkspaceListFrameProps {
  readonly data: Goal[] | null;
  readonly totalCount: number | null;
  readonly selectedGoalId: string | null;
}

/**
 * Component that displays the top-level goal list.
 */
const GoalWorkspaceListFrame = function ({
  data,
  totalCount,
  selectedGoalId,
}: GoalWorkspaceListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const accountingPeriodIdParamName = "accountingPeriodId";
  const fundIdParamName = "fundId";
  const selectedGoalIdParamName = "selectedGoalId";
  const sortParamName = "sort";
  const pageParamName = "page";

  const replaceSearchParams = function (
    update: (params: URLSearchParams) => void,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    update(params);
    const nextQuery = params.toString();
    router.replace(nextQuery === "" ? pathname : `${pathname}?${nextQuery}`);
  };

  const setSort = function (sort: GoalSortOrder | null): void {
    replaceSearchParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
      params.delete(pageParamName);
    });
  };

  const toggleSelection = function (goalId: string): void {
    replaceSearchParams((params) => {
      const currentlySelectedGoalId = params.get(selectedGoalIdParamName);
      if (currentlySelectedGoalId === goalId) {
        params.delete(selectedGoalIdParamName);
        return;
      }
      params.set(selectedGoalIdParamName, goalId);
    });
  };

  const currentSort = tryParseEnum(
    GoalSortOrder,
    searchParams.get(sortParamName) ?? "",
  );

  const columns: ColumnDefinition<Goal>[] = [
    {
      name: "selected",
      headerContent: "",
      getBodyContent: (goal) => (
        <Checkbox
          checked={selectedGoalId === goal.id}
          onClick={(event) => {
            event.stopPropagation();
            toggleSelection(goal.id);
          }}
          slotProps={{
            input: {
              "aria-label": `Select ${goal.fundName} goal`,
            },
          }}
        />
      ),
      alignment: "center",
      minWidth: 0,
      maxWidth: 0,
    },
    {
      name: "accountingPeriod",
      headerContent: "Accounting Period",
      getBodyContent: (goal) => goal.accountingPeriodName,
      sortType:
        currentSort === GoalSortOrder.AccountingPeriod
          ? ColumnSortType.Ascending
          : currentSort === GoalSortOrder.AccountingPeriodDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(GoalSortOrder.AccountingPeriod);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(GoalSortOrder.AccountingPeriodDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 180,
    },
    {
      name: "fund",
      headerContent: "Fund",
      getBodyContent: (goal) => goal.fundName,
      sortType:
        currentSort === GoalSortOrder.Fund
          ? ColumnSortType.Ascending
          : currentSort === GoalSortOrder.FundDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(GoalSortOrder.Fund);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(GoalSortOrder.FundDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 170,
    },
    {
      name: "goalType",
      headerContent: "Goal Type",
      getBodyContent: (goal) => goal.goalType,
      minWidth: 140,
    },
    {
      name: "goalAmount",
      headerContent: "Goal Amount",
      getBodyContent: (goal) => formatCurrency(goal.goalAmount),
      sortType:
        currentSort === GoalSortOrder.GoalAmount
          ? ColumnSortType.Ascending
          : currentSort === GoalSortOrder.GoalAmountDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(GoalSortOrder.GoalAmount);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(GoalSortOrder.GoalAmountDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 150,
    },
    {
      name: "remainingAmountToAssign",
      headerContent: "Remaining To Assign",
      getBodyContent: (goal) => formatCurrency(goal.remainingAmountToAssign),
      alignment: "right",
      minWidth: 180,
    },
    {
      name: "remainingAmountToSpend",
      headerContent: "Remaining To Spend",
      getBodyContent: (goal) => formatCurrency(goal.remainingAmountToSpend),
      alignment: "right",
      minWidth: 170,
    },
  ];

  return (
    <Paper
      sx={{
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        p: { xs: 2, md: 2.5 },
      }}
    >
      <Stack spacing={2.5}>
        <Typography variant="h6" color="text.secondary">
          Goals
        </Typography>
        <ListFrame<Goal>
          columns={columns}
          getId={(goal) => goal.id}
          data={data ?? null}
          totalCount={totalCount ?? null}
          searchParamName=""
          pageParamName={pageParamName}
          initialEmptyState={{
            title: "No goals found",
            description: "No goals have been recorded yet.",
            action: null,
          }}
          filteredEmptyState={{
            title: "No goals match this search",
            description:
              "Try a different accounting period, fund, or goal amount filter, or clear the current filters to see all matching goals.",
            action: (
              <Button
                variant="contained"
                onClick={() => {
                  replaceSearchParams((params) => {
                    params.delete(accountingPeriodIdParamName);
                    params.delete(fundIdParamName);
                    params.delete(pageParamName);
                    params.delete(selectedGoalIdParamName);
                  });
                }}
              >
                Clear search
              </Button>
            ),
          }}
        />
      </Stack>
    </Paper>
  );
};

export default GoalWorkspaceListFrame;
