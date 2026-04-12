"use client";

import { Button, DialogActions, Stack } from "@mui/material";
import type { Fund, FundGoal, FundGoalType } from "@/data/fundTypes";
import { type JSX, startTransition, useActionState, useState } from "react";
import routes, { routeBreadcrumbs } from "@/framework/routes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import FundGoalTypeEntryField from "@/framework/forms/FundGoalTypeEntryField";
import Link from "next/link";
import updateFundGoal from "@/app/accounting-periods/[id]/funds/[fundId]/goal/update/updateFundGoal";

/**
 * Props for the UpdateFundGoalForm component.
 */
interface UpdateFundGoalFormProps {
  readonly fund: Fund;
  readonly fundGoal: FundGoal;
}

/**
 * Component that displays the form for updating a fund goal.
 */
const UpdateFundGoalForm = function ({
  fund,
  fundGoal,
}: UpdateFundGoalFormProps): JSX.Element {
  const [goalType, setGoalType] = useState<FundGoalType | null>(
    fundGoal.goalType,
  );
  const [goalAmount, setGoalAmount] = useState<number | null>(
    fundGoal.goalAmount,
  );

  const redirectUrl = routes.accountingPeriods.fundDetail(
    fundGoal.accountingPeriodId,
    fund.id,
  );
  const [state, action, pending] = useActionState(updateFundGoal, {
    fundId: fund.id,
    accountingPeriodId: fundGoal.accountingPeriodId,
    redirectUrl,
  });

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={routeBreadcrumbs.accountingPeriods.fundGoalUpdate(
          {
            id: fundGoal.accountingPeriodId,
            name: fundGoal.accountingPeriodName,
          },
          fund,
        )}
      />
      <Stack spacing={2} sx={{ maxWidth: "500px" }}>
        <FundGoalTypeEntryField
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
          <Link href={redirectUrl} tabIndex={-1}>
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

export default UpdateFundGoalForm;
