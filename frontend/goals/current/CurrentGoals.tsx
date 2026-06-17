import { Box, Stack, Typography } from "@mui/material";
import CurrentGoalsList from "@/goals/current/CurrentGoalsList";
import type { CurrentGoals as CurrentGoalsModel } from "@/goals/types";
import CurrentGoalsSummaryCards from "@/goals/current/CurrentGoalsSummaryCards";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";

const createEmptyCurrent = function (): CurrentGoalsModel {
  return {
    accountingPeriodId: null,
    accountingPeriodName: null,
    summary: {
      totalAmountToAssign: 0,
      totalAmountAssigned: 0,
      percentageOfAssignmentGoalsMet: {
        totalCount: 0,
        metCount: 0,
        percentageMet: 0,
      },
      totalAmountToSpend: 0,
      totalAmountSpent: 0,
      percentageOfSpendingGoalsMet: {
        totalCount: 0,
        metCount: 0,
        percentageMet: 0,
      },
    },
    goals: [],
  };
};

/**
 * Component that displays the current Goals snapshot.
 */
const CurrentGoals = async function (): Promise<JSX.Element> {
  const apiClient = getApiClient();
  const current: CurrentGoalsModel =
    (await apiClient.GET("/goals/current")).data ?? createEmptyCurrent();

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Box
        sx={{
          maxWidth: 1440,
          width: "100%",
          border: "1px solid",
          borderColor: "divider",
          borderRadius: 3,
          px: { xs: 2, md: 3 },
          py: { xs: 2.5, md: 3 },
          background:
            "linear-gradient(135deg, rgba(15,23,42,0.05) 0%, rgba(255,255,255,0.96) 42%, rgba(245,158,11,0.10) 100%)",
        }}
      >
        <Stack spacing={1}>
          <Typography
            variant="overline"
            sx={{
              color: "text.secondary",
              letterSpacing: 1.4,
              fontWeight: 700,
            }}
          >
            Goals
          </Typography>
          <Typography variant="h5">Current Goals</Typography>
          <Typography color="text.secondary">
            {current.accountingPeriodName === null
              ? "No current accounting period is available to show goal progress yet."
              : `Snapshot of goal progress and recent goal activity for ${current.accountingPeriodName}.`}
          </Typography>
        </Stack>
      </Box>
      <CurrentGoalsSummaryCards current={current} />
      <CurrentGoalsList current={current} />
    </Stack>
  );
};

export default CurrentGoals;
