"use client";

import type {
  AccountingPeriodWithBalance,
  ExpectedIncomeSource,
  ExpectedIncomeSourceRequest,
} from "@/accounting-periods/types";
import { Button, Stack, Typography } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useState,
} from "react";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import ExpectedIncomeSourcesEditor from "@/accounting-periods/workspace/ExpectedIncomeSourcesEditor";
import Frame from "@/framework/view/Frame";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import TransactionWorkspacePageHeader from "@/transactions/workspace/TransactionWorkspacePageHeader";
import saveExpectedIncomeSource from "@/accounting-periods/workspace/saveExpectedIncomeSource";
import { useRouter } from "next/navigation";

/**
 * Mode for the ExpectedIncomeSourceForm component, indicating whether the form is being used to add, change, or delete an expected income source.
 */
type ExpectedIncomeSourceMode = "view" | "add" | "change" | "delete";

/**
 * Props for the ExpectedIncomeSourceForm component.
 */
interface ExpectedIncomeSourceFormProps {
  readonly accountingPeriod: AccountingPeriodWithBalance;
  readonly mode: ExpectedIncomeSourceMode;
  readonly source?: ExpectedIncomeSource;
  readonly backHref: string;
  readonly redirectUrl: string;
}

/**
 * Converts the provided expected income source into a request.
 */
const toRequest = (
  source: ExpectedIncomeSource,
): ExpectedIncomeSourceRequest => ({
  name: source.name,
  incomeLines: source.incomeLines,
  incomeDeductions: source.incomeDeductions,
  untrackedTransfers: source.untrackedTransfers,
  expectedDates: source.expectedDates,
});

/**
 * Gets an empty expected income source request.
 */
const emptySource = (): ExpectedIncomeSourceRequest => ({
  name: "",
  incomeLines: [{ description: "Income", amount: 0 }],
  incomeDeductions: [],
  untrackedTransfers: [],
  expectedDates: [],
});

/**
 * Dialog for adding, changing, or removing one expected-income source.
 */
const ExpectedIncomeSourceForm = function ({
  accountingPeriod,
  mode,
  source,
  backHref,
  redirectUrl,
}: ExpectedIncomeSourceFormProps): JSX.Element {
  const router = useRouter();
  const [draft, setDraft] = useState<ExpectedIncomeSourceRequest>(() =>
    source ? toRequest(source) : emptySource(),
  );
  const [state, action, pending] = useActionState(saveExpectedIncomeSource, {});
  useEffect(() => {
    if (state.success === true) {
      router.replace(redirectUrl, { scroll: false });
    }
  }, [redirectUrl, router, state.success]);
  const save = (): void => {
    startTransition(() => {
      action({
        accountingPeriodId: accountingPeriod.id,
        ...(source === undefined ? {} : { expectedIncomeSourceId: source.id }),
        redirectUrl,
        source: draft,
      });
    });
  };
  const actionLabel =
    mode === "add" ? "Add" : mode === "change" ? "Save Changes" : "Delete";
  const netAmount =
    draft.incomeLines.reduce((total, line) => total + line.amount, 0) -
    draft.incomeDeductions.reduce(
      (total, deduction) => total + deduction.amount,
      0,
    );
  const untrackedAmount = draft.untrackedTransfers.reduce(
    (total, transfer) => total + transfer.amount,
    0,
  );
  const trackedAmount = netAmount - untrackedAmount;
  const expectedAmount = netAmount * draft.expectedDates.length;
  const expectedTrackedAmount = trackedAmount * draft.expectedDates.length;
  const expectedUntrackedAmount = untrackedAmount * draft.expectedDates.length;
  return (
    <PageLayout>
      <TransactionWorkspacePageHeader
        backHref={backHref}
        title={`${mode === "add" ? "Add" : mode === "change" ? "Edit" : "Delete"} Expected Income Source`}
      />
      <ConstrainedContent maxWidth={1200}>
        <Stack spacing={3}>
          {mode === "delete" ? (
            <Typography>
              Delete {source?.name ?? "this expected income source"} from{" "}
              {accountingPeriod.name}?
            </Typography>
          ) : (
            <>
              <ExpectedIncomeSourcesEditor
                source={draft}
                setSource={setDraft}
                year={accountingPeriod.year}
                month={accountingPeriod.month}
              />
              <Frame title="Calculated Totals" color="info">
                <ResponsiveGrid columns={{ xs: 1, sm: 3 }} spacing={2}>
                  <CurrencyEntryField
                    label="Net per payment"
                    value={netAmount}
                  />
                  <CurrencyEntryField
                    label="Tracked per payment"
                    value={trackedAmount}
                  />
                  <CurrencyEntryField
                    label="Untracked per payment"
                    value={untrackedAmount}
                  />
                  <CurrencyEntryField
                    label="Expected total"
                    value={expectedAmount}
                  />
                  <CurrencyEntryField
                    label="Expected tracked"
                    value={expectedTrackedAmount}
                  />
                  <CurrencyEntryField
                    label="Expected untracked"
                    value={expectedUntrackedAmount}
                  />
                </ResponsiveGrid>
              </Frame>
            </>
          )}
          <ErrorAlert
            errorMessage={state.errorTitle ?? null}
            unmappedErrors={state.unmappedErrors ?? null}
          />
          <Stack
            direction={{ xs: "column", sm: "row" }}
            spacing={1.5}
            justifyContent="flex-end"
          >
            <Button variant="outlined" disabled={pending} href={backHref}>
              Cancel
            </Button>
            <Button
              color={mode === "delete" ? "error" : "primary"}
              variant="contained"
              loading={pending}
              onClick={save}
            >
              {actionLabel}
            </Button>
          </Stack>
        </Stack>
      </ConstrainedContent>
    </PageLayout>
  );
};

export type { ExpectedIncomeSourceMode };
export default ExpectedIncomeSourceForm;
