"use client";

import { Button, DialogActions, Stack } from "@mui/material";
import type { Goal, GoalType } from "@/goals/types";
import { type JSX, startTransition, useActionState, useState } from "react";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import GoalTypeEntryField from "@/goals/GoalTypeEntryField";
import Link from "next/link";
import updateGoal from "@/goals/updateGoal";

/**
 * Props for the GoalForm component.
 */
interface UpdateGoalFormProps {
  readonly goal: Goal;
}

/**
 * Component that displays the form for updating a goal.
 */
const UpdateGoalForm = function ({ goal }: UpdateGoalFormProps): JSX.Element {
  const [goalType, setGoalType] = useState<GoalType | null>(goal.goalType);
  const [goalAmount, setGoalAmount] = useState<number | null>(goal.goalAmount);

  const [state, action, pending] = useActionState(updateGoal, {
    goalId: goal.id,
    redirectUrl: "",
  });

  return (
    <Stack spacing={2}>
      <Stack spacing={2} sx={{ maxWidth: "500px" }}>
        <GoalTypeEntryField
          label="Goal Type"
          value={goalType}
          setValue={setGoalType}
          errorMessage={state.goalTypeErrors ?? null}
        />
        <CurrencyEntryField
          label="Goal Amount"
          value={goalAmount}
          setValue={setGoalAmount}
          errorMessage={state.goalAmountErrors ?? null}
        />
        <DialogActions>
          <Link href="" tabIndex={-1}>
            <Button variant="outlined">Cancel</Button>
          </Link>
          <Button
            variant="contained"
            loading={pending}
            disabled={goalType === null || goalAmount === null}
            onClick={() => {
              if (goalType === null || goalAmount === null) {
                return;
              }
              startTransition(() => {
                action({
                  goalType,
                  goalAmount,
                });
              });
            }}
          >
            Update
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

export default UpdateGoalForm;
