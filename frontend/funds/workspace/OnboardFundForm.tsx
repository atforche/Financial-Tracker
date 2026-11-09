"use client";

import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useEffectEvent,
  useState,
} from "react";
import { Stack, Typography } from "@mui/material";
import {
  compareCurrencyAmounts,
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import ContentSurface from "@/framework/view/ContentSurface";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import FormPage from "@/framework/view/FormPage";
import FundGoalSetupSection from "@/funds/workspace/FundGoalSetupSection";
import type { Route } from "next";
import StringEntryField from "@/framework/forms/StringEntryField";
import { buildOnboardFundRequest } from "@/funds/workspace/helpers";
import onboardFund from "@/funds/workspace/onboardFund";
import useFundSetupState from "@/funds/workspace/useFundSetupState";
import { useRouter } from "next/navigation";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the OnboardFundForm component.
 */
interface OnboardFundFormProps {
  readonly redirectUrl: Route;
  readonly unassignedBalance: number | null;
}

/**
 * Component that displays the form for onboarding a fund.
 */
const OnboardFundForm = function ({
  redirectUrl,
  unassignedBalance,
}: OnboardFundFormProps): JSX.Element | null {
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
  const [onboardedBalance, setOnboardedBalance] = useState<number | null>(null);
  const [state, action, pending] = useActionState(onboardFund, {});

  const handleSuccess = useEffectEvent((): void => {
    router.replace(redirectUrl);
  });
  useEffect(() => {
    if (state.success === true) {
      handleSuccess();
    }
  }, [state.success]);

  const remainingUnassignedAmount =
    unassignedBalance === null
      ? null
      : getCurrencyDifference(unassignedBalance, onboardedBalance ?? 0);

  const request = buildOnboardFundRequest({
    name,
    description,
    onboardedBalance,
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
      title="Onboard Fund"
      returnHref={redirectUrl}
      returnLabel="Back to Funds"
      submitLabel="Onboard Fund"
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
          <CurrencyEntryField
            label="Starting Balance"
            value={onboardedBalance}
            setValue={setOnboardedBalance}
            errorMessage={state.onboardedBalanceErrors ?? null}
          />
          {remainingUnassignedAmount === null ? null : (
            <Typography
              variant="body2"
              sx={{
                color:
                  compareCurrencyAmounts(remainingUnassignedAmount, 0) < 0
                    ? "error.main"
                    : "text.secondary",
              }}
            >
              Remaining Unassigned Balance:{" "}
              {formatCurrency(remainingUnassignedAmount)}
            </Typography>
          )}
        </Stack>
      </ContentSurface>
      <ContentSurface>
        <Stack spacing={2}>
          <Typography variant="h5">Goal</Typography>
          <FundGoalSetupSection
            accountingPeriod={null}
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

export default OnboardFundForm;
