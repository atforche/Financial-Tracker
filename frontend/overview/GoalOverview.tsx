import { Paper, Stack, Typography } from "@mui/material";
import GoalTrendsSummaryCards from "@/goals/trends/GoalTrendsSummaryCards";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";
import { summarizeGoalRange } from "@/goals/trends/goalTrendsSummary";

/**
 * Overview component for goals.
 */
const GoalOverview = async function (): Promise<JSX.Element> {
  const apiClient = getApiClient();
  const { data: accountingPeriods } = await apiClient.GET("/accounting-periods", {
    params: { query: { Sort: "DateDescending", Limit: 1, Offset: 0 } },
  });
  const latestAccountingPeriod = accountingPeriods?.items[0] ?? null;
  if (latestAccountingPeriod === null) {
    return <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}><Stack spacing={2}><Typography variant="caption" color="text.secondary">Current Goals</Typography><Typography variant="h5">Goals</Typography><Typography color="text.secondary">No current accounting period is available to show goal summaries.</Typography></Stack></Paper>;
  }
  const [assignmentResponse, spendingResponse] = await Promise.all([
    apiClient.GET("/goals/assignment", { params: { query: { "Filter.AccountingPeriodIds": [latestAccountingPeriod.id], Limit: 500, Offset: 0 } } }),
    apiClient.GET("/goals/spending", { params: { query: { "Filter.AccountingPeriodIds": [latestAccountingPeriod.id], Limit: 500, Offset: 0 } } }),
  ]);
  if (typeof assignmentResponse.data === "undefined" || typeof spendingResponse.data === "undefined") {
    throw new Error("Failed to load goal overview data");
  }
  const summary = summarizeGoalRange(assignmentResponse.data.items, spendingResponse.data.items);
  return <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}><Stack spacing={2}><Typography variant="h6" color="text.secondary">Current Goals ({latestAccountingPeriod.name})</Typography><Typography variant="h6" color="text.secondary">Assignment</Typography><GoalTrendsSummaryCards trends={summary} view="assignment" /><Typography variant="h6" color="text.secondary">Spending</Typography><GoalTrendsSummaryCards trends={summary} view="spending" /></Stack></Paper>;
};

export default GoalOverview;
