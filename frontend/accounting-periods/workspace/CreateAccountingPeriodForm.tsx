"use client";

import { Alert, Button, Stack } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useRef,
  useState,
} from "react";
import { ComboBoxEntryField } from "@/framework/forms/ComboBoxEntryField";
import type { CreateAccountingPeriodRequest } from "@/accounting-periods/types";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import IntegerEntryField from "@/framework/forms/IntegerEntryField";
import { accountingPeriodMonthOptions } from "@/accounting-periods/helpers";
import createAccountingPeriod from "@/accounting-periods/workspace/createAccountingPeriod";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";

/**
 * Props for the CreateAccountingPeriodForm component.
 */
interface CreateAccountingPeriodFormProps {
  readonly isInOnboardingMode: boolean;
  readonly redirectUrl: string;
}

/**
 * Displays the form for creating an accounting period.
 */
const CreateAccountingPeriodForm = function ({
  isInOnboardingMode,
  redirectUrl,
}: CreateAccountingPeriodFormProps): JSX.Element {
  const [year, setYear] = useState<number | null>(null);
  const [month, setMonth] = useState<number | null>(null);
  const formRef = useRef<HTMLDivElement | null>(null);
  const [state, action, pending] = useActionState(createAccountingPeriod, {});
  const selectedMonthOption =
    accountingPeriodMonthOptions.find((option) => option.value === month) ??
    null;

  const reset = function (): void {
    setYear(null);
    setMonth(null);
    focusFirstEntryControl(formRef.current);
  };

  useEffect(() => {
    if (state.success === true) {
      reset();
    }
  }, [state]);

  let request: CreateAccountingPeriodRequest | null = null;
  if (year !== null && month !== null) {
    request = { year, month };
  }

  return (
    <Stack ref={formRef} spacing={2}>
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
      <ComboBoxEntryField<number>
        label="Month"
        options={accountingPeriodMonthOptions}
        value={selectedMonthOption}
        setValue={(value) => {
          setMonth(value?.value ?? null);
        }}
        errorMessage={state.monthErrors ?? null}
      />
      <Stack direction="row" spacing={1} justifyContent="flex-end">
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
      </Stack>
      <ErrorAlert
        errorMessage={state.errorTitle ?? null}
        unmappedErrors={state.unmappedErrors ?? null}
      />
    </Stack>
  );
};

export default CreateAccountingPeriodForm;
