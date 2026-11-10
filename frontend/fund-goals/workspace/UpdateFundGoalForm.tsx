"use client";

import { Button, Stack } from "@mui/material";
import type { FundGoal, UpdateFundGoalRequest } from "@/fund-goals/types";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useEffectEvent,
  useRef,
  useState,
} from "react";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import FormDialog from "@/framework/dialog/FormDialog";
import FundGoalSetupSection from "@/funds/workspace/FundGoalSetupSection";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import getAccountingPeriodBalance from "@/accounting-periods/workspace/getAccountingPeriodBalance";
import updateFundGoal from "@/fund-goals/workspace/updateFundGoal";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the UpdateFundGoalForm component.
 */
interface UpdateFundGoalFormProps {
  readonly fundGoal: FundGoal;
  readonly accountingPeriodId: string;
  readonly redirectUrl: string;
}

/**
 * Opens a dialog for updating a fund goal.
 */
const UpdateFundGoalForm = function ({
  fundGoal,
  accountingPeriodId,
  redirectUrl,
}: UpdateFundGoalFormProps): JSX.Element | null {
  const canWrite = useWriteAccess();
  const [open, setOpen] = useState(false);
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriodWithBalance | null>(null);
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
  const handleSuccess = useEffectEvent((): void => {
    setOpen(false);
    reset();
  });
  useEffect(() => {
    if (state.success === true) {
      handleSuccess();
    }
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
          setAccountingPeriod(null);
          startTransition(async () => {
            setAccountingPeriod(
              await getAccountingPeriodBalance(accountingPeriodId),
            );
          });
        }}
      >
        Edit
      </Button>
      <FormDialog
        open={open}
        title="Update Fund Goal"
        onClose={() => {
          setOpen(false);
          reset();
        }}
        pending={pending}
        submitLabel="Save"
        onSubmit={() => {
          startTransition(() => {
            action({ fundGoal, request, redirectUrl });
          });
        }}
        canSubmit={rangeIsValid}
        extraActions={
          <Button variant="outlined" disabled={pending} onClick={reset}>
            Reset
          </Button>
        }
        maxWidth="md"
      >
        <Stack ref={formRef} spacing={3}>
          <FundGoalSetupSection
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
            plannedMonthlyContributionErrors={
              state.plannedMonthlyContributionErrors ?? null
            }
            minimumEndingBalanceErrors={
              state.minimumEndingBalanceErrors ?? null
            }
            maximumEndingBalanceErrors={
              state.maximumEndingBalanceErrors ?? null
            }
          />
          <ErrorAlert
            errorMessage={state.errorTitle ?? null}
            unmappedErrors={state.unmappedErrors ?? null}
          />
        </Stack>
      </FormDialog>
    </>
  );
};
export default UpdateFundGoalForm;
