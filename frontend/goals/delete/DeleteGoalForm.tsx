"use client";

import { Button, DialogActions, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import Breadcrumbs from "@/framework/Breadcrumbs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { Fund } from "@/funds/types";
import type { Goal } from "@/goals/types";
import Link from "next/link";
import { ToggleState } from "@/accounting-periods/detail/AccountingPeriodDetailViewListFrames";
import accountingPeriodRoutes from "@/accounting-periods/routes";
import breadcrumbs from "@/goals/breadcrumbs";
import deleteGoal from "@/goals/delete/deleteGoal";
import routes from "@/goals/routes";

/**
 * Props for the DeleteGoalForm component.
 */
interface DeleteGoalFormProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly goal: Goal;
  readonly fund: Fund | null;
}

/**
 * Component that displays the form for deleting a goal.
 */
const DeleteGoalForm = function ({
  accountingPeriod,
  goal,
  fund,
}: DeleteGoalFormProps): JSX.Element {
  const cancelUrl = routes.detail({ id: goal.id }, {});
  const redirectUrl =
    fund !== null
      ? accountingPeriodRoutes.fundDetail(
          {
            id: accountingPeriod.id,
            fundId: fund.id,
          },
          {},
        )
      : accountingPeriodRoutes.detail(
          { id: accountingPeriod.id },
          { display: ToggleState.Goals },
        );
  const [state, action, pending] = useActionState(deleteGoal, {
    goalId: goal.id,
    redirectUrl,
  });

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={breadcrumbs.delete(accountingPeriod, goal, fund)}
      />
      <Stack spacing={2} sx={{ maxWidth: "500px" }}>
        <Typography>
          Are you sure you want to delete the goal for {goal.fundName} in{" "}
          {accountingPeriod.name}?
        </Typography>
        <DialogActions>
          <Link href={cancelUrl} tabIndex={-1}>
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
