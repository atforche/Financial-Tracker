"use client";

import type {
  AccountingPeriod,
  CreateAccountingPeriodRequest,
} from "@/accounting-periods/types";
import { Alert, Button, Stack, Typography } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useState,
} from "react";
import {
  accountingPeriodMonthOptions,
  formatAccountingPeriodMonth,
} from "@/accounting-periods/helpers";
import { ComboBoxEntryField } from "@/framework/forms/ComboBoxEntryField";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import IntegerEntryField from "@/framework/forms/IntegerEntryField";
import createAccountingPeriod from "@/accounting-periods/workspace/createAccountingPeriod";
import { useRouter } from "next/navigation";

/**
 * Props for the CreateAccountingPeriodForm component.
 */
interface CreateAccountingPeriodFormProps {
  readonly isInOnboardingMode: boolean;
  readonly latestAccountingPeriod: AccountingPeriod | null;
  readonly open: boolean;
  readonly onClose: () => void;
  readonly redirectUrl: string;
}

/**
 * Displays the form for creating an accounting period.
 */
const CreateAccountingPeriodForm = function ({
  isInOnboardingMode,
  latestAccountingPeriod,
  open,
  onClose,
  redirectUrl,
}: CreateAccountingPeriodFormProps): JSX.Element {
  const router = useRouter();
  const [year, setYear] = useState<number | null>(null);
  const [month, setMonth] = useState<number | null>(null);
  const [state, action, pending] = useActionState(createAccountingPeriod, {});
  const selectedMonthOption =
    accountingPeriodMonthOptions.find((option) => option.value === month) ??
    null;

  useEffect(() => {
    if (state.success === true) {
      onClose();
      router.replace(redirectUrl, { scroll: false });
    }
  }, [onClose, redirectUrl, router, state.success]);

  const nextRequest: CreateAccountingPeriodRequest | null =
    latestAccountingPeriod === null
      ? null
      : latestAccountingPeriod.month === 12
        ? { year: latestAccountingPeriod.year + 1, month: 1 }
        : {
            year: latestAccountingPeriod.year,
            month: latestAccountingPeriod.month + 1,
          };
  const request: CreateAccountingPeriodRequest | null =
    isInOnboardingMode || latestAccountingPeriod === null
      ? year !== null && month !== null
        ? { year, month }
        : null
      : nextRequest;
  const title = isInOnboardingMode
    ? "Create First Accounting Period"
    : "Create Next Accounting Period";
  const nextPeriodName =
    nextRequest === null
      ? null
      : `${formatAccountingPeriodMonth(nextRequest.month)} ${nextRequest.year}`;

  return (
    <Dialog
      open={open}
      onClose={pending ? undefined : onClose}
      fullWidth
      maxWidth="sm"
      title={title}
      actions={
        <>
          <Button disabled={pending} onClick={onClose}>
            Cancel
          </Button>
          <Button
            variant="contained"
            loading={pending}
            disabled={request === null}
            onClick={() => {
              if (request !== null) {
                startTransition(() => {
                  action({ request, redirectUrl });
                });
              }
            }}
          >
            Create Period
          </Button>
        </>
      }
    >
      <Stack spacing={2}>
        {isInOnboardingMode ? (
          <Alert severity="info">
            You are currently in onboarding mode. Adding an accounting period
            will start regular data tracking. You will be unable to modify your
            onboarded accounts and funds once you proceed.
          </Alert>
        ) : null}
        {nextPeriodName === null ? (
          <>
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
          </>
        ) : (
          <Typography>
            Create the next accounting period, {nextPeriodName}?
          </Typography>
        )}
        <ErrorAlert
          errorMessage={state.errorTitle ?? null}
          unmappedErrors={state.unmappedErrors ?? null}
        />
      </Stack>
    </Dialog>
  );
};

export default CreateAccountingPeriodForm;
