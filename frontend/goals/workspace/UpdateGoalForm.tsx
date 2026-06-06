"use client";

import { Button, DialogActions, Stack } from "@mui/material";
import type { Goal, GoalType, UpdateGoalRequest } from "@/goals/types";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useState,
} from "react";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import GoalTypeEntryField from "@/goals/GoalTypeEntryField";
import Link from "next/link";
import updateGoal from "@/goals/workspace/updateGoal";

/**
 * Props for the GoalForm component.
 */
interface UpdateGoalFormProps {
  readonly goal: Goal;
  readonly redirectUrl: string;
}

/**
 * Component that displays the form for updating a goal.
 */
const UpdateGoalForm = function ({
  goal,
  redirectUrl,
}: UpdateGoalFormProps): JSX.Element {
  const [goalType, setGoalType] = useState<GoalType | null>(goal.goalType);
  const [goalAmount, setGoalAmount] = useState<number | null>(goal.goalAmount);

  const [state, action, pending] = useActionState(updateGoal, {});

  const reset = function (): void {
    setGoalType(goal.goalType);
    setGoalAmount(goal.goalAmount);
  };

  useEffect(() => {
    if (state.success === true) {
      reset();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [state]);

  let request: UpdateGoalRequest | null = null;
  if (goalType !== null && goalAmount !== null) {
    request = {
      goalType,
      goalAmount,
    };
  }

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
            <Button variant="outlined" onClick={reset}>
              Cancel
            </Button>
          </Link>
          <Button
            variant="contained"
            loading={pending}
            disabled={request === null}
            onClick={() => {
              if (request === null) {
                return;
              }
              startTransition(() => {
                action({
                  goalId: goal.id,
                  request,
                  redirectUrl,
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
