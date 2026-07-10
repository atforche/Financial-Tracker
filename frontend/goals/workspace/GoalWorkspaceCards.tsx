"use client";

import { Box, ButtonBase, Stack, Typography } from "@mui/material";
import type { CurrentGoals } from "@/goals/types";
import Frame from "@/framework/view/Frame";
import GoalProgress from "@/goals/workspace/GoalProgress";
import type { GoalWorkspaceSearchParams } from "@/goals/workspace/GoalWorkspace";
import type { JSX } from "react";
import KeyboardArrowRight from "@mui/icons-material/KeyboardArrowRight";
import routes from "@/goals/routes";
import { useSearchParams } from "next/navigation";

/**
 * Props for the GoalWorkspaceCards component.
 */
interface GoalWorkspaceCardsProps {
  readonly current: CurrentGoals;
}

/**
 * Displays paired goal progress as navigable workspace cards.
 */
const GoalWorkspaceCards = function ({
  current,
}: GoalWorkspaceCardsProps): JSX.Element {
  const searchParams = useSearchParams();
  const search = (searchParams.get("search") ?? "").trim().toLowerCase();
  const funds = current.goals.filter((goal) =>
    goal.fundName.toLowerCase().includes(search),
  );

  if (funds.length === 0) {
    return (
      <Typography color="text.secondary">
        No goals match the selected accounting period and search filters.
      </Typography>
    );
  }

  return (
    <Box
      sx={{
        display: "grid",
        gap: 2,
        justifyContent: "start",
        justifyItems: "stretch",
        alignItems: "start",
        gridTemplateColumns: {
          xs: "minmax(0, 1fr)",
          sm: "repeat(auto-fit, minmax(340px, max-content))",
        },
      }}
    >
      {funds.map((goal) => {
        const detailSearchParams: GoalWorkspaceSearchParams = {
          ...(current.accountingPeriodId === null
            ? {}
            : { accountingPeriodId: current.accountingPeriodId }),
          ...(search === "" ? {} : { search }),
        };
        const fundIds = searchParams.getAll("fundIds");
        if (fundIds.length > 0) {
          detailSearchParams.fundIds = fundIds;
        }

        let goalsMet = 0;
        if (goal.assignmentGoal?.isGoalMet === true) {
          goalsMet += 1;
        }
        if (goal.spendingGoal?.isGoalMet === true) {
          goalsMet += 1;
        }
        return (
          <ButtonBase
            key={goal.fundId}
            href={routes.workspaceDetail(goal.fundId, detailSearchParams)}
            sx={{
              display: "flex",
              width: "100%",
              minWidth: 0,
              borderRadius: 5,
              textAlign: "left",
              "& .MuiPaper-root": { width: "100%" },
            }}
          >
            <Frame
              title={goal.fundName}
              color={
                goalsMet === 2
                  ? "success"
                  : goalsMet === 1
                    ? "warning"
                    : "error"
              }
              headerContent={
                <KeyboardArrowRight
                  sx={{ color: "text.secondary", fontSize: 22 }}
                />
              }
            >
              <Stack spacing={2.25}>
                <Typography variant="body2" color="text.secondary">
                  {current.accountingPeriodName ?? "No accounting period"}
                </Typography>
                <GoalProgress
                  label="Remaining to assign"
                  progress={goal.assignmentGoal}
                />
                <GoalProgress
                  label="Remaining to spend"
                  progress={goal.spendingGoal}
                />
              </Stack>
            </Frame>
          </ButtonBase>
        );
      })}
    </Box>
  );
};

export default GoalWorkspaceCards;
