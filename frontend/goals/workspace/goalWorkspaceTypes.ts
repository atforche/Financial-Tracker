type GoalWorkspaceView = "assignment" | "spending";

const defaultGoalWorkspaceView: GoalWorkspaceView = "assignment";

/**
 * Determines whether the provided URL value is a supported goal workspace view.
 */
const isGoalWorkspaceView = function (
  value: string | null | undefined,
): value is GoalWorkspaceView {
  return value === "assignment" || value === "spending";
};

export type { GoalWorkspaceView };
export { defaultGoalWorkspaceView, isGoalWorkspaceView };
