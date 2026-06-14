"use client";

import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import { Paper, Stack, Typography } from "@mui/material";
import type { GoalWorkspaceView } from "@/goals/workspace/goalWorkspaceTypes";
import type { JSX } from "react";
import UpdateGoalForm from "@/goals/workspace/UpdateGoalForm";
import { usePathname } from "next/navigation";

/**
 * Displays the available goal actions for the current workspace selection.
 */
interface GoalWorkspaceActionsProps {
  readonly view: GoalWorkspaceView;
  readonly selectedGoal: AssignmentGoal | SpendingGoal | null;
}

/**
 * Renders the update panel for the selected goal.
 */
const GoalWorkspaceActions = function ({
  view,
  selectedGoal,
}: GoalWorkspaceActionsProps): JSX.Element {
  const pathname = usePathname();
  const title =
    view === "assignment" ? "Update Assignment Goal" : "Update Spending Goal";
  const description =
    view === "assignment"
      ? "Select an assignment goal to adjust its behavior and target amount."
      : "Select a spending goal to adjust how spending against the fund is evaluated.";

  return (
    <Paper
      sx={{
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        p: { xs: 2.5, md: 3 },
      }}
    >
      <Stack spacing={3}>
        <Stack spacing={0.5}>
          <Typography variant="h6">{title}</Typography>
          <Typography variant="body2" color="text.secondary">
            {description}
          </Typography>
        </Stack>
        {selectedGoal !== null ? (
          <UpdateGoalForm goal={selectedGoal} redirectUrl={pathname} />
        ) : (
          <Paper
            variant="outlined"
            sx={{ borderRadius: 4, p: { xs: 2.5, md: 3 } }}
          >
            <Stack spacing={1}>
              <Typography variant="subtitle1">No goal selected</Typography>
              <Typography variant="body2" color="text.secondary">
                Pick a goal from the table to edit it here.
              </Typography>
            </Stack>
          </Paper>
        )}
      </Stack>
    </Paper>
  );
};

export default GoalWorkspaceActions;
