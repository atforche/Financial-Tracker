"use client";

import { Button, IconButton, Paper, Stack, Typography } from "@mui/material";
import {
  type GoalDashboardGoal,
  GoalSortOrder,
  formatGoalType,
} from "@/goals/types";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import FilterListOutlined from "@mui/icons-material/FilterListOutlined";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/goals/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";

/**
 * Props for the GoalDashboardListFrame component.
 */
interface GoalDashboardListFrameProps {
  readonly data: GoalDashboardGoal[] | null;
  readonly totalCount: number | null;
  readonly isInOnboardingMode: boolean;
}

/**
 * Presents the paged goal table for the Goals dashboard.
 */
const GoalDashboardListFrame = function ({
  data,
  totalCount,
  isInOnboardingMode,
}: GoalDashboardListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const sortParamName = "sort";
  const pageParamName = "page";
  const goalTypeParamName = "goalType";
  const fundNameParamName = "fundName";
  const startAccountingPeriodIdParamName = "startAccountingPeriodId";
  const endAccountingPeriodIdParamName = "endAccountingPeriodId";

  const setSort = function (sort: GoalSortOrder | null): void {
    const params = new URLSearchParams(searchParams.toString());
    if (sort === null) {
      params.delete(sortParamName);
    } else {
      params.set(sortParamName, sort);
    }
    params.delete(pageParamName);
    router.replace(`${pathname}?${params.toString()}`, { scroll: false });
  };

  const setFundNameFilter = function (fundName: string): void {
    const params = new URLSearchParams(searchParams.toString());
    params.delete(fundNameParamName);
    params.append(fundNameParamName, fundName);
    params.delete(pageParamName);
    router.replace(`${pathname}?${params.toString()}`, { scroll: false });
  };

  const openGoalWorkspace = function (goal: GoalDashboardGoal): void {
    router.push(routes.workspace({ selectedGoalId: goal.id }));
  };

  const currentSort = tryParseEnum(
    GoalSortOrder,
    searchParams.get(sortParamName) ?? "",
  );
  const hasActiveFilters =
    searchParams.getAll(goalTypeParamName).length > 0 ||
    searchParams.getAll(fundNameParamName).length > 0 ||
    searchParams.has(startAccountingPeriodIdParamName) ||
    searchParams.has(endAccountingPeriodIdParamName);

  const columns: ColumnDefinition<GoalDashboardGoal>[] = [
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
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(GoalSortOrder.AccountingPeriod);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(GoalSortOrder.AccountingPeriodDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 170,
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
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(GoalSortOrder.Fund);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(GoalSortOrder.FundDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 160,
    },
    {
      name: "goalType",
      headerContent: "Goal Type",
      getBodyContent: (goal) => formatGoalType(goal.goalType),
      minWidth: 130,
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
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(GoalSortOrder.GoalAmount);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(GoalSortOrder.GoalAmountDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 140,
    },
    {
      name: "remainingAmountToAssign",
      headerContent: "Remaining To Assign",
      getBodyContent: (goal) => formatCurrency(goal.remainingAmountToAssign),
      alignment: "right",
      minWidth: 190,
    },
    {
      name: "remainingAmountToSpend",
      headerContent: "Remaining To Spend",
      getBodyContent: (goal) => formatCurrency(goal.remainingAmountToSpend),
      alignment: "right",
      minWidth: 180,
    },
    {
      name: "actions",
      headerContent: "",
      getBodyContent: (goal) => (
        <Stack direction="row" spacing={0.5} justifyContent="flex-end">
          <IconButton
            size="small"
            color="inherit"
            onClick={(event) => {
              event.stopPropagation();
              setFundNameFilter(goal.fundName);
            }}
            aria-label={`Filter ${goal.fundName}`}
          >
            <FilterListOutlined fontSize="small" color="action" />
          </IconButton>
          <IconButton
            size="small"
            color="primary"
            onClick={(event) => {
              event.stopPropagation();
              openGoalWorkspace(goal);
            }}
            aria-label={`Open ${goal.fundName}`}
          >
            <ArrowForwardOutlined fontSize="small" color="action" />
          </IconButton>
        </Stack>
      ),
      alignment: "right",
      minWidth: 84,
      maxWidth: 84,
    },
  ];

  return (
    <Paper
      sx={{
        border: "1px solid",
        borderColor: "divider",
        p: { xs: 2, md: 2.5 },
      }}
    >
      <Stack spacing={2.5}>
        <Typography variant="h5">Goals</Typography>
        <ListFrame<GoalDashboardGoal>
          columns={columns}
          getId={(goal) => goal.id}
          data={data ?? null}
          totalCount={totalCount ?? null}
          searchParamName="search"
          pageParamName={pageParamName}
          onRowClick={(goal: GoalDashboardGoal): void => {
            setFundNameFilter(goal.fundName);
          }}
          hasActiveFilters={hasActiveFilters}
          initialEmptyState={{
            title: "No goals have been added",
            description: isInOnboardingMode
              ? "Create or onboard a goal to start tracking funding progress."
              : "Create a new goal to start tracking funding progress.",
            action: (
              <Button
                variant="contained"
                onClick={() => {
                  router.push(routes.workspace({ action: "create" }));
                }}
              >
                {isInOnboardingMode ? "Onboard goal" : "Create goal"}
              </Button>
            ),
          }}
          filteredEmptyState={{
            title: "No goals match this dashboard filter",
            description:
              "Try a different fund name, goal type, or accounting period to widen the dashboard scope.",
            action: (
              <Button
                variant="contained"
                onClick={() => {
                  router.replace(pathname);
                }}
              >
                Reset filters
              </Button>
            ),
          }}
        />
      </Stack>
    </Paper>
  );
};

export default GoalDashboardListFrame;
