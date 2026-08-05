"use client";

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
  validateCreateFundSetup,
} from "@/funds/workspace/helpers";
import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodEntryField from "@/accounting-periods/AccountingPeriodEntryField";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Frame from "@/framework/view/Frame";
import FundGoalSetupSection from "@/funds/workspace/FundGoalSetupSection";
import StringEntryField from "@/framework/forms/StringEntryField";
import createFund from "@/funds/workspace/createFund";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import useFundSetupState from "@/funds/workspace/useFundSetupState";
import { useRouter } from "next/navigation";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

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
}: CreateFundFormProps): JSX.Element | null {
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
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(null);

  const [state, action, pending] = useActionState(createFund, {});

  const reset = function (): void {
    fundSetup.reset();
    setAccountingPeriod(null);
    focusFirstEntryControl(formRef.current);
  };

  useEffect(() => {
    if (state.success === true) {
      reset();
      router.replace(redirectUrl, { scroll: false });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [redirectUrl, router, state.success]);

  const fundSetupIsComplete = validateCreateFundSetup(name, accountingPeriod);
  const request = buildCreateFundRequest({
    name,
    description,
    accountingPeriod,
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
          color={fundSetupIsComplete ? "info" : "error"}
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
            <AccountingPeriodEntryField
              label="Accounting Period"
              options={accountingPeriods}
              value={accountingPeriod}
              setValue={setAccountingPeriod}
              errorMessage={state.accountingPeriodErrors ?? null}
            />
          </Stack>
        </Frame>

        <FundGoalSetupSection
          color="info"
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
            Create Fund
          </Button>
        </Stack>
      </Stack>
    </ConstrainedContent>
  );
};

export default CreateFundForm;
