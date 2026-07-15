"use client";

import {
  type AssignmentGoal,
  AssignmentGoalSort,
  type SpendingGoal,
  SpendingGoalSort,
  formatSpendingGoalType,
} from "@/goals/types";
import { Button, IconButton, Paper, Stack, Typography } from "@mui/material";
import {
  type GoalTrendsView,
  defaultGoalTrendsView,
} from "@/goals/trends/goalTrendsTypes";
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
 * Props for the GoalTrendsListFrame component.
 */
interface GoalTrendsListFrameProps {
  readonly view: GoalTrendsView;
  readonly data: AssignmentGoal[] | SpendingGoal[] | null;
  readonly totalCount: number | null;
  readonly isInOnboardingMode: boolean;
}

const getResetRoute = function (
  pathname: string,
  view: GoalTrendsView,
): string {
  if (view === defaultGoalTrendsView) {
    return pathname;
  }

  return `${pathname}?view=${view}`;
};

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
  const pathname = usePathname();
  const router = useRouter();

  const sortParamName = "sort";
  const pageParamName = "page";
  const goalTypeParamName = "goalType";
  const fundNameParamName = "fundName";
  const startAccountingPeriodIdParamName = "startAccountingPeriodId";
  const endAccountingPeriodIdParamName = "endAccountingPeriodId";

  const setSort = function (sort: string | null): void {
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

    const columns: ColumnDefinition<AssignmentGoal>[] = [
      {
        name: "accountingPeriod",
        headerContent: "Accounting Period",
        getBodyContent: (goal) => goal.accountingPeriod?.name ?? "Ongoing",
        sortType:
          currentSort === AssignmentGoalSort.AccountingPeriod
            ? ColumnSortType.Ascending
            : currentSort === AssignmentGoalSort.AccountingPeriodDescending
              ? ColumnSortType.Descending
              : null,
        onSort: (sortType): void => {
          if (sortType === ColumnSortType.Ascending) {
            setSort(AssignmentGoalSort.AccountingPeriod);
          } else if (sortType === ColumnSortType.Descending) {
            setSort(AssignmentGoalSort.AccountingPeriodDescending);
          } else {
            setSort(null);
          }
        },
        minWidth: 170,
      },
      {
        name: "fund",
        headerContent: "Fund",
        getBodyContent: (goal) => goal.fund.name,
        sortType:
          currentSort === AssignmentGoalSort.Fund
            ? ColumnSortType.Ascending
            : currentSort === AssignmentGoalSort.FundDescending
              ? ColumnSortType.Descending
              : null,
        onSort: (sortType): void => {
          if (sortType === ColumnSortType.Ascending) {
            setSort(AssignmentGoalSort.Fund);
          } else if (sortType === ColumnSortType.Descending) {
            setSort(AssignmentGoalSort.FundDescending);
          } else {
            setSort(null);
          }
        },
        minWidth: 160,
      },
      {
        name: "totalAmountToAssign",
        headerContent: "Total Amount To Assign",
        getBodyContent: (goal) => formatCurrency(goal.totalAmountToAssign),
        sortType:
          currentSort === AssignmentGoalSort.TotalAmountToAssign
            ? ColumnSortType.Ascending
            : currentSort === AssignmentGoalSort.TotalAmountToAssignDescending
              ? ColumnSortType.Descending
              : null,
        onSort: (sortType): void => {
          if (sortType === ColumnSortType.Ascending) {
            setSort(AssignmentGoalSort.TotalAmountToAssign);
          } else if (sortType === ColumnSortType.Descending) {
            setSort(AssignmentGoalSort.TotalAmountToAssignDescending);
          } else {
            setSort(null);
          }
        },
        alignment: "right",
        minWidth: 140,
      },
      {
        name: "totalAmountAssigned",
        headerContent: "Total Amount Assigned",
        getBodyContent: (goal) => formatCurrency(goal.totalAmountAssigned),
        sortType:
          currentSort === AssignmentGoalSort.TotalAmountAssigned
            ? ColumnSortType.Ascending
            : currentSort === AssignmentGoalSort.TotalAmountAssignedDescending
              ? ColumnSortType.Descending
              : null,
        onSort: (sortType): void => {
          if (sortType === ColumnSortType.Ascending) {
            setSort(AssignmentGoalSort.TotalAmountAssigned);
          } else if (sortType === ColumnSortType.Descending) {
            setSort(AssignmentGoalSort.TotalAmountAssignedDescending);
          } else {
            setSort(null);
          }
        },
        alignment: "right",
        minWidth: 190,
      },
      {
        name: "isMet",
        headerContent: "Is Goal Met?",
        getBodyContent: (goal) => (goal.isGoalMet ? "Yes" : "No"),
        sortType:
          currentSort === AssignmentGoalSort.IsMet
            ? ColumnSortType.Ascending
            : currentSort === AssignmentGoalSort.IsMetDescending
              ? ColumnSortType.Descending
              : null,
        onSort: (sortType): void => {
          if (sortType === ColumnSortType.Ascending) {
            setSort(AssignmentGoalSort.IsMet);
          } else if (sortType === ColumnSortType.Descending) {
            setSort(AssignmentGoalSort.IsMetDescending);
          } else {
            setSort(null);
          }
        },
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
      <Paper
        sx={{
          border: "1px solid",
          borderColor: "divider",
          p: { xs: 2, md: 2.5 },
        }}
      >
        <Stack spacing={2.5}>
          <Typography variant="h5">Assignment Goals</Typography>
          <ListFrame<AssignmentGoal>
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
                    router.replace(getResetRoute(pathname, view));
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
  }

  const currentSort = tryParseEnum(
    SpendingGoalSort,
    searchParams.get(sortParamName) ?? "",
  );

  const columns: ColumnDefinition<SpendingGoal>[] = [
    {
      name: "accountingPeriod",
      headerContent: "Accounting Period",
      getBodyContent: (goal) => goal.accountingPeriod?.name ?? "Ongoing",
      sortType:
        currentSort === SpendingGoalSort.AccountingPeriod
          ? ColumnSortType.Ascending
          : currentSort === SpendingGoalSort.AccountingPeriodDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(SpendingGoalSort.AccountingPeriod);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(SpendingGoalSort.AccountingPeriodDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 170,
    },
    {
      name: "fund",
      headerContent: "Fund",
      getBodyContent: (goal) => goal.fund.name,
      sortType:
        currentSort === SpendingGoalSort.Fund
          ? ColumnSortType.Ascending
          : currentSort === SpendingGoalSort.FundDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(SpendingGoalSort.Fund);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(SpendingGoalSort.FundDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 160,
    },
    {
      name: "goalType",
      headerContent: "Goal Type",
      getBodyContent: (goal) => formatSpendingGoalType(goal.type),
      sortType:
        currentSort === SpendingGoalSort.Type
          ? ColumnSortType.Ascending
          : currentSort === SpendingGoalSort.TypeDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(SpendingGoalSort.Type);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(SpendingGoalSort.TypeDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 160,
    },
    {
      name: "totalAmountToSpend",
      headerContent: "Amount To Spend",
      getBodyContent: (goal) => formatCurrency(goal.totalAmountToSpend),
      sortType:
        currentSort === SpendingGoalSort.TotalAmountToSpend
          ? ColumnSortType.Ascending
          : currentSort === SpendingGoalSort.TotalAmountToSpendDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(SpendingGoalSort.TotalAmountToSpend);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(SpendingGoalSort.TotalAmountToSpendDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 160,
    },
    {
      name: "totalAmountSpent",
      headerContent: "Amount Spent",
      getBodyContent: (goal) => formatCurrency(goal.totalAmountSpent),
      sortType:
        currentSort === SpendingGoalSort.TotalAmountSpent
          ? ColumnSortType.Ascending
          : currentSort === SpendingGoalSort.TotalAmountSpentDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(SpendingGoalSort.TotalAmountSpent);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(SpendingGoalSort.TotalAmountSpentDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 180,
    },
    {
      name: "isMet",
      headerContent: "Is Goal Met?",
      getBodyContent: (goal) => (goal.isGoalMet ? "Yes" : "No"),
      sortType:
        currentSort === SpendingGoalSort.IsMet
          ? ColumnSortType.Ascending
          : currentSort === SpendingGoalSort.IsMetDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(SpendingGoalSort.IsMet);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(SpendingGoalSort.IsMetDescending);
        } else {
          setSort(null);
        }
      },
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
    <Paper
      sx={{
        border: "1px solid",
        borderColor: "divider",
        p: { xs: 2, md: 2.5 },
      }}
    >
      <Stack spacing={2.5}>
        <Typography variant="h5">Spending Goals</Typography>
        <ListFrame<SpendingGoal>
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
                  router.replace(getResetRoute(pathname, view));
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

export default GoalTrendsListFrame;
