"use client";

import type { AccountType, OnboardAccountRequest } from "@/accounts/types";
import { Button, Stack, Typography } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useRef,
  useState,
} from "react";
import AccountDetailsFrame from "@/accounts/workspace/AccountDetailsFrame";
import AccountStartingBalanceFrame from "@/accounts/workspace/AccountStartingBalanceFrame";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import { buildOnboardRequest } from "@/accounts/workspace/helpers";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import onboardAccount from "@/accounts/workspace/onboardAccount";

/**
 * Props for the OnboardAccountForm component.
 */
interface OnboardAccountFormProps {
  readonly redirectUrl: string;
}

/**
 * Displays the inline onboarding form for the workspace.
 */
const OnboardAccountForm = function ({
  redirectUrl,
}: OnboardAccountFormProps): JSX.Element {
  const [name, setName] = useState<string>("");
  const [accountType, setAccountType] = useState<AccountType | null>(null);
  const [onboardedBalance, setOnboardedBalance] = useState<number | null>(null);
  const formRef = useRef<HTMLDivElement | null>(null);
  const [state, action, pending] = useActionState(onboardAccount, {});
  const detailsAreValid = name !== "" && accountType !== null;
  const balanceIsValid = onboardedBalance !== null;

  const reset = function (): void {
    setName("");
    setAccountType(null);
    setOnboardedBalance(null);
    focusFirstEntryControl(formRef.current);
  };

  const request: OnboardAccountRequest | null = buildOnboardRequest(
    name,
    accountType,
    onboardedBalance,
  );

  useEffect(() => {
    if (state.success === true) {
      reset();
    }
  }, [state]);

  return (
    <Stack ref={formRef} spacing={3} sx={{ width: "100%", maxWidth: 1200 }}>
      <Typography variant="h5">Onboard Account</Typography>
      <AccountDetailsFrame
        color={detailsAreValid ? "info" : "error"}
        name={name}
        setName={setName}
        nameErrorMessage={state.nameErrors ?? null}
        accountType={accountType}
        setAccountType={setAccountType}
        accountTypeErrorMessage={state.typeErrors ?? null}
      />
      <AccountStartingBalanceFrame
        value={onboardedBalance}
        setValue={setOnboardedBalance}
        errorMessage={state.onboardedBalanceErrors ?? null}
        color={balanceIsValid ? "info" : "error"}
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
          Onboard account
        </Button>
      </Stack>
    </Stack>
  );
};

export default OnboardAccountForm;
