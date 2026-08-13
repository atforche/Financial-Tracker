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
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import ExpectedIncomeSourcesEditor from "@/accounting-periods/workspace/ExpectedIncomeSourcesEditor";
import updateExpectedIncomeSources from "@/accounting-periods/workspace/updateExpectedIncomeSources";
import { useRouter } from "next/navigation";

/**
 * Mode for the ExpectedIncomeSourceForm component, indicating whether the form is being used to add, change, or delete an expected income source.
 */
type ExpectedIncomeSourceMode = "add" | "change" | "delete";

/**
 * Props for the ExpectedIncomeSourceForm component.
 */
interface ExpectedIncomeSourceFormProps {
  readonly accountingPeriod: AccountingPeriodWithBalance;
  readonly mode: ExpectedIncomeSourceMode;
  readonly source?: ExpectedIncomeSource;
  readonly open: boolean;
  readonly onClose: () => void;
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
  expectedDates: source.expectedDates,
});

/**
 * Gets an empty expected income source request.
 */
const emptySource = (): ExpectedIncomeSourceRequest => ({
  name: "",
  incomeLines: [{ description: "Income", amount: 0 }],
  incomeDeductions: [],
  expectedDates: [],
});

/** 
 * Dialog for adding, changing, or removing one expected-income source.
 */
const ExpectedIncomeSourceForm = function ({
  accountingPeriod,
  mode,
  source,
  open,
  onClose,
  redirectUrl,
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
      onClose();
      router.replace(redirectUrl, { scroll: false });
    }
  }, [onClose, redirectUrl, router, state.success]);
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
        : mode === "change"
          ? existing.map((item, index) =>
              index === sourceIndex ? draft : item,
            )
          : existing.filter((_, index) => index !== sourceIndex);
    startTransition(() => {
      action({ accountingPeriodId: accountingPeriod.id, redirectUrl, sources });
    });
  };
  const actionLabel =
    mode === "add" ? "Add" : mode === "change" ? "Save Changes" : "Delete";
  return (
    <Dialog
      open={open}
      onClose={pending ? undefined : onClose}
      fullWidth
      maxWidth="md"
      title={`${mode === "add" ? "Add" : mode === "change" ? "Change" : "Delete"} Expected Income Source`}
      actions={
        <>
          <Button disabled={pending} onClick={onClose}>
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
        </>
      }
    >
      <Stack spacing={2}>
        {mode === "delete" ? (
          <Typography>
            Delete {source?.name ?? "this expected income source"} from{" "}
            {accountingPeriod.name}?
          </Typography>
        ) : (
          <ExpectedIncomeSourcesEditor
            sources={[draft]}
            setSources={(sources) => {
              setDraft(sources[0] ?? emptySource());
            }}
            year={accountingPeriod.year}
            month={accountingPeriod.month}
            showSourceControls={false}
          />
        )}
        <ErrorAlert
          errorMessage={state.errorTitle ?? null}
          unmappedErrors={state.unmappedErrors ?? null}
        />
      </Stack>
    </Dialog>
  );
};

export type { ExpectedIncomeSourceMode };
export default ExpectedIncomeSourceForm;
