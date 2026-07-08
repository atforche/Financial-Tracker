"use client";

import type { Account, UpdateAccountRequest } from "@/accounts/types";
import { Button, Stack } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useRef,
  useState,
} from "react";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import StringEntryField from "@/framework/forms/StringEntryField";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import updateAccount from "@/accounts/workspace/updateAccount";
import { useRouter } from "next/navigation";

/**
 * Props for the UpdateAccountForm component.
 */
interface UpdateAccountFormProps {
  readonly account: Account;
  readonly redirectUrl: string;
}

/**
 * Displays the action for updating the selected account.
 */
const UpdateAccountForm = function ({
  account,
  redirectUrl,
}: UpdateAccountFormProps): JSX.Element {
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const [accountId, setAccountId] = useState<string>(account.id);
  const [name, setName] = useState<string>(account.name);
  const formRef = useRef<HTMLDivElement | null>(null);
  if (accountId !== account.id) {
    setAccountId(account.id);
    setName(account.name);
  }

  const [state, action, pending] = useActionState(updateAccount, {});

  const reset = function (): void {
    setName(account.name);
    focusFirstEntryControl(formRef.current);
  };

  useEffect(() => {
    if (state.success === true) {
      setOpen(false);
      router.replace(redirectUrl, { scroll: false });
    }
  }, [redirectUrl, router, state.success]);

  let request: UpdateAccountRequest | null = null;
  if (name !== "") {
    request = {
      name,
    };
  }

  return (
    <>
      <Button
        variant="contained"
        onClick={() => {
          setOpen(true);
        }}
      >
        Edit
      </Button>
      <Dialog
        open={open}
        onClose={
          pending
            ? // eslint-disable-next-line no-undefined
              undefined
            : (): void => {
                setOpen(false);
                reset();
              }
        }
        fullWidth
        maxWidth="sm"
        title="Update Account"
        actions={
          <>
            <Button
              disabled={pending}
              onClick={() => {
                setOpen(false);
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
                  action({ accountId: account.id, redirectUrl, request });
                });
              }}
            >
              Save
            </Button>
          </>
        }
      >
        <Stack ref={formRef} spacing={3}>
          <StringEntryField
            label="Name"
            value={name}
            setValue={setName}
            errorMessage={state.nameErrors ?? null}
          />
          <ErrorAlert
            errorMessage={state.errorTitle ?? null}
            unmappedErrors={state.unmappedErrors ?? null}
          />
        </Stack>
      </Dialog>
    </>
  );
};

export default UpdateAccountForm;
