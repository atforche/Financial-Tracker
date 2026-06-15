import { Paper, Stack, Typography } from "@mui/material";
import { AccountingPeriodSortOrder } from "@/accounting-periods/types";
import GoalDashboardSummaryCards from "@/goals/dashboard/GoalDashboardSummaryCards";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Overview component for goals.
 */
const GoalOverview = async function (): Promise<JSX.Element> {
  const apiClient = getApiClient();
  const accountingPeriodsResponse = await apiClient.GET("/accounting-periods", {
    params: {
      query: {
        Search: "",
        Sort: AccountingPeriodSortOrder.DateDescending,
        Limit: 1,
        Offset: 0,
      },
    },
  });

  const latestAccountingPeriod =
    accountingPeriodsResponse.data?.items[0] ?? null;

  if (latestAccountingPeriod === null) {
    return (
      <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
        <Stack spacing={2}>
          <Typography variant="caption" color="text.secondary">
            Current Goals
          </Typography>
          <Typography variant="h5">Goals</Typography>
          <Typography color="text.secondary">
            No current accounting period is available to show goal summaries.
          </Typography>
        </Stack>
      </Paper>
    );
  }

  const { data: dashboard } = await apiClient.GET("/goals/dashboard", {
    params: {
      query: {
        AssignmentGoalLimit: 10,
        AssignmentBalanceEventLimit: 10,
        SpendingGoalLimit: 0,
        SpendingBalanceEventLimit: 0,
        StartAccountingPeriodId: latestAccountingPeriod.id,
        EndAccountingPeriodId: latestAccountingPeriod.id,
      },
    },
  });

  if (typeof dashboard === "undefined") {
    throw new Error("Failed to load goal overview data");
  }

  return (
    <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
      <Stack spacing={2}>
        <Typography variant="h6" color="text.secondary">
          Current Goals ({latestAccountingPeriod.name})
        </Typography>
        <Typography variant="h6" color="text.secondary">
          Assignment
        </Typography>
        <GoalDashboardSummaryCards dashboard={dashboard} view="assignment" />
        <Typography variant="h6" color="text.secondary">
          Spending
        </Typography>
        <GoalDashboardSummaryCards dashboard={dashboard} view="spending" />
      </Stack>
    </Paper>
  );
};

export default GoalOverview;
