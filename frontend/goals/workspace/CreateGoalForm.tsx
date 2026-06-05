"use client";

import { Button, DialogActions, Stack } from "@mui/material";
import type { CreateGoalRequest, GoalType } from "@/goals/types";
import type { Fund, FundIdentifier } from "@/funds/types";
import { type JSX, startTransition, useActionState, useState } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodEntryField from "@/accounting-periods/AccountingPeriodEntryField";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import FundEntryField from "@/funds/FundEntryField";
import GoalTypeEntryField from "@/goals/GoalTypeEntryField";
import createGoal from "@/goals/workspace/createGoal";

/**
 * Props for the CreateGoalForm component.
 */
interface CreateGoalFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly funds: Fund[];
  readonly redirectUrl: string;
}

/**
 * Component that displays the form for creating a goal.
 */
const CreateGoalForm = function ({
  accountingPeriods,
  funds,
  redirectUrl,
}: CreateGoalFormProps): JSX.Element {
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(
      accountingPeriods.length > 0
        ? (accountingPeriods[accountingPeriods.length - 1] ?? null)
        : null,
    );
  const [fund, setFund] = useState<FundIdentifier | null>(null);
  const [goalType, setGoalType] = useState<GoalType | null>(null);
  const [goalAmount, setGoalAmount] = useState<number | null>(null);

  const [state, action, pending] = useActionState(createGoal, {});

  let request: CreateGoalRequest | null = null;
  if (
    accountingPeriod !== null &&
    fund !== null &&
    goalType !== null &&
    goalAmount !== null
  ) {
    request = {
      accountingPeriodId: accountingPeriod.id,
      fundId: fund.id,
      goalType,
      goalAmount,
    };
  }

  const availableFunds = funds.filter(
    (fundOption) =>
      accountingPeriod !== null && fundOption.name !== "Unassigned",
  );

  const reset = function (): void {
    setAccountingPeriod(
      accountingPeriods.length > 0
        ? (accountingPeriods[accountingPeriods.length - 1] ?? null)
        : null,
    );
    setFund(null);
    setGoalType(null);
    setGoalAmount(null);
  };

  return (
    <Stack spacing={2}>
      <Stack spacing={2} sx={{ maxWidth: "500px" }}>
        <AccountingPeriodEntryField
          label="Accounting Period"
          options={accountingPeriods}
          value={accountingPeriod}
          setValue={setAccountingPeriod}
          errorMessage={state.accountingPeriodErrors ?? null}
        />
        <FundEntryField
          label="Fund"
          options={availableFunds}
          value={fund}
          setValue={setFund}
          filter={null}
        />
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
          <Button variant="outlined" onClick={reset}>
            Reset
          </Button>
          <Button
            variant="contained"
            loading={pending}
            disabled={request === null}
            onClick={() => {
              if (request === null) {
                return;
              }
              startTransition(() => {
                action({
                  redirectUrl,
                  request,
                });
              });
            }}
          >
            Create
          </Button>
        </DialogActions>
        <ErrorAlert
          errorMessage={state.errorTitle ?? null}
          unmappedErrors={
            [state.fundErrors ?? null, state.unmappedErrors ?? null]
              .filter((message) => message !== null)
              .join(", ") || null
          }
        />
      </Stack>
    </Stack>
  );
};

export default CreateGoalForm;
