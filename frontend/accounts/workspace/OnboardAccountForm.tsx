"use client";

import type { AccountType, OnboardAccountRequest } from "@/accounts/types";
import { Button, Stack } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useState,
} from "react";
import AccountDetailsFrame from "@/accounts/workspace/AccountDetailsFrame";
import AccountStartingBalanceFrame from "@/accounts/workspace/AccountStartingBalanceFrame";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import { buildOnboardRequest } from "@/accounts/workspace/helpers";
import onboardAccount from "@/accounts/workspace/onboardAccount";
import { useRouter } from "next/navigation";

/**
 * Props for the OnboardAccountForm component.
 */
interface OnboardAccountFormProps {
  readonly open: boolean;
  readonly onClose: () => void;
  readonly redirectUrl: string;
}

/**
 * Displays the account onboarding dialog for the workspace.
 */
const OnboardAccountForm = function ({
  open,
  onClose,
  redirectUrl,
}: OnboardAccountFormProps): JSX.Element {
  const router = useRouter();
  const [name, setName] = useState<string>("");
  const [accountType, setAccountType] = useState<AccountType | null>(null);
  const [onboardedBalance, setOnboardedBalance] = useState<number | null>(null);
  const [state, action, pending] = useActionState(onboardAccount, {});
  const request: OnboardAccountRequest | null = buildOnboardRequest(
    name,
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
      open={open}
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
        <AccountDetailsFrame
          color={name !== "" && accountType !== null ? "success" : "error"}
          name={name}
          setName={setName}
          nameErrorMessage={state.nameErrors ?? null}
          accountType={accountType}
          setAccountType={setAccountType}
          accountTypeErrorMessage={state.typeErrors ?? null}
        />
        <AccountStartingBalanceFrame
          color={request !== null ? "success" : "error"}
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
