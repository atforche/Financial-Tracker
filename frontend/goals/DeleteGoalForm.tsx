"use client";

import { Button, DialogActions, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import Breadcrumbs from "@/framework/Breadcrumbs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { Fund } from "@/funds/types";
import type { Goal } from "@/goals/types";
import Link from "next/link";
import breadcrumbs from "@/accounting-periods/breadcrumbs";
import deleteGoal from "@/goals/deleteGoal";
import routes from "@/accounting-periods/routes";

/**
 * Props for the DeleteGoalForm component.
 */
interface DeleteGoalFormProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly fund: Fund;
  readonly goal: Goal;
}

/**
 * Component that displays the form for deleting a goal.
 */
const DeleteGoalForm = function ({
  accountingPeriod,
  fund,
  goal,
}: DeleteGoalFormProps): JSX.Element {
  const redirectUrl = routes.fundDetail(
    {
      id: goal.accountingPeriodId,
      fundId: fund.id,
    },
    {},
  );
  const [state, action, pending] = useActionState(deleteGoal, {
    goalId: goal.id,
    redirectUrl,
  });

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={breadcrumbs.goalDelete(accountingPeriod, fund)}
      />
      <Stack spacing={2} sx={{ maxWidth: "500px" }}>
        <Typography>
          Are you sure you want to delete the goal for {accountingPeriod.name}?
        </Typography>
        <DialogActions>
          <Link href={redirectUrl} tabIndex={-1}>
            <Button variant="outlined">Cancel</Button>
          </Link>
          <Button
            variant="contained"
            loading={pending}
            onClick={() => {
              startTransition(() => {
                action();
              });
            }}
          >
            Delete
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

export default DeleteGoalForm;
