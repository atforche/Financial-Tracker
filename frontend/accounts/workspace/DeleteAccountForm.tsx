"use client";

import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
  Typography,
} from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useState,
} from "react";
import type { Account } from "@/accounts/types";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import deleteAccount from "@/accounts/workspace/deleteAccount";
import { useRouter } from "next/navigation";

/**
 * Props for the DeleteAccountForm component.
 */
interface DeleteAccountFormProps {
  readonly account: Account;
  readonly redirectUrl: string;
}

/**
 * Displays the action for deleting the selected account.
 */
const DeleteAccountForm = function ({
  account,
  redirectUrl,
}: DeleteAccountFormProps): JSX.Element {
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const [state, action, pending] = useActionState(deleteAccount, {});

  useEffect(() => {
    if (state.success === true) {
      router.replace(redirectUrl, { scroll: false });
    }
  }, [redirectUrl, router, state.success]);

  return (
    <>
      <Button
        color="error"
        variant="outlined"
        onClick={() => {
          setOpen(true);
        }}
      >
        Delete
      </Button>
      <Dialog
        open={open}
        onClose={
          pending
            ? // eslint-disable-next-line no-undefined
              undefined
            : (): void => {
                setOpen(false);
              }
        }
        fullWidth
        maxWidth="sm"
      >
        <DialogTitle>Delete Account</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            <Typography>
              Are you sure you want to delete the account &quot;{account.name}
              &quot;?
            </Typography>
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
              setOpen(false);
            }}
          >
            Cancel
          </Button>
          <Button
            color="error"
            variant="contained"
            loading={pending}
            onClick={() => {
              startTransition(() => {
                action({
                  accountId: account.id,
                  redirectUrl,
                });
              });
            }}
          >
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
};

export default DeleteAccountForm;
