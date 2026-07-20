"use client";

import { Button, Stack } from "@mui/material";
import type { FundPlan, UpdateFundPlanRequest } from "@/fund-plans/types";
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
import FundPlanSetupSection from "@/funds/workspace/FundPlanSetupSection";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import updateFundPlan from "@/fund-plans/workspace/updateFundPlan";

/**
 * Props for the UpdateFundPlanForm component.
 */
interface UpdateFundPlanFormProps {
  readonly fundPlan: FundPlan;
  readonly redirectUrl: string;
}

/**
 * Opens a dialog for updating a fund plan.
 */
const UpdateFundPlanForm = function ({
  fundPlan,
  redirectUrl,
}: UpdateFundPlanFormProps): JSX.Element {
  const [open, setOpen] = useState(false);
  const [regularContribution, setRegularContribution] = useState(
    fundPlan.regularContribution ?? null,
  );
  const [minimumFundedBalance, setMinimumFundedBalance] = useState(
    fundPlan.minimumFundedBalance ?? null,
  );
  const [maximumFundedBalance, setMaximumFundedBalance] = useState(
    fundPlan.maximumFundedBalance ?? null,
  );
  const [targetEndingBalance, setTargetEndingBalance] = useState(
    fundPlan.targetEndingBalance ?? null,
  );
  const formRef = useRef<HTMLDivElement | null>(null);
  const [state, action, pending] = useActionState(updateFundPlan, {});
  const reset = (): void => {
    setRegularContribution(fundPlan.regularContribution ?? null);
    setMinimumFundedBalance(fundPlan.minimumFundedBalance ?? null);
    setMaximumFundedBalance(fundPlan.maximumFundedBalance ?? null);
    setTargetEndingBalance(fundPlan.targetEndingBalance ?? null);
    focusFirstEntryControl(formRef.current);
  };
  useEffect(() => {
    if (state.success === true) {
      setOpen(false);
      reset();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [state.success]);
  const request: UpdateFundPlanRequest = {
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
        title="Update Funding Plan"
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
                  action({ fundPlan, request, redirectUrl });
                });
              }}
            >
              Save
            </Button>
          </>
        }
      >
        <Stack ref={formRef} spacing={3}>
          <FundPlanSetupSection
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
export default UpdateFundPlanForm;
