"use client";

import type {
  AccountingPeriodWithBalance,
  ExpectedIncomeSource,
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
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import ExpectedIncomeSourcesEditor from "@/accounting-periods/workspace/ExpectedIncomeSourcesEditor";
import { IncomeBreakdownKindModel } from "@/framework/data/api";
import Link from "next/link";
import updateExpectedIncomeSources from "@/accounting-periods/workspace/updateExpectedIncomeSources";
import { useRouter } from "next/navigation";

/**
 * Mode for the ExpectedIncomeSourceForm component, indicating whether the form is being used to add, change, or delete an expected income source.
 */
type ExpectedIncomeSourceMode = "add" | "change";

/**
 * Props for the ExpectedIncomeSourceForm component.
 */
interface ExpectedIncomeSourceFormProps {
  readonly accountingPeriod: AccountingPeriodWithBalance;
  readonly mode: ExpectedIncomeSourceMode;
  readonly source?: ExpectedIncomeSource;
  readonly redirectUrl: string;
  readonly cancelUrl: string;
}

/**
 * Converts an expected income source response into an update request.
 */
const toRequest = (
  source: ExpectedIncomeSource,
): ExpectedIncomeSourceRequest => ({
  name: source.name,
  income: {
    kind: source.income.kind,
    trackedAmount: source.income.trackedAmount,
    untrackedAmount: source.income.untrackedAmount,
    earnings: source.income.earnings,
    employeeDeductions: source.income.employeeDeductions,
    employerContributions: source.income.employerContributions,
    taxWithholdings: source.income.taxWithholdings,
  },
  expectedDates: source.expectedDates,
});

/**
 * Gets an empty expected income source request.
 */
const emptySource = (): ExpectedIncomeSourceRequest => ({
  name: "",
  income: {
    kind: IncomeBreakdownKindModel.Payroll,
    trackedAmount: null,
    untrackedAmount: null,
    earnings: [],
    employeeDeductions: [],
    employerContributions: [],
    taxWithholdings: [],
  },
  expectedDates: [],
});

/**
 * Displays a page-level form for creating, editing, or deleting one expected income source.
 */
const ExpectedIncomeSourceForm = function ({
  accountingPeriod,
  mode,
  source,
  redirectUrl,
  cancelUrl,
}: ExpectedIncomeSourceFormProps): JSX.Element {
  const router = useRouter();
  const [draft, setDraft] = useState<ExpectedIncomeSourceRequest>(() =>
    source ? toRequest(source) : emptySource(),
  );
  const [state, action, pending] = useActionState(
    updateExpectedIncomeSources,
    {},
  );
  useEffect(() => {
    if (state.success === true) {
      router.replace(redirectUrl, { scroll: false });
    }
  }, [redirectUrl, router, state.success]);
  const save = (): void => {
    const existing = accountingPeriod.expectedIncomeSources.map(toRequest);
    const sourceIndex = source
      ? accountingPeriod.expectedIncomeSources.findIndex(
          (item) => item.id === source.id,
        )
      : -1;
    const sources =
      mode === "add"
        ? [...existing, draft]
        : existing.map((item, index) => (index === sourceIndex ? draft : item));
    startTransition(() => {
      action({ accountingPeriodId: accountingPeriod.id, redirectUrl, sources });
    });
  };
  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <ExpectedIncomeSourcesEditor
        sources={[draft]}
        setSources={(sources): void => {
          setDraft(sources[0] ?? emptySource());
        }}
        year={accountingPeriod.year}
        month={accountingPeriod.month}
        showSourceControls={false}
      />
      <ErrorAlert
        errorMessage={state.errorTitle ?? null}
        unmappedErrors={state.unmappedErrors ?? null}
      />
      <Stack direction="row" spacing={1} justifyContent="flex-end">
        <Link href={cancelUrl} style={{ textDecoration: "none" }}>
          <Button component="span" disabled={pending}>
            Cancel
          </Button>
        </Link>
        <Button variant="contained" loading={pending} onClick={save}>
          {mode === "add" ? "Add" : "Save Changes"}
        </Button>
      </Stack>
    </Stack>
  );
};

export type { ExpectedIncomeSourceMode };
export { toRequest };
export default ExpectedIncomeSourceForm;
