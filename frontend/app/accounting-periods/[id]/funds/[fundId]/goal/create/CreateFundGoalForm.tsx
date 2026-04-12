"use client";

import { Button, DialogActions, Stack } from "@mui/material";
import type { Fund, FundGoalType } from "@/data/fundTypes";
import { type JSX, startTransition, useActionState, useState } from "react";
import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import FundGoalTypeEntryField from "@/framework/forms/FundGoalTypeEntryField";
import Link from "next/link";
import createFundGoal from "@/app/accounting-periods/[id]/funds/[fundId]/goal/create/createFundGoal";
import routes from "@/framework/routes";

/**
 * Props for the CreateFundGoalForm component.
 */
interface CreateFundGoalFormProps {
  readonly fund: Fund;
  readonly accountingPeriod: AccountingPeriod;
}

/**
 * Component that displays the form for creating a fund goal.
 */
const CreateFundGoalForm = function ({
  fund,
  accountingPeriod,
}: CreateFundGoalFormProps): JSX.Element {
  const [goalType, setGoalType] = useState<FundGoalType | null>(null);
  const [goalAmount, setGoalAmount] = useState<number | null>(null);

  const redirectUrl = routes.accountingPeriods.fundDetail(
    accountingPeriod.id,
    fund.id,
  );
  const [state, action, pending] = useActionState(createFundGoal, {
    fundId: fund.id,
    redirectUrl,
  });

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={[
          {
            label: "Accounting Periods",
            href: routes.accountingPeriods.index,
          },
          {
            label: accountingPeriod.name,
            href: routes.accountingPeriods.detail(accountingPeriod.id),
          },
          {
            label: fund.name,
            href: routes.accountingPeriods.fundDetail(
              accountingPeriod.id,
              fund.id,
            ),
          },
          {
            label: "Add Goal",
            href: routes.accountingPeriods.fundGoalCreate(
              accountingPeriod.id,
              fund.id,
            ),
          },
        ]}
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

export default CreateFundGoalForm;
