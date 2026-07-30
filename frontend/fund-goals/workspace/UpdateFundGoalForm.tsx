"use client";

import { Button, Stack } from "@mui/material";
import type { FundGoal, UpdateFundGoalRequest } from "@/fund-goals/types";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useRef,
  useState,
} from "react";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import FundGoalSetupSection from "@/funds/workspace/FundGoalSetupSection";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import updateFundGoal from "@/fund-goals/workspace/updateFundGoal";

/**
 * Props for the UpdateFundGoalForm component.
 */
interface UpdateFundGoalFormProps {
  readonly fundGoal: FundGoal;
  readonly redirectUrl: string;
}

/**
 * Opens a dialog for updating a fund goal.
 */
const UpdateFundGoalForm = function ({
  fundGoal,
  redirectUrl,
}: UpdateFundGoalFormProps): JSX.Element {
  const [open, setOpen] = useState(false);
  const [regularContribution, setRegularContribution] = useState(
    fundGoal.regularContribution ?? null,
  );
  const [minimumFundedBalance, setMinimumFundedBalance] = useState(
    fundGoal.minimumFundedBalance ?? null,
  );
  const [maximumFundedBalance, setMaximumFundedBalance] = useState(
    fundGoal.maximumFundedBalance ?? null,
  );
  const [targetEndingBalance, setTargetEndingBalance] = useState(
    fundGoal.targetEndingBalance ?? null,
  );
  const formRef = useRef<HTMLDivElement | null>(null);
  const [state, action, pending] = useActionState(updateFundGoal, {});
  const reset = (): void => {
    setRegularContribution(fundGoal.regularContribution ?? null);
    setMinimumFundedBalance(fundGoal.minimumFundedBalance ?? null);
    setMaximumFundedBalance(fundGoal.maximumFundedBalance ?? null);
    setTargetEndingBalance(fundGoal.targetEndingBalance ?? null);
    focusFirstEntryControl(formRef.current);
  };
  useEffect(() => {
    if (state.success === true) {
      setOpen(false);
      reset();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [state.success]);
  const request: UpdateFundGoalRequest = {
    regularContribution,
    minimumFundedBalance,
    maximumFundedBalance,
    targetEndingBalance,
  };
  const rangeIsValid =
    minimumFundedBalance === null ||
    maximumFundedBalance === null ||
    minimumFundedBalance <= maximumFundedBalance;
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
        title="Update Fund Goal"
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
              disabled={!rangeIsValid}
              onClick={() => {
                startTransition(() => {
                  action({ fundGoal, request, redirectUrl });
                });
              }}
            >
              Save
            </Button>
          </>
        }
      >
        <Stack ref={formRef} spacing={3}>
          <FundGoalSetupSection
            regularContribution={regularContribution}
            setRegularContribution={setRegularContribution}
            minimumFundedBalance={minimumFundedBalance}
            setMinimumFundedBalance={setMinimumFundedBalance}
            maximumFundedBalance={maximumFundedBalance}
            setMaximumFundedBalance={setMaximumFundedBalance}
            targetEndingBalance={targetEndingBalance}
            setTargetEndingBalance={setTargetEndingBalance}
          />
          <ErrorAlert errorMessage={null} unmappedErrors={null} />
        </Stack>
      </Dialog>
    </>
  );
};
export default UpdateFundGoalForm;
