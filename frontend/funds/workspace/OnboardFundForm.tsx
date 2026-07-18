"use client";

import { AssignmentGoalType, SpendingGoalType } from "@/goals/types";
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
  validateAssignmentGoalSetup,
  validateOnboardFundSetup,
  validateSpendingGoalSetup,
} from "@/funds/workspace/helpers";
import AssignmentGoalSetupSection from "@/funds/workspace/AssignmentGoalSetupSection";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Frame from "@/framework/view/Frame";
import SpendingGoalSetupSection from "@/funds/workspace/SpendingGoalSetupSection";
import StringEntryField from "@/framework/forms/StringEntryField";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import { formatCurrency } from "@/framework/currencyHelpers";
import onboardFund from "@/funds/workspace/onboardFund";
import { useRouter } from "next/navigation";

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
}: OnboardFundFormProps): JSX.Element {
  const router = useRouter();
  const [name, setName] = useState<string>("");
  const [description, setDescription] = useState<string>("");
  const [assignmentGoalType, setAssignmentGoalType] =
    useState<AssignmentGoalType | null>(AssignmentGoalType.MonthlyTarget);
  const [assignmentGoalAmount, setAssignmentGoalAmount] = useState<
    number | null
  >(null);
  const [spendingGoalType, setSpendingGoalType] =
    useState<SpendingGoalType | null>(SpendingGoalType.Standard);
  const formRef = useRef<HTMLDivElement | null>(null);
  const [onboardedBalance, setOnboardedBalance] = useState<number | null>(null);
  const [state, action, pending] = useActionState(onboardFund, {});

  const reset = function (): void {
    setName("");
    setDescription("");
    setOnboardedBalance(null);
    setAssignmentGoalType(AssignmentGoalType.MonthlyTarget);
    setAssignmentGoalAmount(null);
    setSpendingGoalType(SpendingGoalType.Standard);
    focusFirstEntryControl(formRef.current);
  };

  useEffect(() => {
    if (state.success === true) {
      reset();
      router.replace(redirectUrl, { scroll: false });
    }
  }, [redirectUrl, router, state.success]);

  const remainingUnassignedAmount =
    unassignedBalance === null
      ? null
      : unassignedBalance - (onboardedBalance ?? 0);

  const fundSetupIsComplete = validateOnboardFundSetup(name, onboardedBalance);
  const assignmentGoalSetupIsComplete = validateAssignmentGoalSetup(
    assignmentGoalType,
    assignmentGoalAmount,
  );
  const spendingGoalSetupIsComplete =
    validateSpendingGoalSetup(spendingGoalType);
  const request = buildOnboardFundRequest(
    name,
    description,
    onboardedBalance,
    assignmentGoalType,
    assignmentGoalAmount,
    spendingGoalType,
  );

  return (
    <ConstrainedContent maxWidth={780}>
      <Stack ref={formRef} spacing={3}>
        <Frame
          title="Fund Setup"
          color={fundSetupIsComplete ? "warning" : "error"}
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
                    remainingUnassignedAmount < 0
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

        <AssignmentGoalSetupSection
          color={assignmentGoalSetupIsComplete ? "warning" : "error"}
          value={assignmentGoalType}
          setValue={setAssignmentGoalType}
          amount={assignmentGoalAmount}
          setAmount={setAssignmentGoalAmount}
          typeErrorMessage={state.assignmentGoalTypeErrors ?? null}
          amountErrorMessage={state.assignmentGoalAmountErrors ?? null}
        />

        <SpendingGoalSetupSection
          color={spendingGoalSetupIsComplete ? "warning" : "error"}
          value={spendingGoalType}
          setValue={setSpendingGoalType}
          typeErrorMessage={state.spendingGoalTypeErrors ?? null}
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
