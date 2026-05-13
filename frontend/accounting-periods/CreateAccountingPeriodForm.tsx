"use client";

import { Alert, Button, DialogActions, Stack } from "@mui/material";
import { type JSX, startTransition, useActionState, useState } from "react";
import Breadcrumbs from "@/framework/Breadcrumbs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import IntegerEntryField from "@/framework/forms/IntegerEntryField";
import Link from "next/link";
import breadcrumbs from "@/accounting-periods/breadcrumbs";
import createAccountingPeriod from "@/accounting-periods/createAccountingPeriod";
import routes from "@/accounting-periods/routes";

/**
 * Props for the CreateAccountingPeriodForm component.
 */
interface CreateAccountingPeriodFormProps {
  readonly isInOnboardingMode: boolean;
}

/**
 * Components that diplays the form for creating an accounting period.
 */
const CreateAccountingPeriodForm = function ({
  isInOnboardingMode,
}: CreateAccountingPeriodFormProps): JSX.Element {
  const [year, setYear] = useState<number | null>(null);
  const [month, setMonth] = useState<number | null>(null);
  const [state, action, pending] = useActionState(createAccountingPeriod, {});

  return (
    <Stack spacing={2}>
      <Breadcrumbs breadcrumbs={breadcrumbs.create()} />
      <Stack spacing={2} sx={{ maxWidth: "500px" }}>
        {isInOnboardingMode ? (
          <Alert severity="info">
            You are currently in onboarding mode. Adding an accounting period
            will start regular data tracking. You will be unable to modify your
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
          <Link href={routes.index({})} tabIndex={-1}>
            <Button variant="outlined">Cancel</Button>
          </Link>
          <Button
            variant="contained"
            loading={pending}
            disabled={year === null || month === null}
            onClick={() => {
              startTransition(() => {
                action({ year: year ?? 0, month: month ?? 0 });
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

export default CreateAccountingPeriodForm;
