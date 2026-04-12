"use client";

import { Button, DialogActions, Stack, Typography } from "@mui/material";
import type { Fund, FundGoal } from "@/data/fundTypes";
import { type JSX, startTransition, useActionState } from "react";
import routes, { routeBreadcrumbs } from "@/framework/routes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Link from "next/link";
import deleteFundGoal from "@/app/accounting-periods/[id]/funds/[fundId]/goal/delete/deleteFundGoal";

/**
 * Props for the DeleteFundGoalForm component.
 */
interface DeleteFundGoalFormProps {
  readonly fund: Fund;
  readonly fundGoal: FundGoal;
}

/**
 * Component that displays the form for deleting a fund goal.
 */
const DeleteFundGoalForm = function ({
  fund,
  fundGoal,
}: DeleteFundGoalFormProps): JSX.Element {
  const redirectUrl = routes.accountingPeriods.fundDetail(
    fundGoal.accountingPeriodId,
    fund.id,
  );
  const [state, action, pending] = useActionState(deleteFundGoal, {
    fundId: fund.id,
    accountingPeriodId: fundGoal.accountingPeriodId,
    redirectUrl,
  });

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={routeBreadcrumbs.accountingPeriods.fundGoalDelete(
          {
            id: fundGoal.accountingPeriodId,
            name: fundGoal.accountingPeriodName,
          },
          fund,
        )}
      />
      <Stack spacing={2} sx={{ maxWidth: "500px" }}>
        <Typography>
          Are you sure you want to delete the goal for{" "}
          {fundGoal.accountingPeriodName}?
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

export default DeleteFundGoalForm;
