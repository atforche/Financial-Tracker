import {
  type AssignmentGoal,
  AssignmentGoalSortOrder,
  type SpendingGoal,
  SpendingGoalSortOrder,
} from "@/goals/types";
import { Box, Stack } from "@mui/material";
import {
  type GoalWorkspaceView,
  defaultGoalWorkspaceView,
  isGoalWorkspaceView,
} from "@/goals/workspace/goalWorkspaceTypes";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Fund } from "@/funds/types";
import GoalWorkspaceActions from "@/goals/workspace/GoalWorkspaceActions";
import GoalWorkspaceFilter from "@/goals/workspace/GoalWorkspaceFilter";
import GoalWorkspaceListFrame from "@/goals/workspace/GoalWorkspaceListFrame";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";
import { redirect } from "next/navigation";
import routes from "@/goals/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";
import tryParseEnum from "@/framework/data/tryParseEnum";

/**
 * Search parameters supported by the Goals workspace.
 */
interface GoalWorkspaceSearchParams {
  accountingPeriodIds?: string | string[];
  fundIds?: string | string[];
  sort?: string;
  page?: number | string | null;
  selectedGoalId?: string;
  view?: GoalWorkspaceView;
}

/**
 * Props for the GoalWorkspace component.
 */
interface GoalWorkspaceProps {
  readonly searchParams: Promise<GoalWorkspaceSearchParams>;
}

const toRepeatedSearchParam = function (
  value: string | string[] | undefined,
): string[] {
  if (Array.isArray(value)) {
    return value;
  }

  return typeof value === "string" ? [value] : [];
};

const workspaceStyles = {
  container: { width: "100%" },
  content: { maxWidth: 1440, width: "100%" },
  grid: {
    display: "grid",
    gap: 3,
    gridTemplateColumns: "repeat(auto-fit, minmax(min(100%, 600px), 1fr))",
  },
} as const;

const renderAssignmentWorkspace = function ({
  accountingPeriods,
  funds,
  goals,
  selectedGoal,
}: {
  readonly accountingPeriods: {
    items: AccountingPeriod[];
  };
  readonly funds: { items: Fund[] };
  readonly goals: { items: AssignmentGoal[]; totalCount: number };
  readonly selectedGoal: AssignmentGoal | null;
}): JSX.Element {
  return (
    <Stack spacing={3} sx={workspaceStyles.container}>
      <Stack spacing={3} sx={workspaceStyles.content}>
        <GoalWorkspaceFilter
          accountingPeriods={accountingPeriods.items}
          funds={funds.items}
          view="assignment"
        />
      </Stack>
      <Box sx={workspaceStyles.grid}>
        <GoalWorkspaceListFrame
          view="assignment"
          data={goals.items}
          totalCount={goals.totalCount}
          selectedGoalId={selectedGoal?.id ?? null}
        />
        <GoalWorkspaceActions view="assignment" selectedGoal={selectedGoal} />
      </Box>
    </Stack>
  );
};

const renderSpendingWorkspace = function ({
  accountingPeriods,
  funds,
  goals,
  selectedGoal,
}: {
  readonly accountingPeriods: {
    items: AccountingPeriod[];
  };
  readonly funds: { items: Fund[] };
  readonly goals: { items: SpendingGoal[]; totalCount: number };
  readonly selectedGoal: SpendingGoal | null;
}): JSX.Element {
  return (
    <Stack spacing={3} sx={workspaceStyles.container}>
      <Stack spacing={3} sx={workspaceStyles.content}>
        <GoalWorkspaceFilter
          accountingPeriods={accountingPeriods.items}
          funds={funds.items}
          view="spending"
        />
      </Stack>
      <Box sx={workspaceStyles.grid}>
        <GoalWorkspaceListFrame
          view="spending"
          data={goals.items}
          totalCount={goals.totalCount}
          selectedGoalId={selectedGoal?.id ?? null}
        />
        <GoalWorkspaceActions view="spending" selectedGoal={selectedGoal} />
      </Box>
    </Stack>
  );
};

/**
 * Displays the goal workspace with list-backed inline actions.
 */
