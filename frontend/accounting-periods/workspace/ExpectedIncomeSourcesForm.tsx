"use client";

import type {
  AccountingPeriodWithBalance,
  ExpectedIncomeSourceRequest,
} from "@/accounting-periods/types";
import { Button, Stack } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useState,
} from "react";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import ExpectedIncomeSourcesEditor from "@/accounting-periods/workspace/ExpectedIncomeSourcesEditor";
import updateExpectedIncomeSources from "@/accounting-periods/workspace/updateExpectedIncomeSources";
import { useRouter } from "next/navigation";

/**
 * Props for the ExpectedIncomeSourcesForm component.
 */
interface ExpectedIncomeSourcesFormProps {
  readonly accountingPeriod: AccountingPeriodWithBalance;
  readonly open: boolean;
  readonly onClose: () => void;
  readonly redirectUrl: string;
}

/**
 * Dialog for changing expected-income sources on an open Accounting Period.
 */
const ExpectedIncomeSourcesForm = function ({
  accountingPeriod,
  open,
  onClose,
  redirectUrl,
}: ExpectedIncomeSourcesFormProps): JSX.Element {
  const router = useRouter();
  const [sources, setSources] = useState<ExpectedIncomeSourceRequest[]>(() =>
    accountingPeriod.expectedIncomeSources.map((source) => ({
      name: source.name,
      income: {
        kind: source.income.kind,
        trackedAmount: source.trackedAmount,
        untrackedAmount: source.untrackedAmount,
        earnings: source.income.earnings,
        employeeDeductions: source.income.employeeDeductions,
        employerContributions: source.income.employerContributions,
        taxWithholdings: source.income.taxWithholdings,
      },
      expectedDates: source.expectedDates,
    })),
  );
  const [state, action, pending] = useActionState(
    updateExpectedIncomeSources,
    {},
  );

  useEffect(() => {
    if (state.success === true) {
      onClose();
      router.replace(redirectUrl, { scroll: false });
    }
  }, [onClose, redirectUrl, router, state.success]);

  return (
    <Dialog
      open={open}
      onClose={pending ? undefined : onClose}
      fullWidth
      maxWidth="md"
      title={`Expected Income: ${accountingPeriod.name}`}
      actions={
        <>
          <Button disabled={pending} onClick={onClose}>
            Cancel
          </Button>
          <Button
            variant="contained"
            loading={pending}
            onClick={() => {
              startTransition(() => {
                action({
                  accountingPeriodId: accountingPeriod.id,
                  redirectUrl,
                  sources,
                });
              });
            }}
          >
            Save
          </Button>
        </>
      }
    >
      <Stack spacing={2}>
        <ExpectedIncomeSourcesEditor
          sources={sources}
          setSources={setSources}
          year={accountingPeriod.year}
          month={accountingPeriod.month}
        />
        <ErrorAlert
          errorMessage={state.errorTitle ?? null}
          unmappedErrors={state.unmappedErrors ?? null}
        />
      </Stack>
    </Dialog>
  );
};

export default ExpectedIncomeSourcesForm;
