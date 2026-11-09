"use client";

import type {
  AccountingPeriod,
  AccountingPeriodWithBalance,
} from "@/accounting-periods/types";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useEffectEvent,
  useState,
} from "react";
import { Stack, Typography } from "@mui/material";
import AccountingPeriodEntryField from "@/accounting-periods/AccountingPeriodEntryField";
import ContentSurface from "@/framework/view/ContentSurface";
import FormPage from "@/framework/view/FormPage";
import FundGoalSetupSection from "@/funds/workspace/FundGoalSetupSection";
import type { Route } from "next";
import StringEntryField from "@/framework/forms/StringEntryField";
import { buildCreateFundRequest } from "@/funds/workspace/helpers";
import createFund from "@/funds/workspace/createFund";
import useFundSetupState from "@/funds/workspace/useFundSetupState";
import { useRouter } from "next/navigation";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the CreateFundForm component.
 */
interface CreateFundFormProps {
  readonly accountingPeriods: AccountingPeriodWithBalance[];
  readonly redirectUrl: Route;
}

/**
 * Component that displays the form for creating a fund.
 */
const CreateFundForm = function ({
  accountingPeriods,
  redirectUrl,
}: CreateFundFormProps): JSX.Element | null {
  const canWrite = useWriteAccess();
  const router = useRouter();
  const fundSetup = useFundSetupState();
  const {
    name,
    setName,
    description,
    setDescription,
    plannedMonthlyContribution,
    setPlannedMonthlyContribution,
    minimumEndingBalance,
    setMinimumEndingBalance,
    maximumEndingBalance,
    setMaximumEndingBalance,
    allowExpectedContributionAboveMaximum,
    setAllowExpectedContributionAboveMaximum,
  } = fundSetup;
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(null);
  const accountingPeriodWithBalance =
    accountingPeriods.find((period) => period.id === accountingPeriod?.id) ??
    null;

  const [state, action, pending] = useActionState(createFund, {});

  const handleSuccess = useEffectEvent((): void => {
    router.replace(redirectUrl);
  });
  useEffect(() => {
    if (state.success === true) {
      handleSuccess();
    }
  }, [state.success]);

  const request = buildCreateFundRequest({
    name,
    description,
    accountingPeriod,
    plannedMonthlyContribution,
    minimumEndingBalance,
    maximumEndingBalance,
    allowExpectedContributionAboveMaximum,
  });

  if (!canWrite) {
    return null;
  }

  return (
    <FormPage
      title="Create Fund"
      returnHref={redirectUrl}
      returnLabel="Back to Funds"
      submitLabel="Create Fund"
      canSubmit={request !== null}
      pending={pending}
      onSubmit={() => {
        if (request === null) {
          return;
        }
        startTransition(() => {
          action({ redirectUrl, request });
        });
      }}
      errorTitle={state.errorTitle ?? null}
      unmappedErrors={state.unmappedErrors ?? null}
    >
      <ContentSurface>
        <Stack spacing={2}>
          <Typography variant="h5">Fund</Typography>
          <StringEntryField
            label="Name"
            value={name}
            setValue={setName}
            errorMessage={state.nameErrors ?? null}
          />
          <StringEntryField
            label="Description"
            value={description}
            setValue={setDescription}
            errorMessage={state.descriptionErrors ?? null}
          />
          <AccountingPeriodEntryField
            label="Accounting Period"
            options={accountingPeriods}
            value={accountingPeriod}
            setValue={setAccountingPeriod}
            errorMessage={state.accountingPeriodErrors ?? null}
          />
        </Stack>
      </ContentSurface>
      <ContentSurface>
        <Stack spacing={2}>
          <Typography variant="h5">Goal</Typography>
          <FundGoalSetupSection
            accountingPeriod={accountingPeriodWithBalance}
            plannedMonthlyContribution={plannedMonthlyContribution}
            setPlannedMonthlyContribution={setPlannedMonthlyContribution}
            minimumEndingBalance={minimumEndingBalance}
            setMinimumEndingBalance={setMinimumEndingBalance}
            maximumEndingBalance={maximumEndingBalance}
            setMaximumEndingBalance={setMaximumEndingBalance}
            allowExpectedContributionAboveMaximum={
              allowExpectedContributionAboveMaximum
            }
            setAllowExpectedContributionAboveMaximum={
              setAllowExpectedContributionAboveMaximum
            }
          />
        </Stack>
      </ContentSurface>
    </FormPage>
  );
};

export default CreateFundForm;
