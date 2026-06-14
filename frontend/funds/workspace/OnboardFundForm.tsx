"use client";

import {
  AssignmentGoalType,
  SpendingGoalType,
  formatAssignmentGoalType,
  formatSpendingGoalType,
} from "@/goals/types";
import { Box, Button, Paper, Stack, Typography } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useRef,
  useState,
} from "react";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import FundGoalTypeEntryField from "@/funds/FundGoalTypeEntryField";
import type { OnboardFundRequest } from "@/funds/types";
import StringEntryField from "@/framework/forms/StringEntryField";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import formatCurrency from "@/framework/formatCurrency";
import onboardFund from "@/funds/workspace/onboardFund";

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
    }
  }, [state]);

  const remainingUnassignedAmount =
    unassignedBalance === null
      ? null
      : unassignedBalance - (onboardedBalance ?? 0);

  let request: OnboardFundRequest | null = null;
  if (
    name !== "" &&
    onboardedBalance !== null &&
    assignmentGoalType !== null &&
    assignmentGoalAmount !== null &&
    spendingGoalType !== null
  ) {
    request = {
      name,
      description,
      onboardedBalance,
      assignmentGoalType,
      assignmentGoalAmount,
      spendingGoalType,
    };
  }

  return (
    <Stack ref={formRef} spacing={3} sx={{ width: "100%", maxWidth: "780px" }}>
      <Box
        sx={{
          px: { xs: 2.5, sm: 3 },
          py: { xs: 2.5, sm: 3 },
          borderRadius: 4,
          background:
            "linear-gradient(135deg, rgba(249,115,22,0.14) 0%, rgba(251,191,36,0.08) 100%)",
          border: "1px solid",
          borderColor: "divider",
        }}
      >
        <Stack spacing={0.75}>
          <Typography variant="h5">Onboard Fund</Typography>
          <Typography variant="body2" color="text.secondary">
            Set the starting balance and establish the goal rules this fund
            should follow from day one.
          </Typography>
        </Stack>
      </Box>

      <Paper variant="outlined" sx={{ p: { xs: 2.5, sm: 3 }, borderRadius: 4 }}>
        <Stack spacing={2.5}>
          <Stack spacing={0.5}>
            <Typography variant="h6">Fund Details</Typography>
            <Typography variant="body2" color="text.secondary">
              Capture the identity of the fund and the amount you want to seed
              it with.
            </Typography>
          </Stack>
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
      </Paper>

      <Paper variant="outlined" sx={{ p: { xs: 2.5, sm: 3 }, borderRadius: 4 }}>
        <Stack spacing={2.5}>
          <Stack spacing={0.5}>
            <Typography variant="h6">Goal Setup</Typography>
            <Typography variant="body2" color="text.secondary">
              Define how much this fund should receive and whether it behaves
              like a standard spending bucket or a debt payoff target.
            </Typography>
          </Stack>
          <FundGoalTypeEntryField
            label="Assignment Goal Type"
            options={[
              AssignmentGoalType.MonthlyTarget,
              AssignmentGoalType.RecurringContribution,
            ]}
            value={assignmentGoalType}
            setValue={setAssignmentGoalType}
            formatOptionLabel={formatAssignmentGoalType}
            errorMessage={state.assignmentGoalTypeErrors ?? null}
          />
          <CurrencyEntryField
            label="Assignment Goal Amount"
            value={assignmentGoalAmount}
            setValue={setAssignmentGoalAmount}
            errorMessage={state.assignmentGoalAmountErrors ?? null}
          />
          <FundGoalTypeEntryField
            label="Spending Goal Type"
            options={[SpendingGoalType.Standard, SpendingGoalType.Debt]}
            value={spendingGoalType}
            setValue={setSpendingGoalType}
            formatOptionLabel={formatSpendingGoalType}
            errorMessage={state.spendingGoalTypeErrors ?? null}
          />
        </Stack>
      </Paper>

      <ErrorAlert
        errorMessage={state.errorTitle ?? null}
        unmappedErrors={state.unmappedErrors ?? null}
      />

      <Paper variant="outlined" sx={{ p: 2, borderRadius: 4 }}>
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
      </Paper>
    </Stack>
  );
};

export default OnboardFundForm;
