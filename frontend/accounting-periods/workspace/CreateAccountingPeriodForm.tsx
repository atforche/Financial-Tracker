"use client";

import { Alert, Button, DialogActions, Stack } from "@mui/material";
import { type JSX, startTransition, useActionState, useState } from "react";
import type { CreateAccountingPeriodRequest } from "@/accounting-periods/types";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import IntegerEntryField from "@/framework/forms/IntegerEntryField";
import createAccountingPeriod from "@/accounting-periods/workspace/createAccountingPeriod";

/**
 * Props for the CreateAccountingPeriodForm component.
 */
interface CreateAccountingPeriodFormProps {
  readonly isInOnboardingMode: boolean;
  readonly redirectUrl: string;
}

/**
 * Components that displays the form for creating an accounting period.
 */
const CreateAccountingPeriodForm = function ({
  isInOnboardingMode,
  redirectUrl,
}: CreateAccountingPeriodFormProps): JSX.Element {
  const [year, setYear] = useState<number | null>(null);
  const [month, setMonth] = useState<number | null>(null);
  const [state, action, pending] = useActionState(createAccountingPeriod, {});

  const reset = function (): void {
    setYear(null);
    setMonth(null);
  };

  let request: CreateAccountingPeriodRequest | null = null;
  if (year !== null && month !== null) {
    request = { year, month };
  }

  return (
    <Stack spacing={2}>
      {isInOnboardingMode ? (
        <Alert severity="info">
          You are currently in onboarding mode. Adding an accounting period will
          start regular data tracking. You will be unable to modify your
          onboarded accounts and funds once you proceed.
        </Alert>
      ) : null}
      <IntegerEntryField
        label="Year"
        value={year}
        setValue={setYear}
        errorMessage={state.yearErrors ?? null}
      />
      <IntegerEntryField
        label="Month"
        value={month}
        setValue={setMonth}
        errorMessage={state.monthErrors ?? null}
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
            startTransition(() => {
              if (request !== null) {
                action({ request, redirectUrl });
              }
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
  );
};

export default CreateAccountingPeriodForm;
