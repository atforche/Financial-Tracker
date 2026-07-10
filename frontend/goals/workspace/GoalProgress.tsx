import { Box, Stack, Typography } from "@mui/material";
import {
  getGoalProgressBackgroundColor,
  getGoalProgressPercent,
} from "@/goals/workspace/helpers";
import type { CurrentGoalProgress } from "@/goals/types";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";

/**
 * Props for the GoalProgress component.
 */
interface GoalProgressProps {
  readonly label: string;
  readonly progress: CurrentGoalProgress | null;
}

/**
 * Component that displays information about a goal progress.
 */
const GoalProgress = function ({
  label,
  progress,
}: GoalProgressProps): JSX.Element {
  const progressPercent = getGoalProgressPercent(progress);
  return (
    <Stack spacing={0.75}>
      <Stack direction="row" justifyContent="space-between" gap={2}>
        <Typography variant="body2" fontWeight={700}>
          {label}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {progress === null
            ? "No goal"
            : `${formatCurrency(progress.remainingAmount)} remaining`}
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
            width: `${progressPercent}%`,
            height: "100%",
            borderRadius: 999,
            bgcolor: getGoalProgressBackgroundColor(progress, progressPercent),
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

export default GoalProgress;
