"use client";

import type { AccountType, OnboardAccountRequest } from "@/accounts/types";
import { Button, DialogActions, Stack } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useState,
} from "react";
import AccountTypeEntryField from "@/accounts/AccountTypeEntryField";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import StringEntryField from "@/framework/forms/StringEntryField";
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
  const [state, action, pending] = useActionState(onboardAccount, {});

  const reset = function (): void {
    setName("");
    setAccountType(null);
    setOnboardedBalance(null);
  };

  let request: OnboardAccountRequest | null = null;
  if (name !== "" && accountType !== null && onboardedBalance !== null) {
    request = {
      name,
      type: accountType,
      onboardedBalance,
    };
  }

  useEffect(() => {
    if (state.success === true) {
      reset();
    }
  }, [state]);

  return (
    <Stack spacing={3}>
      <Stack spacing={2.5} sx={{ maxWidth: 520 }}>
        <StringEntryField
          label="Name"
          value={name}
          setValue={setName}
          errorMessage={state.nameErrors ?? null}
        />
        <AccountTypeEntryField
          label="Type"
          value={accountType}
          setValue={setAccountType}
          errorMessage={state.typeErrors ?? null}
        />
        <CurrencyEntryField
          label="Starting Balance"
          value={onboardedBalance}
          setValue={setOnboardedBalance}
          errorMessage={state.onboardedBalanceErrors ?? null}
        />
      </Stack>
      <ErrorAlert
        errorMessage={state.errorTitle ?? null}
        unmappedErrors={state.unmappedErrors ?? null}
      />
      <DialogActions sx={{ px: 0, pb: 0 }}>
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
      </DialogActions>
    </Stack>
  );
};

export default OnboardAccountForm;