const GoalWorkspace = async function ({
  searchParams,
}: GoalWorkspaceProps): Promise<JSX.Element> {
  const { accountingPeriodIds, fundIds, sort, page, selectedGoalId, view } =
    await searchParams;
  const apiClient = getApiClient();
  const currentPage = normalizePageValue(page);
  const currentView = isGoalWorkspaceView(view)
    ? view
    : defaultGoalWorkspaceView;
  const normalizedAccountingPeriodIds =
    toRepeatedSearchParam(accountingPeriodIds);
  const normalizedFundIds = toRepeatedSearchParam(fundIds);

  const accountingPeriodsPromise = apiClient.GET("/accounting-periods", {
    params: {
      query: {
        Limit: 500,
      },
    },
  });
  const fundsPromise = apiClient.GET("/funds");

  if (currentView === "assignment") {
    const currentSort =
      typeof sort === "string"
        ? tryParseEnum(AssignmentGoalSortOrder, sort)
        : null;
    const goalsPromise = apiClient.GET("/goals/assignment/many", {
      params: {
        query: {
          ...(normalizedAccountingPeriodIds.length > 0
            ? { AccountingPeriodIds: normalizedAccountingPeriodIds }
            : {}),
          ...(normalizedFundIds.length > 0
            ? { FundIds: normalizedFundIds }
            : {}),
          ...(currentSort !== null ? { Sort: currentSort } : {}),
          Limit: rowsPerPage,
          Offset: getPageOffset(currentPage),
        },
      },
    });

    const [{ data: accountingPeriods }, { data: funds }, { data: goals }] =
      await Promise.all([accountingPeriodsPromise, fundsPromise, goalsPromise]);

    if (typeof accountingPeriods === "undefined") {
      throw new Error("Failed to fetch accounting periods");
    }
    if (typeof funds === "undefined") {
      throw new Error("Failed to fetch funds");
    }
    if (typeof goals === "undefined") {
      throw new Error("Failed to fetch assignment goals");
    }

    const selectedGoal =
      goals.items.find((goal) => goal.id === selectedGoalId) ?? null;

    if (typeof selectedGoalId === "string" && selectedGoal === null) {
      redirect(
        routes.workspace({
          ...(normalizedAccountingPeriodIds.length > 0
            ? { accountingPeriodIds: normalizedAccountingPeriodIds }
            : {}),
          ...(normalizedFundIds.length > 0
            ? { fundIds: normalizedFundIds }
            : {}),
          ...(typeof sort === "string" ? { sort } : {}),
          ...(typeof page !== "undefined" ? { page: currentPage } : {}),
          view: currentView,
        }),
      );
    }

    return renderAssignmentWorkspace({
      accountingPeriods,
      funds,
      goals,
      selectedGoal,
    });
  }

  const currentSort =
    typeof sort === "string" ? tryParseEnum(SpendingGoalSortOrder, sort) : null;
  const goalsPromise = apiClient.GET("/goals/spending/many", {
    params: {
      query: {
        ...(normalizedAccountingPeriodIds.length > 0
          ? { AccountingPeriodIds: normalizedAccountingPeriodIds }
          : {}),
        ...(normalizedFundIds.length > 0 ? { FundIds: normalizedFundIds } : {}),
        ...(currentSort !== null ? { Sort: currentSort } : {}),
        Limit: rowsPerPage,
        Offset: getPageOffset(currentPage),
      },
    },
  });

  const [{ data: accountingPeriods }, { data: funds }, { data: goals }] =
    await Promise.all([accountingPeriodsPromise, fundsPromise, goalsPromise]);

  if (typeof accountingPeriods === "undefined") {
    throw new Error("Failed to fetch accounting periods");
  }
  if (typeof funds === "undefined") {
    throw new Error("Failed to fetch funds");
  }
  if (typeof goals === "undefined") {
    throw new Error("Failed to fetch spending goals");
  }

  const selectedGoal =
    goals.items.find((goal) => goal.id === selectedGoalId) ?? null;

  if (typeof selectedGoalId === "string" && selectedGoal === null) {
    redirect(
      routes.workspace({
        ...(normalizedAccountingPeriodIds.length > 0
          ? { accountingPeriodIds: normalizedAccountingPeriodIds }
          : {}),
        ...(normalizedFundIds.length > 0 ? { fundIds: normalizedFundIds } : {}),
        ...(typeof sort === "string" ? { sort } : {}),
        ...(typeof page !== "undefined" ? { page: currentPage } : {}),
        view: currentView,
      }),
    );
  }

  return renderSpendingWorkspace({
    accountingPeriods,
    funds,
    goals,
    selectedGoal,
  });
};

export type { GoalWorkspaceSearchParams };
export default GoalWorkspace;
