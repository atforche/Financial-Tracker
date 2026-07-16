"use client";

import {
  type AssignmentGoal,
  AssignmentGoalSort,
  type SpendingGoal,
  SpendingGoalSort,
} from "@/goals/types";
import { Button, IconButton, Stack } from "@mui/material";
import {
  type GoalTrendsView,
  defaultGoalTrendsView,
} from "@/goals/trends/goalTrendsTypes";
import { useRouter, useSearchParams } from "next/navigation";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import FilterListOutlined from "@mui/icons-material/FilterListOutlined";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import createColumnSortProps from "@/framework/listframe/createColumnSortProps";
import { formatCurrency } from "@/framework/currencyHelpers";
import { formatSpendingGoalType } from "@/goals/helpers";
import routes from "@/goals/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Props for the GoalTrendsListFrame component.
 */
interface GoalTrendsListFrameProps {
  readonly view: GoalTrendsView;
  readonly data: AssignmentGoal[] | SpendingGoal[] | null;
  readonly totalCount: number | null;
  readonly isInOnboardingMode: boolean;
}

/**
 * Presents the paged goal table for the Goals trends.
 */
const GoalTrendsListFrame = function ({
  view,
  data,
  totalCount,
  isInOnboardingMode,
}: GoalTrendsListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const router = useRouter();

  const sortParamName = "sort";
  const pageParamName = "page";
  const goalTypeParamName = "goalType";
  const fundNameParamName = "fundName";
  const startAccountingPeriodIdParamName = "startAccountingPeriodId";
  const endAccountingPeriodIdParamName = "endAccountingPeriodId";
  const updateParams = useSearchParamUpdater([pageParamName]);

  const setSort = function (sort: string | null): void {
    updateParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
    });
  };

  const setFundNameFilter = function (fundName: string): void {
    updateParams((params) => {
      params.delete(fundNameParamName);
      params.append(fundNameParamName, fundName);
    });
  };

  const openGoalWorkspace = function (): void {
    router.push(routes.workspace({}));
  };

  const hasActiveFilters =
    searchParams.getAll(goalTypeParamName).length > 0 ||
    searchParams.getAll(fundNameParamName).length > 0 ||
    searchParams.has(startAccountingPeriodIdParamName) ||
    searchParams.has(endAccountingPeriodIdParamName);

  const emptyActionLabel =
    view === "assignment"
      ? "Open assignment workspace"
      : "Open spending workspace";
  const emptyDescription =
    view === "assignment"
      ? "Create or onboard an assignment goal to start tracking funding progress."
      : "Create or onboard a spending goal to start tracking spending progress.";

  if (view === "assignment") {
    const currentSort = tryParseEnum(
      AssignmentGoalSort,
      searchParams.get(sortParamName) ?? "",
    );

    const getSortProps = createColumnSortProps(currentSort, setSort);

    const columns: ColumnDefinition<AssignmentGoal>[] = [
      {
        name: "accountingPeriod",
        headerContent: "Accounting Period",
        getBodyContent: (goal) => goal.accountingPeriod?.name ?? "Ongoing",
        ...getSortProps(
          AssignmentGoalSort.AccountingPeriod,
          AssignmentGoalSort.AccountingPeriodDescending,
        ),
        minWidth: 170,
      },
      {
        name: "fund",
        headerContent: "Fund",
        getBodyContent: (goal) => goal.fund.name,
        ...getSortProps(
          AssignmentGoalSort.Fund,
          AssignmentGoalSort.FundDescending,
        ),
        minWidth: 160,
      },
      {
        name: "totalAmountToAssign",
        headerContent: "Total Amount To Assign",
        getBodyContent: (goal) => formatCurrency(goal.totalAmountToAssign),
        ...getSortProps(
          AssignmentGoalSort.TotalAmountToAssign,
          AssignmentGoalSort.TotalAmountToAssignDescending,
        ),
        alignment: "right",
        minWidth: 140,
      },
      {
        name: "totalAmountAssigned",
        headerContent: "Total Amount Assigned",
        getBodyContent: (goal) => formatCurrency(goal.totalAmountAssigned),
        ...getSortProps(
          AssignmentGoalSort.TotalAmountAssigned,
          AssignmentGoalSort.TotalAmountAssignedDescending,
        ),
        alignment: "right",
        minWidth: 190,
      },
      {
        name: "isMet",
        headerContent: "Is Goal Met?",
        getBodyContent: (goal) => (goal.isGoalMet ? "Yes" : "No"),
        ...getSortProps(
          AssignmentGoalSort.IsMet,
          AssignmentGoalSort.IsMetDescending,
        ),
        alignment: "center",
        minWidth: 120,
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
                setFundNameFilter(goal.fund.name);
              }}
              aria-label={`Filter ${goal.fund.name}`}
            >
              <FilterListOutlined fontSize="small" color="action" />
            </IconButton>
            <IconButton
              size="small"
              color="primary"
              onClick={(event) => {
                event.stopPropagation();
                openGoalWorkspace();
              }}
              aria-label={`Open ${goal.fund.name}`}
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
      <ListFrame<AssignmentGoal>
        title="Assignment Goals"
        columns={columns}
        getId={(goal) => goal.id}
        // eslint-disable-next-line @typescript-eslint/no-unsafe-type-assertion
        data={data as AssignmentGoal[] | null}
        totalCount={totalCount ?? null}
        searchParamName="search"
        pageParamName={pageParamName}
        onRowClick={(goal: AssignmentGoal): void => {
          setFundNameFilter(goal.fund.name);
        }}
        hasActiveFilters={hasActiveFilters}
        initialEmptyState={{
          title: "No assignment goals have been added",
          description: isInOnboardingMode
            ? emptyDescription
            : "Create a new assignment goal to start tracking funding progress.",
          action: (
            <Button
              variant="contained"
              onClick={() => {
                router.push(routes.workspace({}));
              }}
            >
              {emptyActionLabel}
            </Button>
          ),
        }}
        filteredEmptyState={{
          title: "No assignment goals match this trends filter",
          description:
            "Try a different fund name, goal type, or accounting period to widen the trends scope.",
          action: (
            <Button
              variant="contained"
              onClick={() => {
                updateParams((params) => {
                  [...params.keys()].forEach((key) => {
                    params.delete(key);
                  });
                  if (view !== defaultGoalTrendsView) {
                    params.set("view", view);
                  }
                });
              }}
            >
              Reset filters
            </Button>
          ),
        }}
      />
    );
  }

  const currentSort = tryParseEnum(
    SpendingGoalSort,
    searchParams.get(sortParamName) ?? "",
  );

  const getSortProps = createColumnSortProps(currentSort, setSort);

  const columns: ColumnDefinition<SpendingGoal>[] = [
    {
      name: "accountingPeriod",
      headerContent: "Accounting Period",
      getBodyContent: (goal) => goal.accountingPeriod?.name ?? "Ongoing",
      ...getSortProps(
        SpendingGoalSort.AccountingPeriod,
        SpendingGoalSort.AccountingPeriodDescending,
      ),
      minWidth: 170,
    },
    {
      name: "fund",
      headerContent: "Fund",
      getBodyContent: (goal) => goal.fund.name,
      ...getSortProps(SpendingGoalSort.Fund, SpendingGoalSort.FundDescending),
      minWidth: 160,
    },
    {
      name: "goalType",
      headerContent: "Goal Type",
      getBodyContent: (goal) => formatSpendingGoalType(goal.type),
      ...getSortProps(SpendingGoalSort.Type, SpendingGoalSort.TypeDescending),
      minWidth: 160,
    },
    {
      name: "totalAmountToSpend",
      headerContent: "Amount To Spend",
      getBodyContent: (goal) => formatCurrency(goal.totalAmountToSpend),
      ...getSortProps(
        SpendingGoalSort.TotalAmountToSpend,
        SpendingGoalSort.TotalAmountToSpendDescending,
      ),
      alignment: "right",
      minWidth: 160,
    },
    {
      name: "totalAmountSpent",
      headerContent: "Amount Spent",
      getBodyContent: (goal) => formatCurrency(goal.totalAmountSpent),
      ...getSortProps(
        SpendingGoalSort.TotalAmountSpent,
        SpendingGoalSort.TotalAmountSpentDescending,
      ),
      alignment: "right",
      minWidth: 180,
    },
    {
      name: "isMet",
      headerContent: "Is Goal Met?",
      getBodyContent: (goal) => (goal.isGoalMet ? "Yes" : "No"),
      ...getSortProps(SpendingGoalSort.IsMet, SpendingGoalSort.IsMetDescending),
      alignment: "center",
      minWidth: 120,
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
              setFundNameFilter(goal.fund.name);
            }}
            aria-label={`Filter ${goal.fund.name}`}
          >
            <FilterListOutlined fontSize="small" color="action" />
          </IconButton>
          <IconButton
            size="small"
            color="primary"
            onClick={(event) => {
              event.stopPropagation();
              openGoalWorkspace();
            }}
            aria-label={`Open ${goal.fund.name}`}
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
    <ListFrame<SpendingGoal>
      title="Spending Goals"
      columns={columns}
      getId={(goal) => goal.id}
      // eslint-disable-next-line @typescript-eslint/no-unsafe-type-assertion
      data={data as SpendingGoal[] | null}
      totalCount={totalCount ?? null}
      searchParamName="search"
      pageParamName={pageParamName}
      onRowClick={(goal: SpendingGoal): void => {
        setFundNameFilter(goal.fund.name);
      }}
      hasActiveFilters={hasActiveFilters}
      initialEmptyState={{
        title: "No spending goals have been added",
        description: isInOnboardingMode
          ? emptyDescription
          : "Create a new spending goal to start tracking spending progress.",
        action: (
          <Button
            variant="contained"
            onClick={() => {
              router.push(routes.workspace({}));
            }}
          >
            {emptyActionLabel}
          </Button>
        ),
      }}
      filteredEmptyState={{
        title: "No spending goals match this trends filter",
        description:
          "Try a different fund name, goal type, or accounting period to widen the trends scope.",
        action: (
          <Button
            variant="contained"
            onClick={() => {
              updateParams((params) => {
                [...params.keys()].forEach((key) => {
                  params.delete(key);
                });
                if (view !== defaultGoalTrendsView) {
                  params.set("view", view);
                }
              });
            }}
          >
            Reset filters
          </Button>
        ),
      }}
    />
  );
};

export default GoalTrendsListFrame;
