"use client";

import {
  type AssignmentGoal,
  type AssignmentGoalType,
  type SpendingGoal,
  type SpendingGoalType,
  type UpdateAssignmentGoalRequest,
  type UpdateSpendingGoalRequest,
  formatAssignmentGoalType,
  formatSpendingGoalType,
} from "@/goals/types";
import { Box, Button, Paper, Stack } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useRef,
  useState,
} from "react";
import AssignmentGoalSetupSection from "@/funds/workspace/AssignmentGoalSetupSection";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Frame from "@/framework/view/Frame";
import SpendingGoalSetupSection from "@/funds/workspace/SpendingGoalSetupSection";
import TransactionDisplayField from "@/transactions/workspace/TransactionDisplayField";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import formatCurrency from "@/framework/formatCurrency";
import updateAssignmentGoal from "@/goals/workspace/updateAssignmentGoal";
import updateSpendingGoal from "@/goals/workspace/updateSpendingGoal";

/**
 * Props for the GoalForm component.
 */
interface UpdateGoalFormProps {
  readonly goal: AssignmentGoal | SpendingGoal;
  readonly redirectUrl: string;
}

const isAssignmentGoal = function (
  goal: AssignmentGoal | SpendingGoal,
): goal is AssignmentGoal {
  return "goalAmount" in goal;
};

/**
 * Component that displays the form for updating a goal.
 */
const UpdateGoalForm = function ({
  goal,
  redirectUrl,
}: UpdateGoalFormProps): JSX.Element {
  const assignmentGoal = isAssignmentGoal(goal) ? goal : null;
  const spendingGoal = isAssignmentGoal(goal) ? null : goal;
  const [assignmentGoalType, setAssignmentGoalType] =
    useState<AssignmentGoalType | null>(assignmentGoal?.type ?? null);
  const [assignmentGoalAmount, setAssignmentGoalAmount] = useState<
    number | null
  >(assignmentGoal?.goalAmount ?? null);
  const [spendingGoalType, setSpendingGoalType] =
    useState<SpendingGoalType | null>(spendingGoal?.type ?? null);
  const formRef = useRef<HTMLDivElement | null>(null);

  const [
    updateAssignmentState,
    updateAssignmentAction,
    updateAssignmentPending,
  ] = useActionState(updateAssignmentGoal, {});
  const [updateSpendingState, updateSpendingAction, updateSpendingPending] =
    useActionState(updateSpendingGoal, {});

  const reset = function (): void {
    setAssignmentGoalType(assignmentGoal?.type ?? null);
    setAssignmentGoalAmount(assignmentGoal?.goalAmount ?? null);
    setSpendingGoalType(spendingGoal?.type ?? null);
    focusFirstEntryControl(formRef.current);
  };

  useEffect(() => {
    if (
      updateAssignmentState.success === true ||
      updateSpendingState.success === true
    ) {
      reset();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [updateAssignmentState, updateSpendingState]);

  const updateAssignmentRequest: UpdateAssignmentGoalRequest | null =
    assignmentGoalType !== null && assignmentGoalAmount !== null
      ? {
          assignmentGoalType,
          goalAmount: assignmentGoalAmount,
        }
      : null;

  const updateSpendingRequest: UpdateSpendingGoalRequest | null =
    spendingGoalType !== null
      ? {
          spendingGoalType,
        }
      : null;

  return (
    <Stack ref={formRef} spacing={2}>
      <Stack spacing={3} sx={{ width: "100%" }}>
        <Frame title="Goal Context">
          <Box
            sx={{
              display: "grid",
              gap: 2,
              gridTemplateColumns:
                "repeat(auto-fit, minmax(min(100%, 220px), 1fr))",
            }}
          >
            <TransactionDisplayField
              label="Accounting Period"
              value={goal.accountingPeriodName ?? "Onboarded"}
            />
            <TransactionDisplayField label="Fund" value={goal.fundName} />
            <TransactionDisplayField
              label="Current Goal Type"
              value={
                assignmentGoal !== null
                  ? formatAssignmentGoalType(assignmentGoal.type)
                  : spendingGoal !== null
                    ? formatSpendingGoalType(spendingGoal.type)
                    : ""
              }
            />
            <TransactionDisplayField
              label={
                assignmentGoal !== null
                  ? "Remaining To Assign"
                  : "Remaining To Spend"
              }
              value={formatCurrency(
                assignmentGoal !== null
                  ? assignmentGoal.remainingAmountToAssign
                  : spendingGoal !== null
                    ? spendingGoal.remainingAmountToSpend
                    : 0,
              )}
            />
          </Box>
        </Frame>

        {assignmentGoal !== null ? (
          <AssignmentGoalSetupSection
            value={assignmentGoalType}
            setValue={setAssignmentGoalType}
            amount={assignmentGoalAmount}
            setAmount={setAssignmentGoalAmount}
            typeErrorMessage={updateAssignmentState.typeErrors ?? null}
            amountErrorMessage={updateAssignmentState.goalAmountErrors ?? null}
          />
        ) : (
          <SpendingGoalSetupSection
            value={spendingGoalType}
            setValue={setSpendingGoalType}
            typeErrorMessage={updateSpendingState.typeErrors ?? null}
          />
        )}

        <ErrorAlert
          errorMessage={
            updateAssignmentState.errorTitle ??
            updateSpendingState.errorTitle ??
            null
          }
          unmappedErrors={
            updateAssignmentState.unmappedErrors ??
            updateSpendingState.unmappedErrors ??
            null
          }
        />

        <Paper variant="outlined" sx={{ p: 2, borderRadius: 4 }}>
          <Stack
            direction={{ xs: "column", sm: "row" }}
            spacing={1.5}
            justifyContent="flex-end"
          >
            <Button variant="outlined" onClick={reset}>
              Reset
            </Button>
            <Button
              variant="contained"
              loading={updateAssignmentPending || updateSpendingPending}
              disabled={
                updateAssignmentRequest === null &&
                updateSpendingRequest === null
              }
              onClick={() => {
                if (
                  updateAssignmentRequest === null &&
                  updateSpendingRequest === null
                ) {
                  return;
                }
                startTransition(() => {
                  if (
                    assignmentGoal !== null &&
                    updateAssignmentRequest !== null
                  ) {
                    updateAssignmentAction({
                      goal: assignmentGoal,
                      request: updateAssignmentRequest,
                      redirectUrl,
                    });
                  } else if (
                    spendingGoal !== null &&
                    updateSpendingRequest !== null
                  ) {
                    updateSpendingAction({
                      goal: spendingGoal,
                      request: updateSpendingRequest,
                      redirectUrl,
                    });
                  }
                });
              }}
            >
              {assignmentGoal !== null
                ? "Update Assignment Goal"
                : "Update Spending Goal"}
            </Button>
          </Stack>
        </Paper>
      </Stack>
    </Stack>
  );
};

export default UpdateGoalForm;
