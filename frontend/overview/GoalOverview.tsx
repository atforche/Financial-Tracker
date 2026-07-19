import { Paper, Stack, Typography } from "@mui/material";
import { AccountingPeriodSortModel } from "@/framework/data/api";
import GoalTrendsSummaryCards from "@/goals/trends/GoalTrendsSummaryCards";
import type { JSX } from "react";
import createApiClient from "@/framework/data/createApiClient";
import { summarizeGoalRange } from "@/goals/trends/goalTrendsSummary";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Overview component for goals.
 */
const GoalOverview = async function (): Promise<JSX.Element> {
  const apiClient = createApiClient();
  const accountingPeriodsResponse = await apiClient.GET("/accounting-periods", {
    params: {
      query: {
        Sort: AccountingPeriodSortModel.DateDescending,
        Limit: 1,
        Offset: 0,
      },
    },
  });
  const accountingPeriods = unwrapApiResponse(
    accountingPeriodsResponse,
    "Failed to load accounting periods",
  );
  const latestAccountingPeriod = accountingPeriods.items[0] ?? null;
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
  const [assignmentResponse, spendingResponse] = await Promise.all([
    apiClient.GET("/goals/assignment", {
      params: {
        query: {
          "Filter.AccountingPeriodIds": [latestAccountingPeriod.id],
          Limit: 500,
          Offset: 0,
        },
      },
    }),
    apiClient.GET("/goals/spending", {
      params: {
        query: {
          "Filter.AccountingPeriodIds": [latestAccountingPeriod.id],
          Limit: 500,
          Offset: 0,
        },
      },
    }),
  ]);
  const assignmentGoals = unwrapApiResponse(
    assignmentResponse,
    "Failed to load assignment goals",
  );
  const spendingGoals = unwrapApiResponse(
    spendingResponse,
    "Failed to load spending goals",
  );
  const summary = summarizeGoalRange(
    assignmentGoals.items,
    spendingGoals.items,
  );
  return (
    <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
      <Stack spacing={2}>
        <Typography variant="h6" color="text.secondary">
          Current Goals ({latestAccountingPeriod.name})
        </Typography>
        <Typography variant="h6" color="text.secondary">
          Assignment
        </Typography>
        <GoalTrendsSummaryCards trends={summary} view="assignment" />
        <Typography variant="h6" color="text.secondary">
          Spending
        </Typography>
        <GoalTrendsSummaryCards trends={summary} view="spending" />
      </Stack>
    </Paper>
  );
};

export default GoalOverview;
