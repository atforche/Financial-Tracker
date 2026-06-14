"use client";

import {
  type AssignmentGoal,
  AssignmentGoalSortOrder,
  type SpendingGoal,
  SpendingGoalSortOrder,
  formatAssignmentGoalType,
  formatSpendingGoalType,
} from "@/goals/types";
import { Button, Checkbox, Paper, Stack, Typography } from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import tryParseEnum from "@/framework/data/tryParseEnum";

type GoalWorkspaceListFrameProps =
  | {
      readonly view: "assignment";
      readonly data: AssignmentGoal[] | null;
      readonly totalCount: number | null;
      readonly selectedGoalId: string | null;
    }
  | {
      readonly view: "spending";
      readonly data: SpendingGoal[] | null;
      readonly totalCount: number | null;
      readonly selectedGoalId: string | null;
    };

/**
 * Component that displays the top-level goal list.
 */
const GoalWorkspaceListFrame = function ({
  view,
  data,
  totalCount,
  selectedGoalId,
}: GoalWorkspaceListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const accountingPeriodIdParamName = "accountingPeriodIds";
  const fundIdParamName = "fundIds";
  const selectedGoalIdParamName = "selectedGoalId";
  const sortParamName = "sort";
  const pageParamName = "page";

  const replaceSearchParams = function (
    update: (params: URLSearchParams) => void,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    update(params);
    const nextQuery = params.toString();
    router.replace(nextQuery === "" ? pathname : `${pathname}?${nextQuery}`, {
      scroll: false,
    });
  };

  const clearSearch = function (): void {
    replaceSearchParams((params) => {
      params.delete(accountingPeriodIdParamName);
      params.delete(fundIdParamName);
      params.delete(pageParamName);
      params.delete(selectedGoalIdParamName);
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

  if (view === "assignment") {
    const currentSort = tryParseEnum(
      AssignmentGoalSortOrder,
      searchParams.get(sortParamName) ?? "",
    );

    const setSort = function (sort: AssignmentGoalSortOrder | null): void {
      replaceSearchParams((params) => {
        if (sort === null) {
          params.delete(sortParamName);
        } else {
          params.set(sortParamName, sort);
        }
        params.delete(pageParamName);
      });
    };

    const columns: ColumnDefinition<AssignmentGoal>[] = [
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
                "aria-label": `Select ${goal.fundName} assignment goal`,
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
          currentSort === AssignmentGoalSortOrder.AccountingPeriod
            ? ColumnSortType.Ascending
            : currentSort === AssignmentGoalSortOrder.AccountingPeriodDescending
              ? ColumnSortType.Descending
              : null,
        onSort: (sortType: ColumnSortType | null): void => {
          if (sortType === ColumnSortType.Ascending) {
            setSort(AssignmentGoalSortOrder.AccountingPeriod);
          } else if (sortType === ColumnSortType.Descending) {
            setSort(AssignmentGoalSortOrder.AccountingPeriodDescending);
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
          currentSort === AssignmentGoalSortOrder.Fund
            ? ColumnSortType.Ascending
            : currentSort === AssignmentGoalSortOrder.FundDescending
              ? ColumnSortType.Descending
              : null,
        onSort: (sortType: ColumnSortType | null): void => {
          if (sortType === ColumnSortType.Ascending) {
            setSort(AssignmentGoalSortOrder.Fund);
          } else if (sortType === ColumnSortType.Descending) {
            setSort(AssignmentGoalSortOrder.FundDescending);
          } else {
            setSort(null);
          }
        },
        minWidth: 170,
      },
      {
        name: "type",
        headerContent: "Goal Type",
        getBodyContent: (goal) => formatAssignmentGoalType(goal.type),
        sortType:
          currentSort === AssignmentGoalSortOrder.Type
            ? ColumnSortType.Ascending
            : currentSort === AssignmentGoalSortOrder.TypeDescending
              ? ColumnSortType.Descending
              : null,
        onSort: (sortType: ColumnSortType | null): void => {
          if (sortType === ColumnSortType.Ascending) {
            setSort(AssignmentGoalSortOrder.Type);
          } else if (sortType === ColumnSortType.Descending) {
            setSort(AssignmentGoalSortOrder.TypeDescending);
          } else {
            setSort(null);
          }
        },
        minWidth: 170,
      },
      {
        name: "goalAmount",
        headerContent: "Goal Amount",
        getBodyContent: (goal) => formatCurrency(goal.goalAmount),
        sortType:
          currentSort === AssignmentGoalSortOrder.GoalAmount
            ? ColumnSortType.Ascending
            : currentSort === AssignmentGoalSortOrder.GoalAmountDescending
              ? ColumnSortType.Descending
              : null,
        onSort: (sortType: ColumnSortType | null): void => {
          if (sortType === ColumnSortType.Ascending) {
            setSort(AssignmentGoalSortOrder.GoalAmount);
          } else if (sortType === ColumnSortType.Descending) {
            setSort(AssignmentGoalSortOrder.GoalAmountDescending);
          } else {
            setSort(null);
          }
        },
        alignment: "right",
        minWidth: 150,
      },
      {
        name: "totalAmountToAssign",
        headerContent: "Assigned So Far",
        getBodyContent: (goal) => formatCurrency(goal.totalAmountToAssign),
        alignment: "right",
        minWidth: 160,
      },
      {
        name: "remainingAmountToAssign",
        headerContent: "Remaining To Assign",
        getBodyContent: (goal) => formatCurrency(goal.remainingAmountToAssign),
        alignment: "right",
        minWidth: 180,
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
            Assignment Goals
          </Typography>
          <ListFrame<AssignmentGoal>
            columns={columns}
            getId={(goal) => goal.id}
            data={data}
            totalCount={totalCount ?? null}
            searchParamName=""
            pageParamName={pageParamName}
            initialEmptyState={{
              title: "No assignment goals found",
              description:
                "No assignment goals match the current workspace view yet.",
              action: null,
            }}
            filteredEmptyState={{
              title: "No assignment goals match this search",
              description:
                "Try a different accounting period or fund filter, or clear the current filters to see all assignment goals.",
              action: (
                <Button variant="contained" onClick={clearSearch}>
                  Clear search
                </Button>
              ),
            }}
          />
        </Stack>
      </Paper>
    );
  }

  const currentSort = tryParseEnum(
    SpendingGoalSortOrder,
    searchParams.get(sortParamName) ?? "",
  );

  const setSort = function (sort: SpendingGoalSortOrder | null): void {
    replaceSearchParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
      params.delete(pageParamName);
    });
  };

  const columns: ColumnDefinition<SpendingGoal>[] = [
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
              "aria-label": `Select ${goal.fundName} spending goal`,
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
        currentSort === SpendingGoalSortOrder.AccountingPeriod
          ? ColumnSortType.Ascending
          : currentSort === SpendingGoalSortOrder.AccountingPeriodDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(SpendingGoalSortOrder.AccountingPeriod);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(SpendingGoalSortOrder.AccountingPeriodDescending);
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
        currentSort === SpendingGoalSortOrder.Fund
          ? ColumnSortType.Ascending
          : currentSort === SpendingGoalSortOrder.FundDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(SpendingGoalSortOrder.Fund);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(SpendingGoalSortOrder.FundDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 170,
    },
    {
      name: "type",
      headerContent: "Goal Type",
      getBodyContent: (goal) => formatSpendingGoalType(goal.type),
      sortType:
        currentSort === SpendingGoalSortOrder.Type
          ? ColumnSortType.Ascending
          : currentSort === SpendingGoalSortOrder.TypeDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(SpendingGoalSortOrder.Type);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(SpendingGoalSortOrder.TypeDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 170,
    },
    {
      name: "totalAmountToSpend",
      headerContent: "Spent So Far",
      getBodyContent: (goal) => formatCurrency(goal.totalAmountToSpend),
      alignment: "right",
      minWidth: 150,
    },
    {
      name: "remainingAmountToSpend",
      headerContent: "Remaining To Spend",
      getBodyContent: (goal) => formatCurrency(goal.remainingAmountToSpend),
      alignment: "right",
      minWidth: 180,
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
          Spending Goals
        </Typography>
        <ListFrame<SpendingGoal>
          columns={columns}
          getId={(goal) => goal.id}
          data={data}
          totalCount={totalCount ?? null}
          searchParamName=""
          pageParamName={pageParamName}
          initialEmptyState={{
            title: "No spending goals found",
            description:
              "No spending goals match the current workspace view yet.",
            action: null,
          }}
          filteredEmptyState={{
            title: "No spending goals match this search",
            description:
              "Try a different accounting period or fund filter, or clear the current filters to see all spending goals.",
            action: (
              <Button variant="contained" onClick={clearSearch}>
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
