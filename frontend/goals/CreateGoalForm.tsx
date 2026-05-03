"use client";

import { Button, DialogActions, Stack } from "@mui/material";
import type { Fund, FundIdentifier } from "@/funds/types";
import { type JSX, startTransition, useActionState, useState } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodEntryField from "@/accounting-periods/AccountingPeriodEntryField";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import FundEntryField from "@/funds/FundEntryField";
import type { GoalType } from "@/goals/types";
import GoalTypeEntryField from "@/goals/GoalTypeEntryField";
import Link from "next/link";
import { ToggleState } from "@/accounting-periods/AccountingPeriodViewListFrames";
import accountingPeriodRoutes from "@/accounting-periods/routes";
import breadcrumbs from "@/goals/breadcrumbs";
import createGoal from "@/goals/createGoal";

/**
 * Props for the CreateGoalForm component.
 */
interface CreateGoalFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly funds: Fund[];
  readonly providedAccountingPeriod?: AccountingPeriod | null;
  readonly providedFund?: Fund | null;
}

/**
 * Gets the URL to return the user to after creating a goal.
 */
const getRedirectUrl = function (
  providedAccountingPeriod: AccountingPeriod | null,
  providedFund: Fund | null,
): string {
  if (providedAccountingPeriod !== null && providedFund !== null) {
    return accountingPeriodRoutes.fundDetail(
      {
        id: providedAccountingPeriod.id,
        fundId: providedFund.id,
      },
      {},
    );
  }
  if (providedAccountingPeriod !== null) {
    return accountingPeriodRoutes.detail(
      { id: providedAccountingPeriod.id },
      { display: ToggleState.Goals },
    );
  }
  return accountingPeriodRoutes.index({});
};

/**
 * Component that displays the form for creating a goal.
 */
const CreateGoalForm = function ({
  accountingPeriods,
  funds,
  providedAccountingPeriod = null,
  providedFund = null,
}: CreateGoalFormProps): JSX.Element {
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(providedAccountingPeriod);
  const [fund, setFund] = useState<FundIdentifier | null>(
    providedFund ? { id: providedFund.id, name: providedFund.name } : null,
  );
  const [goalType, setGoalType] = useState<GoalType | null>(null);
  const [goalAmount, setGoalAmount] = useState<number | null>(null);

  const redirectUrl = getRedirectUrl(providedAccountingPeriod, providedFund);
  const [state, action, pending] = useActionState(createGoal, {
    redirectUrl,
  });

  const canSubmit =
    accountingPeriod !== null &&
    fund !== null &&
    goalType !== null &&
    goalAmount !== null;
  const availableFunds = funds.filter(
    (fundOption) =>
      accountingPeriod !== null && fundOption.name !== "Unassigned",
  );

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={breadcrumbs.create(providedAccountingPeriod, providedFund)}
      />
      <Stack spacing={2} sx={{ maxWidth: "500px" }}>
        <AccountingPeriodEntryField
          label="Accounting Period"
          options={accountingPeriods}
          value={accountingPeriod}
          setValue={
            providedAccountingPeriod === null ? setAccountingPeriod : null
          }
          errorMessage={state.accountingPeriodErrors ?? null}
        />
        <FundEntryField
          label="Fund"
          options={availableFunds}
          value={fund}
          setValue={providedFund === null ? setFund : null}
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
          <Link href={redirectUrl} tabIndex={-1}>
            <Button variant="outlined">Cancel</Button>
          </Link>
          <Button
            variant="contained"
            loading={pending}
            disabled={!canSubmit}
            onClick={() => {
              if (
                accountingPeriod === null ||
                fund === null ||
                goalType === null ||
                goalAmount === null
              ) {
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
