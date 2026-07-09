"use client";

import type {
  AssignmentGoal,
  AssignmentGoalType,
  SpendingGoal,
  SpendingGoalType,
  UpdateAssignmentGoalRequest,
  UpdateSpendingGoalRequest,
} from "@/goals/types";
import { Button, Stack } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useRef,
  useState,
} from "react";
import AssignmentGoalSetupSection from "@/funds/workspace/AssignmentGoalSetupSection";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import SpendingGoalSetupSection from "@/funds/workspace/SpendingGoalSetupSection";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import updateAssignmentGoal from "@/goals/workspace/updateAssignmentGoal";
import updateSpendingGoal from "@/goals/workspace/updateSpendingGoal";

/**
 * Props for the UpdateGoalForm component.
 */
interface UpdateGoalFormProps {
  readonly assignmentGoal: AssignmentGoal;
  readonly spendingGoal: SpendingGoal;
  readonly redirectUrl: string;
}

/** Opens a dialog for updating both paired goal configurations together. */
const UpdateGoalForm = function ({
  assignmentGoal,
  spendingGoal,
  redirectUrl,
}: UpdateGoalFormProps): JSX.Element {
  const [open, setOpen] = useState(false);
  const [assignmentGoalType, setAssignmentGoalType] =
    useState<AssignmentGoalType | null>(assignmentGoal.type);
  const [assignmentGoalAmount, setAssignmentGoalAmount] = useState<
    number | null
  >(assignmentGoal.goalAmount);
  const [spendingGoalType, setSpendingGoalType] =
    useState<SpendingGoalType | null>(spendingGoal.type);
  const formRef = useRef<HTMLDivElement | null>(null);
  const [assignmentState, updateAssignment, assignmentPending] = useActionState(
    updateAssignmentGoal,
    {},
  );
  const [spendingState, updateSpending, spendingPending] = useActionState(
    updateSpendingGoal,
    {},
  );

  const reset = function (): void {
    setAssignmentGoalType(assignmentGoal.type);
    setAssignmentGoalAmount(assignmentGoal.goalAmount);
    setSpendingGoalType(spendingGoal.type);
    focusFirstEntryControl(formRef.current);
  };

  useEffect(() => {
    if (assignmentState.success === true && spendingState.success === true) {
      setOpen(false);
      reset();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [assignmentState.success, spendingState.success]);

  const assignmentRequest: UpdateAssignmentGoalRequest | null =
    assignmentGoalType !== null && assignmentGoalAmount !== null
      ? { assignmentGoalType, goalAmount: assignmentGoalAmount }
      : null;
  const spendingRequest: UpdateSpendingGoalRequest | null =
    spendingGoalType !== null ? { spendingGoalType } : null;
  const pending = assignmentPending || spendingPending;

  return (
    <>
      <Button
        variant="contained"
        onClick={() => {
          setOpen(true);
        }}
      >
        Edit
      </Button>
      <Dialog
        open={open}
        fullWidth
        maxWidth="md"
        title="Update Goals"
        {...(pending
          ? {}
          : {
              onClose: (): void => {
                setOpen(false);
                reset();
              },
            })}
        actions={
          <>
            <Button
              disabled={pending}
              onClick={() => {
                setOpen(false);
                reset();
              }}
            >
              Cancel
            </Button>
            <Button variant="outlined" disabled={pending} onClick={reset}>
              Reset
            </Button>
            <Button
              variant="contained"
              loading={pending}
              disabled={assignmentRequest === null || spendingRequest === null}
              onClick={() => {
                if (assignmentRequest === null || spendingRequest === null) {
                  return;
                }
                startTransition(() => {
                  updateAssignment({
                    goal: assignmentGoal,
                    request: assignmentRequest,
                    redirectUrl,
                  });
                  updateSpending({
                    goal: spendingGoal,
                    request: spendingRequest,
                    redirectUrl,
                  });
                });
              }}
            >
              Save
            </Button>
          </>
        }
      >
        <Stack ref={formRef} spacing={3}>
          <AssignmentGoalSetupSection
            value={assignmentGoalType}
            setValue={setAssignmentGoalType}
            amount={assignmentGoalAmount}
            setAmount={setAssignmentGoalAmount}
            typeErrorMessage={assignmentState.typeErrors ?? null}
            amountErrorMessage={assignmentState.goalAmountErrors ?? null}
          />
          <SpendingGoalSetupSection
            value={spendingGoalType}
            setValue={setSpendingGoalType}
            typeErrorMessage={spendingState.typeErrors ?? null}
          />
          <ErrorAlert
            errorMessage={
              assignmentState.errorTitle ?? spendingState.errorTitle ?? null
            }
            unmappedErrors={
              assignmentState.unmappedErrors ??
              spendingState.unmappedErrors ??
              null
            }
          />
        </Stack>
      </Dialog>
    </>
  );
};

export default UpdateGoalForm;
