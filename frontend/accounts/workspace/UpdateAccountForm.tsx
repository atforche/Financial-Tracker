"use client";

import type { Account, UpdateAccountRequest } from "@/accounts/types";
import { Button, DialogActions, Stack } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useState,
} from "react";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import StringEntryField from "@/framework/forms/StringEntryField";
import updateAccount from "@/accounts/workspace/updateAccount";

/**
 * Props for the UpdateAccountForm component.
 */
interface UpdateAccountFormProps {
  readonly account: Account;
  readonly redirectUrl: string;
}

/**
 * Displays the inline update form for the selected account.
 */
const UpdateAccountForm = function ({
  account,
  redirectUrl,
}: UpdateAccountFormProps): JSX.Element {
  const [accountId, setAccountId] = useState<string>(account.id);
  const [name, setName] = useState<string>(account.name);
  if (accountId !== account.id) {
    setAccountId(account.id);
    setName(account.name);
  }

  const [state, action, pending] = useActionState(updateAccount, {});

  const reset = function (): void {
    setName(account.name);
  };

  useEffect(() => {
    if (state.success === true) {
      reset();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [state]);

  let request: UpdateAccountRequest | null = null;
  if (name !== "") {
    request = {
      name,
    };
  }

  return (
    <Stack spacing={3}>
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
              action({ accountId: account.id, redirectUrl, request });
            });
          }}
        >
          Update account
        </Button>
      </DialogActions>
    </Stack>
  );
};

export default UpdateAccountForm;
