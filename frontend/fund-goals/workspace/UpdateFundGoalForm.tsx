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
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import FundGoalSetupSection from "@/funds/workspace/FundGoalSetupSection";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import updateFundGoal from "@/fund-goals/workspace/updateFundGoal";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the UpdateFundGoalForm component.
 */
interface UpdateFundGoalFormProps {
  readonly fundGoal: FundGoal;
  readonly accountingPeriod: AccountingPeriodWithBalance;
  readonly redirectUrl: string;
}

/**
 * Opens a dialog for updating a fund goal.
 */
const UpdateFundGoalForm = function ({
  fundGoal,
  accountingPeriod,
  redirectUrl,
}: UpdateFundGoalFormProps): JSX.Element | null {
  const canWrite = useWriteAccess();
  const [open, setOpen] = useState(false);
  const [plannedMonthlyContribution, setPlannedMonthlyContribution] = useState(
    fundGoal.plannedMonthlyContribution ?? null,
  );
  const [minimumEndingBalance, setMinimumEndingBalance] = useState(
    fundGoal.minimumEndingBalance,
  );
  const [maximumEndingBalance, setMaximumEndingBalance] = useState(
    fundGoal.maximumEndingBalance ?? null,
  );
  const [
    allowExpectedContributionAboveMaximum,
    setAllowExpectedContributionAboveMaximum,
  ] = useState(fundGoal.allowExpectedContributionAboveMaximum ?? false);
  const formRef = useRef<HTMLDivElement | null>(null);
  const [state, action, pending] = useActionState(updateFundGoal, {});
  const reset = (): void => {
    setPlannedMonthlyContribution(fundGoal.plannedMonthlyContribution ?? null);
    setMinimumEndingBalance(fundGoal.minimumEndingBalance);
    setMaximumEndingBalance(fundGoal.maximumEndingBalance ?? null);
    setAllowExpectedContributionAboveMaximum(
      fundGoal.allowExpectedContributionAboveMaximum ?? false,
    );
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
    plannedMonthlyContribution,
    minimumEndingBalance,
    maximumEndingBalance,
    allowExpectedContributionAboveMaximum,
  };
  const rangeIsValid =
    maximumEndingBalance === null ||
    minimumEndingBalance <= maximumEndingBalance;
  if (!canWrite) {
    return null;
  }

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
            showFrame={false}
            accountingPeriod={accountingPeriod}
            originalPlannedMonthlyContribution={
              fundGoal.plannedMonthlyContribution ?? null
            }
            plannedMonthlyContribution={plannedMonthlyContribution}
            setPlannedMonthlyContribution={setPlannedMonthlyContribution}
            minimumEndingBalance={minimumEndingBalance}
            setMinimumEndingBalance={setMinimumEndingBalance}
            maximumEndingBalance={maximumEndingBalance}
            setMaximumEndingBalance={setMaximumEndingBalance}
            allowExpectedContributionAboveMaximum={
              allowExpectedContributionAboveMaximum
            }
            setAllowExpectedContributionAboveMaximum={
              setAllowExpectedContributionAboveMaximum
            }
          />
          <ErrorAlert errorMessage={null} unmappedErrors={null} />
        </Stack>
      </Dialog>
    </>
  );
};
export default UpdateFundGoalForm;
