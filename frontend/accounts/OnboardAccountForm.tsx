"use client";

import type { AccountType, OnboardAccountRequest } from "@/accounts/types";
import { Button, DialogActions, Stack } from "@mui/material";
import { type JSX, startTransition, useActionState, useState } from "react";
import AccountTypeEntryField from "@/accounts/AccountTypeEntryField";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Link from "next/link";
import StringEntryField from "@/framework/forms/StringEntryField";
import breadcrumbs from "@/accounts/breadcrumbs";
import onboardAccount from "@/accounts/onboardAccount";
import routes from "@/accounts/routes";

/**
 * Component that displays the form for onboarding an account.
 */
const OnboardAccountForm = function (): JSX.Element {
  const [name, setName] = useState<string>("");
  const [accountType, setAccountType] = useState<AccountType | null>(null);
  const [onboardedBalance, setOnboardedBalance] = useState<number | null>(null);
  const [state, action, pending] = useActionState(onboardAccount, {});

  let request: OnboardAccountRequest | null = null;
  if (name !== "" && accountType !== null && onboardedBalance !== null) {
    request = {
      name,
      type: accountType,
      onboardedBalance,
    };
  }

  return (
    <Stack spacing={2}>
      <Breadcrumbs breadcrumbs={breadcrumbs.onboard()} />
      <Stack spacing={2} sx={{ maxWidth: "500px" }}>
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
        <DialogActions>
          <Link href={routes.index({})} tabIndex={-1}>
            <Button variant="outlined">Cancel</Button>
          </Link>
          <Button
            variant="contained"
            loading={pending}
            disabled={request === null}
            onClick={() => {
              if (request === null) {
                return;
              }
              startTransition(() => {
                action(request);
              });
            }}
          >
            Onboard
          </Button>
        </DialogActions>
        <ErrorAlert
          errorMessage={state.errorTitle ?? null}
          unmappedErrors={state.unmappedErrors ?? null}
        />
      </Stack>
    </Stack>
  );
};

export default OnboardAccountForm;
