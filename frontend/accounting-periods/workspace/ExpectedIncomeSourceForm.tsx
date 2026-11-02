"use client";

import type {
  AccountingPeriod,
  ExpectedIncomeSource,
  ExpectedIncomeSourceRequest,
} from "@/accounting-periods/types";
import { Box, Button, Stack, Typography } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useState,
} from "react";
import ContentSurface from "@/framework/view/ContentSurface";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import ExpectedIncomePaymentCard from "@/accounting-periods/workspace/ExpectedIncomePaymentCard";
import ExpectedIncomePeriodCard from "@/accounting-periods/workspace/ExpectedIncomePeriodCard";
import ExpectedIncomeSourcesEditor from "@/accounting-periods/workspace/ExpectedIncomeSourcesEditor";
import PageLayout from "@/framework/view/PageLayout";
import ReturnLink from "@/framework/view/ReturnLink";
import StringEntryField from "@/framework/forms/StringEntryField";
import routes from "@/accounting-periods/routes";
import saveExpectedIncomeSource from "@/accounting-periods/workspace/saveExpectedIncomeSource";
import { useRouter } from "next/navigation";

/**
 * Props for the ExpectedIncomeSourceForm component.
 */
interface ExpectedIncomeSourceFormProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly source?: ExpectedIncomeSource;
  readonly backHref: string;
  readonly redirectUrl: string;
}

/**
 * Converts the provided expected income source into a request.
 */
const toRequest = function (
  source: ExpectedIncomeSource,
): ExpectedIncomeSourceRequest {
  return {
    name: source.name,
    incomeLines: source.incomeLines,
    incomeDeductions: source.incomeDeductions,
    untrackedTransfers: source.untrackedTransfers,
    expectedDates: source.expectedDates,
  };
};

/**
 * Gets an empty expected income source request.
 */
const emptySource = function (): ExpectedIncomeSourceRequest {
  return {
    name: "",
    incomeLines: [{ description: "Income", amount: 0 }],
    incomeDeductions: [],
    untrackedTransfers: [],
    expectedDates: [],
  };
};

/**
 * Form for adding an expected-income source, or changing the provided one, with live totals for the period and a
 * breakdown of each payment.
 */
const ExpectedIncomeSourceForm = function ({
  accountingPeriod,
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
      router.replace(redirectUrl);
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
  const actions = (
    <Stack
      direction="row"
      spacing={1}
      sx={{ "& > *": { flex: { xs: 1, sm: "none" } } }}
    >
      <Button variant="outlined" disabled={pending} href={backHref}>
        Cancel
      </Button>
      <Button variant="contained" loading={pending} onClick={save}>
        {source === undefined ? "Add" : "Save Changes"}
      </Button>
    </Stack>
  );
  return (
    <PageLayout>
      <Stack spacing={2}>
        <ReturnLink
          returnUrl={backHref}
          fallback={{
            href: routes.workspaceDetail(accountingPeriod.id, {}),
            label: `Back to ${accountingPeriod.name}`,
          }}
        />
        <Stack
          direction="row"
          useFlexGap
          spacing={2}
          sx={{
            flexWrap: "wrap",
            alignItems: "center",
            justifyContent: "space-between",
          }}
        >
          <Typography variant="h4" sx={{ overflowWrap: "anywhere" }}>
            {source === undefined
              ? `Add Expected Income to ${accountingPeriod.name}`
              : `Edit ${source.name}`}
          </Typography>
          <Box sx={{ display: { xs: "none", sm: "block" } }}>{actions}</Box>
        </Stack>
        <ErrorAlert
          errorMessage={state.errorTitle ?? null}
          unmappedErrors={state.unmappedErrors ?? null}
        />
      </Stack>
      <Box
        sx={{
          display: "grid",
          gap: 2,
          gridTemplateColumns: { xs: "minmax(0, 1fr)", lg: "2fr 1fr" },
          alignItems: "start",
        }}
      >
        <ContentSurface>
          <Stack spacing={2}>
            <StringEntryField
              label="Source Name"
              value={draft.name}
              setValue={(name): void => {
                setDraft({ ...draft, name });
              }}
            />
            <ExpectedIncomeSourcesEditor
              source={draft}
              setSource={setDraft}
              year={accountingPeriod.year}
              month={accountingPeriod.month}
            />
          </Stack>
        </ContentSurface>
        <Stack spacing={2} sx={{ position: { lg: "sticky" }, top: { lg: 10 } }}>
          <ExpectedIncomePeriodCard source={draft} />
          <ExpectedIncomePaymentCard source={draft} />
        </Stack>
      </Box>
      {/* On small screens the actions stay pinned to the bottom so they remain in reach while scrolling */}
      <Box
        sx={{
          display: { xs: "block", sm: "none" },
          position: "sticky",
          bottom: 0,
          zIndex: "appBar",
          py: 1.5,
          bgcolor: "background.default",
          borderTop: 1,
          borderColor: "divider",
        }}
      >
        {actions}
      </Box>
    </PageLayout>
  );
};

export default ExpectedIncomeSourceForm;
