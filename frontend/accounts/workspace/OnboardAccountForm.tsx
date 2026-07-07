"use client";

import type { AccountType, OnboardAccountRequest } from "@/accounts/types";
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
} from "@mui/material";
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
    >
      <DialogTitle>Onboard Account</DialogTitle>
      <DialogContent>
        <Stack ref={formRef} spacing={3} sx={{ pt: 1 }}>
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
        </Stack>
      </DialogContent>
      <DialogActions>
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
      </DialogActions>
    </Dialog>
  );
};

export default OnboardAccountForm;
