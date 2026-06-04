"use client";

import { Button, DialogActions, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { Goal } from "@/goals/types";
import Link from "next/link";
import deleteGoal from "@/goals/deleteGoal";

/**
 * Props for the DeleteGoalForm component.
 */
interface DeleteGoalFormProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly goal: Goal;
}

/**
 * Component that displays the form for deleting a goal.
 */
const DeleteGoalForm = function ({
  accountingPeriod,
  goal,
}: DeleteGoalFormProps): JSX.Element {
  const [state, action, pending] = useActionState(deleteGoal, {
    goalId: goal.id,
    redirectUrl: "/",
  });

  return (
    <Stack spacing={2}>
      <Stack spacing={2} sx={{ maxWidth: "500px" }}>
        <Typography>
          Are you sure you want to delete the goal for {goal.fundName} in{" "}
          {accountingPeriod.name}?
        </Typography>
        <DialogActions>
          <Link href="" tabIndex={-1}>
            <Button variant="outlined">Cancel</Button>
          </Link>
          <Button
            variant="contained"
            loading={pending}
            onClick={() => {
              startTransition(() => {
                action();
              });
            }}
          >
            Delete
          </Button>
        </DialogActions>
        <ErrorAlert
          errorMessage={state.errorTitle ?? null}
          unmappedErrors={state.unmappedErrors ?? null}
        />
      </Stack>
    </Stack>
  );
};

export default DeleteGoalForm;
