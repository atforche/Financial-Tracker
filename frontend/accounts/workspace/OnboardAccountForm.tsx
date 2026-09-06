"use client";

import type { AccountType, OnboardAccountRequest } from "@/accounts/types";
import { Button, Divider, Stack } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useState,
} from "react";
import AccountTypeEntryField from "@/accounts/AccountTypeEntryField";
import CreatableComboBoxEntryField from "@/framework/forms/CreatableComboBoxEntryField";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import StringEntryField from "@/framework/forms/StringEntryField";
import { buildOnboardRequest } from "@/accounts/workspace/helpers";
import onboardAccount from "@/accounts/workspace/onboardAccount";
import { useRouter } from "next/navigation";

/**
 * Props for the OnboardAccountForm component.
 */
interface OnboardAccountFormProps {
  readonly financialInstitutions: readonly string[];
  readonly onClose: () => void;
  readonly redirectUrl: string;
}

/**
 * Displays the account onboarding dialog for the workspace.
 */
const OnboardAccountForm = function ({
  financialInstitutions,
  onClose,
  redirectUrl,
}: OnboardAccountFormProps): JSX.Element {
  const router = useRouter();
  const [name, setName] = useState<string>("");
  const [financialInstitution, setFinancialInstitution] = useState<
    string | null
  >(null);
  const [accountType, setAccountType] = useState<AccountType | null>(null);
  const [onboardedBalance, setOnboardedBalance] = useState<number | null>(null);
  const [state, action, pending] = useActionState(onboardAccount, {});
  const request: OnboardAccountRequest | null = buildOnboardRequest(
    name,
    financialInstitution,
    accountType,
    onboardedBalance,
  );

  useEffect(() => {
    if (state.success === true) {
      onClose();
      router.replace(redirectUrl, { scroll: false });
    }
  }, [onClose, redirectUrl, router, state.success]);

  return (
    <Dialog
      open
      onClose={pending ? undefined : onClose}
      fullWidth
      maxWidth="md"
      title="Onboard Account"
      actions={
        <>
          <Button disabled={pending} onClick={onClose}>
            Cancel
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
            Onboard Account
          </Button>
        </>
      }
    >
      <Stack spacing={3}>
        <StringEntryField
          label="Name"
          value={name}
          setValue={setName}
          errorMessage={state.nameErrors ?? null}
        />
        <CreatableComboBoxEntryField
          options={financialInstitutions}
          value={financialInstitution}
          setValue={setFinancialInstitution}
        />
        <AccountTypeEntryField
          value={accountType}
          setValue={setAccountType}
          errorMessage={state.typeErrors ?? null}
        />
        <Divider />
        <CurrencyEntryField
          label="Starting Balance"
          value={onboardedBalance}
          setValue={setOnboardedBalance}
          errorMessage={state.onboardedBalanceErrors ?? null}
        />
        <ErrorAlert
          errorMessage={state.errorTitle ?? null}
          unmappedErrors={state.unmappedErrors ?? null}
        />
      </Stack>
    </Dialog>
  );
};

export default OnboardAccountForm;
