"use client";

import { Box, ButtonBase, Stack, Typography } from "@mui/material";
import type { CurrentGoalProgress, CurrentGoals } from "@/goals/types";
import Frame from "@/framework/view/Frame";
import type { GoalWorkspaceSearchParams } from "@/goals/workspace/GoalWorkspace";
import type { JSX } from "react";
import KeyboardArrowRight from "@mui/icons-material/KeyboardArrowRight";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/goals/routes";
import { useSearchParams } from "next/navigation";

/**
 * Props for the GoalWorkspaceCards component.
 */
interface GoalWorkspaceCardsProps {
  readonly current: CurrentGoals;
}

const getProgressPercent = function (
  progress: CurrentGoalProgress | null,
): number {
  if (progress === null) {
    return 0;
  }
  if (progress.targetAmount === 0) {
    return 100;
  }
  return Math.min((progress.currentAmount / progress.targetAmount) * 100, 100);
};

const GoalProgress = function ({
  label,
  progress,
}: {
  readonly label: string;
  readonly progress: CurrentGoalProgress | null;
}): JSX.Element {
  const remaining =
    progress === null
      ? null
      : Math.max(progress.targetAmount - progress.currentAmount, 0);
  return (
    <Stack spacing={0.75}>
      <Stack direction="row" justifyContent="space-between" gap={2}>
        <Typography variant="body2" fontWeight={700}>
          {label}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {progress === null
            ? "No goal"
            : `${formatCurrency(remaining ?? 0)} remaining`}
        </Typography>
      </Stack>
      <Box
        sx={{
          height: 8,
          overflow: "hidden",
          borderRadius: 999,
          bgcolor: "action.hover",
        }}
      >
        <Box
          sx={{
            width: `${getProgressPercent(progress)}%`,
            height: "100%",
            borderRadius: 999,
            bgcolor:
              progress?.isGoalMet === true ? "success.main" : "primary.main",
            transition: "width 200ms ease",
          }}
        />
      </Box>
      <Typography variant="caption" color="text.secondary">
        {progress === null
          ? "Set up a goal"
          : `${formatCurrency(progress.currentAmount)} of ${formatCurrency(progress.targetAmount)}`}
      </Typography>
    </Stack>
  );
};

/** Displays paired goal progress as navigable workspace cards. */
const GoalWorkspaceCards = function ({
  current,
}: GoalWorkspaceCardsProps): JSX.Element {
  const searchParams = useSearchParams();
  const funds = current.goals;

  if (funds.length === 0) {
    return (
      <Typography color="text.secondary">
        No goals match the selected accounting period and fund filters.
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
        };
        const fundIds = searchParams.getAll("fundIds");
        if (fundIds.length > 0) {
          detailSearchParams.fundIds = fundIds;
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
                goal.assignmentGoal?.isGoalMet === true &&
                goal.spendingGoal?.isGoalMet === true
                  ? "success"
                  : "warning"
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
