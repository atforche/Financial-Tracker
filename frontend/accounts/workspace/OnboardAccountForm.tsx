"use client";

import type { AccountType, OnboardAccountRequest } from "@/accounts/types";
import { Box, Button, Stack } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useRef,
  useState,
} from "react";
import AccountTypeEntryField from "@/accounts/AccountTypeEntryField";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Frame from "@/framework/view/Frame";
import StringEntryField from "@/framework/forms/StringEntryField";
import { buildOnboardRequest } from "@/accounts/workspace/helpers";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
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
  const formRef = useRef<HTMLDivElement | null>(null);
  const [state, action, pending] = useActionState(onboardAccount, {});
  const setupIsValid =
    name !== "" && accountType !== null && onboardedBalance !== null;

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
      onClose();
      router.replace(redirectUrl, { scroll: false });
    }
  }, [onClose, redirectUrl, router, state.success]);

  return (
    <Dialog
      open={open}
      onClose={
        pending
          ? // eslint-disable-next-line no-undefined
            undefined
          : (): void => {
              onClose();
              reset();
            }
      }
      fullWidth
      maxWidth="md"
      title="Onboard Account"
      actions={
        <>
          <Button
            disabled={pending}
            onClick={() => {
              onClose();
              reset();
            }}
          >
            Cancel
          </Button>
          <Button variant="outlined" disabled={pending} onClick={reset}>
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
        </>
      }
    >
      <Stack ref={formRef} spacing={3}>
        <Frame title="Account Setup" color={setupIsValid ? "info" : "error"}>
          <Box
            sx={{
              display: "grid",
              gap: 2,
              gridTemplateColumns: "repeat(auto-fit, minmax(min(100%, 220px)))",
            }}
          >
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
          </Box>
        </Frame>
        <ErrorAlert
          errorMessage={state.errorTitle ?? null}
          unmappedErrors={state.unmappedErrors ?? null}
        />
      </Stack>
    </Dialog>
  );
};

export default OnboardAccountForm;
