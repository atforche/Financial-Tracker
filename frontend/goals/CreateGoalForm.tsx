"use client";

import { Button, DialogActions, Stack } from "@mui/material";
import { type JSX, startTransition, useActionState, useState } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { Fund } from "@/funds/types";
import type { GoalType } from "@/goals/types";
import GoalTypeEntryField from "@/goals/GoalTypeEntryField";
import Link from "next/link";
import breadcrumbs from "@/accounting-periods/breadcrumbs";
import createGoal from "@/goals/createGoal";
import routes from "@/accounting-periods/routes";

/**
 * Props for the CreateGoalForm component.
 */
interface CreateGoalFormProps {
  readonly fund: Fund;
  readonly accountingPeriod: AccountingPeriod;
}

/**
 * Component that displays the form for creating a goal.
 */
const CreateGoalForm = function ({
  fund,
  accountingPeriod,
}: CreateGoalFormProps): JSX.Element {
  const [goalType, setGoalType] = useState<GoalType | null>(null);
  const [goalAmount, setGoalAmount] = useState<number | null>(null);

  const redirectUrl = routes.fundDetail(
    {
      id: accountingPeriod.id,
      fundId: fund.id,
    },
    {},
  );
  const [state, action, pending] = useActionState(createGoal, {
    fundId: fund.id,
    redirectUrl,
  });

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={breadcrumbs.goalCreate(accountingPeriod, fund)}
      />
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
                  fundId: fund.id,
                  accountingPeriodId: accountingPeriod.id,
                  goalType,
                  goalAmount,
                });
              });
            }}
          >
            Create
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

export default CreateGoalForm;
