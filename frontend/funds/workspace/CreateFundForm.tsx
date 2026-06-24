"use client";

import { AssignmentGoalType, SpendingGoalType } from "@/goals/types";
import { Box, Button, Paper, Stack, Typography } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useRef,
  useState,
} from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodEntryField from "@/accounting-periods/AccountingPeriodEntryField";
import AssignmentGoalSetupSection from "@/funds/workspace/AssignmentGoalSetupSection";
import type { CreateFundRequest } from "@/funds/types";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import SpendingGoalSetupSection from "@/funds/workspace/SpendingGoalSetupSection";
import StringEntryField from "@/framework/forms/StringEntryField";
import createFund from "@/funds/workspace/createFund";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";

/**
 * Props for the CreateFundForm component.
 */
interface CreateFundFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly redirectUrl: string;
}

/**
 * Component that displays the form for creating a fund.
 */
const CreateFundForm = function ({
  accountingPeriods,
  redirectUrl,
}: CreateFundFormProps): JSX.Element {
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
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(null);

  const [state, action, pending] = useActionState(createFund, {});

  const reset = function (): void {
    setName("");
    setDescription("");
    setAccountingPeriod(null);
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

  let request: CreateFundRequest | null = null;
  if (
    name !== "" &&
    accountingPeriod !== null &&
    assignmentGoalType !== null &&
    assignmentGoalAmount !== null &&
    spendingGoalType !== null
  ) {
    request = {
      name,
      description,
      accountingPeriodId: accountingPeriod.id,
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
            "linear-gradient(135deg, rgba(8,145,178,0.16) 0%, rgba(14,116,144,0.06) 100%)",
          border: "1px solid",
          borderColor: "divider",
        }}
      >
        <Stack spacing={0.75}>
          <Typography variant="h5">Create Fund</Typography>
          <Typography variant="body2" color="text.secondary">
            Add the fund details, choose the accounting period, and define how
            this fund should be guided by its assignment and spending goals.
          </Typography>
        </Stack>
      </Box>

      <Paper variant="outlined" sx={{ p: { xs: 2.5, sm: 3 }, borderRadius: 4 }}>
        <Stack spacing={2.5}>
          <Stack spacing={0.5}>
            <Typography variant="h6">Fund Details</Typography>
            <Typography variant="body2" color="text.secondary">
              Start with the basics for where this fund belongs.
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
          <AccountingPeriodEntryField
            label="Accounting Period"
            options={accountingPeriods}
            value={accountingPeriod}
            setValue={setAccountingPeriod}
            errorMessage={state.accountingPeriodErrors ?? null}
          />
        </Stack>
      </Paper>

      <AssignmentGoalSetupSection
        value={assignmentGoalType}
        setValue={setAssignmentGoalType}
        amount={assignmentGoalAmount}
        setAmount={setAssignmentGoalAmount}
        typeErrorMessage={state.assignmentGoalTypeErrors ?? null}
        amountErrorMessage={state.assignmentGoalAmountErrors ?? null}
      />

      <SpendingGoalSetupSection
        value={spendingGoalType}
        setValue={setSpendingGoalType}
        typeErrorMessage={state.spendingGoalTypeErrors ?? null}
      />

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
            Create Fund
          </Button>
        </Stack>
      </Paper>
    </Stack>
  );
};

export default CreateFundForm;
