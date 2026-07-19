import { Stack, Typography } from "@mui/material";
import type { AccountingPeriod } from "@/accounting-periods/types";
import ContentSurface from "@/framework/view/ContentSurface";
import GoalTrendsSummaryCards from "@/goals/trends/GoalTrendsSummaryCards";
import type { JSX } from "react";
import createApiClient from "@/framework/data/createApiClient";
import loadAllPages from "@/framework/data/loadAllPages";
import { summarizeGoalRange } from "@/goals/trends/goalTrendsSummary";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Props for the GoalOverview component.
 */
interface GoalOverviewProps {
  readonly latestAccountingPeriod: AccountingPeriod | null;
}

/**
 * Overview component for goals.
 */
const GoalOverview = async function ({
  latestAccountingPeriod,
}: GoalOverviewProps): Promise<JSX.Element> {
  if (latestAccountingPeriod === null) {
    return (
      <ContentSurface>
        <Stack spacing={2}>
          <Typography variant="caption" color="text.secondary">
            Current Goals
          </Typography>
          <Typography variant="h5">Goals</Typography>
          <Typography color="text.secondary">
            No current accounting period is available to show goal summaries.
          </Typography>
        </Stack>
      </ContentSurface>
    );
  }
  const apiClient = createApiClient();
  const [assignmentGoals, spendingGoals] = await Promise.all([
    loadAllPages(async (limit, offset) =>
      unwrapApiResponse(
        await apiClient.GET("/goals/assignment", {
          params: {
            query: {
              "Filter.AccountingPeriodIds": [latestAccountingPeriod.id],
              Limit: limit,
              Offset: offset,
            },
          },
        }),
        "Failed to load assignment goals",
      ),
    ),
    loadAllPages(async (limit, offset) =>
      unwrapApiResponse(
        await apiClient.GET("/goals/spending", {
          params: {
            query: {
              "Filter.AccountingPeriodIds": [latestAccountingPeriod.id],
              Limit: limit,
              Offset: offset,
            },
          },
        }),
        "Failed to load spending goals",
      ),
    ),
  ]);
  const summary = summarizeGoalRange(assignmentGoals, spendingGoals);
  return (
    <ContentSurface>
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
    </ContentSurface>
  );
};

export default GoalOverview;
