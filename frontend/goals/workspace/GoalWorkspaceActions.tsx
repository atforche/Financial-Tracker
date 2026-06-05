"use client";

import { Paper, Stack, ToggleButton, ToggleButtonGroup } from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateGoalForm from "@/goals/workspace/CreateGoalForm";
import DeleteGoalForm from "@/goals/workspace/DeleteGoalForm";
import type { Fund } from "@/funds/types";
import type { Goal } from "@/goals/types";
import type { GoalWorkspaceAction } from "@/goals/workspace/GoalWorkspace";
import type { JSX } from "react";
import UpdateGoalForm from "@/goals/workspace/UpdateGoalForm";

/**
 * Props for the GoalWorkspaceActions component.
 */
interface GoalWorkspaceActionsProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly funds: Fund[];
  readonly selectedGoal: Goal | null;
  readonly requestedAction: GoalWorkspaceAction | null;
}

/**
 * Gets the available actions for the provided goal.
 */
const getAvailableActions = function (
  selectedGoal: Goal | null,
): readonly GoalWorkspaceAction[] {
  if (selectedGoal === null) {
    return ["create"];
  }
  return ["update", "delete"];
};

/**
 * Displays the available goal actions for the current workspace selection.
 */
const GoalWorkspaceActions = function ({
  accountingPeriods,
  funds,
  selectedGoal,
  requestedAction,
}: GoalWorkspaceActionsProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const allActions: readonly GoalWorkspaceAction[] = [
    "create",
    "update",
    "delete",
  ];
  const availableActions: readonly GoalWorkspaceAction[] =
    getAvailableActions(selectedGoal);

  const activeAction =
    requestedAction !== null && availableActions.includes(requestedAction)
      ? requestedAction
      : availableActions[0];

  const setAction = function (action: GoalWorkspaceAction | null): void {
    const params = new URLSearchParams(searchParams.toString());
    if (action === null) {
      params.delete("action");
    } else {
      params.set("action", action);
    }
    router.replace(`${pathname}?${params.toString()}`);
  };

  const selectedGoalAccountingPeriod = selectedGoal
    ? (accountingPeriods.find(
        (period) => period.id === selectedGoal.accountingPeriodId,
      ) ?? null)
    : null;

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
        <ToggleButtonGroup
          value={activeAction}
          exclusive
          onChange={(_, nextValue: GoalWorkspaceAction | null) => {
            setAction(nextValue);
          }}
          sx={{ flexWrap: "wrap" }}
        >
          {allActions.map((action) => (
            <ToggleButton
              key={action}
              value={action}
              disabled={!availableActions.includes(action)}
            >
              {action === "create"
                ? "Create"
                : action === "update"
                  ? "Update"
                  : "Delete"}
            </ToggleButton>
          ))}
        </ToggleButtonGroup>
        {activeAction === "create" ? (
          <CreateGoalForm
            accountingPeriods={accountingPeriods}
            funds={funds}
            redirectUrl={pathname}
          />
        ) : null}
        {activeAction === "update" && selectedGoal !== null ? (
          <UpdateGoalForm goal={selectedGoal} redirectUrl={pathname} />
        ) : null}
        {activeAction === "delete" &&
        selectedGoal !== null &&
        selectedGoalAccountingPeriod !== null ? (
          <DeleteGoalForm
            accountingPeriod={selectedGoalAccountingPeriod}
            goal={selectedGoal}
            redirectUrl={pathname}
          />
        ) : null}
      </Stack>
    </Paper>
  );
};

export default GoalWorkspaceActions;
