"use client";

import { AssignmentGoalType, SpendingGoalType } from "@/goals/types";
import { Button, Stack } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useRef,
  useState,
} from "react";
import {
  buildCreateFundRequest,
  validateAssignmentGoalSetup,
  validateCreateFundSetup,
  validateSpendingGoalSetup,
} from "@/funds/workspace/helpers";
import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodEntryField from "@/accounting-periods/AccountingPeriodEntryField";
import AssignmentGoalSetupSection from "@/funds/workspace/AssignmentGoalSetupSection";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Frame from "@/framework/view/Frame";
import SpendingGoalSetupSection from "@/funds/workspace/SpendingGoalSetupSection";
import StringEntryField from "@/framework/forms/StringEntryField";
import createFund from "@/funds/workspace/createFund";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import { useRouter } from "next/navigation";

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
      router.replace(redirectUrl, { scroll: false });
    }
  }, [redirectUrl, router, state.success]);

  const fundSetupIsComplete = validateCreateFundSetup(name, accountingPeriod);
  const assignmentGoalSetupIsComplete = validateAssignmentGoalSetup(
    assignmentGoalType,
    assignmentGoalAmount,
  );
  const spendingGoalSetupIsComplete =
    validateSpendingGoalSetup(spendingGoalType);
  const request = buildCreateFundRequest(
    name,
    description,
    accountingPeriod,
    assignmentGoalType,
    assignmentGoalAmount,
    spendingGoalType,
  );

  return (
    <Stack ref={formRef} spacing={3} sx={{ width: "100%", maxWidth: "780px" }}>
      <Frame title="Fund Setup" color={fundSetupIsComplete ? "info" : "error"}>
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
          <AccountingPeriodEntryField
            label="Accounting Period"
            options={accountingPeriods}
            value={accountingPeriod}
            setValue={setAccountingPeriod}
            errorMessage={state.accountingPeriodErrors ?? null}
          />
        </Stack>
      </Frame>

      <AssignmentGoalSetupSection
        color={assignmentGoalSetupIsComplete ? "info" : "error"}
        value={assignmentGoalType}
        setValue={setAssignmentGoalType}
        amount={assignmentGoalAmount}
        setAmount={setAssignmentGoalAmount}
        typeErrorMessage={state.assignmentGoalTypeErrors ?? null}
        amountErrorMessage={state.assignmentGoalAmountErrors ?? null}
      />

      <SpendingGoalSetupSection
        color={spendingGoalSetupIsComplete ? "info" : "error"}
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
          Create Fund
        </Button>
      </Stack>
    </Stack>
  );
};

export default CreateFundForm;
