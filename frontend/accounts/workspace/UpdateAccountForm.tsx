"use client";

import type { Account, UpdateAccountRequest } from "@/accounts/types";
import { Button, Stack } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useState,
} from "react";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import StringEntryField from "@/framework/forms/StringEntryField";
import updateAccount from "@/accounts/workspace/updateAccount";
import { useRouter } from "next/navigation";

/**
 * Props for the UpdateAccountForm component.
 */
interface UpdateAccountFormProps {
  readonly account: Account;
  readonly redirectUrl: string;
  readonly onClose: () => void;
}

/**
 * Displays the action for updating the selected account.
 */
const UpdateAccountForm = function ({
  account,
  redirectUrl,
  onClose,
}: UpdateAccountFormProps): JSX.Element {
  const router = useRouter();
  const [name, setName] = useState<string>(account.name);
  const [state, action, pending] = useActionState(updateAccount, {});

  useEffect(() => {
    if (state.success === true) {
      onClose();
      router.replace(redirectUrl, { scroll: false });
    }
  }, [onClose, redirectUrl, router, state.success]);

  let request: UpdateAccountRequest | null = null;
  if (name !== "") {
    request = {
      name,
    };
  }

  return (
    <Dialog
      open
      onClose={
        pending
          ? undefined
          : (): void => {
              onClose();
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
              onClose();
            }}
          >
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
                action({ accountId: account.id, redirectUrl, request });
              });
            }}
          >
            Save
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
        <ErrorAlert
          errorMessage={state.errorTitle ?? null}
          unmappedErrors={state.unmappedErrors ?? null}
        />
      </Stack>
    </Dialog>
  );
};

export default UpdateAccountForm;
