"use client";

import { Button, DialogActions, Stack } from "@mui/material";
import { type JSX, startTransition, useActionState, useState } from "react";
import type { Account } from "@/accounts/types";
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

  const [state, action, pending] = useActionState(updateAccount, {
    accountId: account.id,
    redirectUrl,
  });

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
        <Button
          variant="outlined"
          onClick={() => {
            setName(account.name);
          }}
        >
          Reset
        </Button>
        <Button
          variant="contained"
          loading={pending}
          disabled={name === ""}
          onClick={() => {
            if (name === "") {
              return;
            }
            startTransition(() => {
              action({ name });
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
