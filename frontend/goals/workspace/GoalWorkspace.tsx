import { Box, Stack } from "@mui/material";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import type { GoalSortOrder } from "@/goals/types";
import GoalWorkspaceActions from "@/goals/workspace/GoalWorkspaceActions";
import GoalWorkspaceFilter from "@/goals/workspace/GoalWorkspaceFilter";
import GoalWorkspaceListFrame from "@/goals/workspace/GoalWorkspaceListFrame";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";
import { redirect } from "next/navigation";
import routes from "@/goals/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";

type GoalWorkspaceAction = "create" | "update" | "delete";

/**
 * Search parameters supported by the Goals workspace.
 */
interface GoalWorkspaceSearchParams {
  accountingPeriodIds?: string | string[];
  fundIds?: string | string[];
  sort?: GoalSortOrder;
  page?: number | string | null;
  selectedGoalId?: string;
  action?: GoalWorkspaceAction;
}

/**
 * Props for the GoalWorkspace component.
 */
interface GoalWorkspaceProps {
  readonly searchParams: Promise<GoalWorkspaceSearchParams>;
}

/**
 * Displays the goal workspace with list-backed inline actions.
 */
const GoalWorkspace = async function ({
  searchParams,
}: GoalWorkspaceProps): Promise<JSX.Element> {
  const { accountingPeriodIds, fundIds, sort, page, selectedGoalId, action } =
    await searchParams;
  const apiClient = getApiClient();
  const currentPage = normalizePageValue(page);

  const accountingPeriodsPromise = apiClient.GET("/accounting-periods", {
    params: {
      query: {
        Limit: 500,
      },
    },
  });
  const fundsPromise = apiClient.GET("/funds");
  const goalsPromise = apiClient.GET("/goals/many", {
    params: {
      query: {
        ...(Array.isArray(accountingPeriodIds)
          ? { AccountingPeriodIds: accountingPeriodIds }
          : typeof accountingPeriodIds !== "undefined"
            ? { AccountingPeriodIds: [accountingPeriodIds] }
            : {}),
        ...(Array.isArray(fundIds)
          ? { FundIds: fundIds }
          : typeof fundIds !== "undefined"
            ? { FundIds: [fundIds] }
            : {}),
        Sort: sort ?? null,
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
    throw new Error("Failed to fetch goals");
  }

  const selectedGoal =
    goals.items.find((goal) => goal.id === selectedGoalId) ?? null;

  if (typeof selectedGoalId === "string" && selectedGoal === null) {
    redirect(
      routes.workspace({
        accountingPeriodIds: Array.isArray(accountingPeriodIds)
          ? accountingPeriodIds
          : typeof accountingPeriodIds !== "undefined"
            ? [accountingPeriodIds]
            : [],
        fundIds: Array.isArray(fundIds)
          ? fundIds
          : typeof fundIds !== "undefined"
            ? [fundIds]
            : [],
        ...(typeof sort !== "undefined" ? { sort } : {}),
        ...(typeof page !== "undefined" ? { page: currentPage } : {}),
        selectedGoalId: "",
        ...(typeof action !== "undefined" ? { action } : {}),
      }),
    );
  }

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Stack spacing={3} sx={{ maxWidth: 1440, width: "100%" }}>
        <GoalWorkspaceFilter
          accountingPeriods={accountingPeriods.items}
          funds={funds.items}
        />
      </Stack>
      <Box
        sx={{
          display: "grid",
          gap: 3,
          gridTemplateColumns:
            "repeat(auto-fit, minmax(min(100%, 600px), 1fr))",
        }}
      >
        <GoalWorkspaceListFrame
          data={goals.items}
          totalCount={goals.totalCount}
          selectedGoalId={selectedGoal?.id ?? null}
        />
        <GoalWorkspaceActions
          accountingPeriods={accountingPeriods.items}
          funds={funds.items}
          selectedGoal={selectedGoal}
          requestedAction={action ?? null}
        />
      </Box>
    </Stack>
  );
};

export type { GoalWorkspaceAction, GoalWorkspaceSearchParams };
export default GoalWorkspace;
