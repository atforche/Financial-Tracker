"use client";

import { Button, Stack, Typography } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useRef,
  useState,
} from "react";
import {
  buildOnboardFundRequest,
  validateOnboardFundSetup,
} from "@/funds/workspace/helpers";
import {
  compareCurrencyAmounts,
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Frame from "@/framework/view/Frame";
import FundGoalSetupSection from "@/funds/workspace/FundGoalSetupSection";
import StringEntryField from "@/framework/forms/StringEntryField";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import onboardFund from "@/funds/workspace/onboardFund";
import useFundSetupState from "@/funds/workspace/useFundSetupState";
import { useRouter } from "next/navigation";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the OnboardFundForm component.
 */
interface OnboardFundFormProps {
  readonly redirectUrl: string;
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
    regularContribution,
    setRegularContribution,
    minimumFundedBalance,
    setMinimumFundedBalance,
    maximumFundedBalance,
    setMaximumFundedBalance,
    targetEndingBalance,
    setTargetEndingBalance,
  } = fundSetup;
  const formRef = useRef<HTMLDivElement | null>(null);
  const [onboardedBalance, setOnboardedBalance] = useState<number | null>(null);
  const [state, action, pending] = useActionState(onboardFund, {});

  const reset = function (): void {
    fundSetup.reset();
    setOnboardedBalance(null);
    focusFirstEntryControl(formRef.current);
  };

  useEffect(() => {
    if (state.success === true) {
      reset();
      router.replace(redirectUrl, { scroll: false });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [redirectUrl, router, state.success]);

  const remainingUnassignedAmount =
    unassignedBalance === null
      ? null
      : getCurrencyDifference(unassignedBalance, onboardedBalance ?? 0);

  const fundSetupIsComplete = validateOnboardFundSetup(name, onboardedBalance);
  const request = buildOnboardFundRequest({
    name,
    description,
    onboardedBalance,
    regularContribution,
    minimumFundedBalance,
    maximumFundedBalance,
    targetEndingBalance,
  });

  if (!canWrite) {
    return null;
  }

  return (
    <ConstrainedContent maxWidth={780}>
      <Stack ref={formRef} spacing={3}>
        <Frame
          title="Fund Setup"
          color={fundSetupIsComplete ? "success" : "error"}
        >
          <Stack spacing={2.5}>
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
            {remainingUnassignedAmount !== null ? (
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
            ) : null}
          </Stack>
        </Frame>

        <FundGoalSetupSection
          color="success"
          regularContribution={regularContribution}
          setRegularContribution={setRegularContribution}
          minimumFundedBalance={minimumFundedBalance}
          setMinimumFundedBalance={setMinimumFundedBalance}
          maximumFundedBalance={maximumFundedBalance}
          setMaximumFundedBalance={setMaximumFundedBalance}
          targetEndingBalance={targetEndingBalance}
          setTargetEndingBalance={setTargetEndingBalance}
        />

        <ErrorAlert
          errorMessage={state.errorTitle ?? null}
          unmappedErrors={state.unmappedErrors ?? null}
        />

        <Stack
          direction={{ xs: "column", sm: "row" }}
          spacing={1.5}
          justifyContent="flex-end"
        >
          <Button variant="outlined" onClick={reset}>
            Reset
          </Button>
          <Button
            variant="contained"
            loading={pending}
            disabled={request === null}
            onClick={() => {
              if (request === null) {
                return;
              }
              startTransition(() => {
                action({
                  redirectUrl,
                  request,
                });
              });
            }}
          >
            Onboard Fund
          </Button>
        </Stack>
      </Stack>
    </ConstrainedContent>
  );
};

export default OnboardFundForm;
