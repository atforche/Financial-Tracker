"use client";

import { Button, Stack, Typography } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useState,
} from "react";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { Fund } from "@/funds/types";
import deleteFund from "@/funds/workspace/deleteFund";
import { useRouter } from "next/navigation";

interface DeleteFundFormProps {
  readonly fund: Fund;
  readonly redirectUrl: string;
}

/**
 * Displays the action for deleting the selected fund.
 */
const DeleteFundForm = function ({
  fund,
  redirectUrl,
}: DeleteFundFormProps): JSX.Element {
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const [state, action, pending] = useActionState(deleteFund, {});

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
        title="Delete Fund"
        actions={
          <>
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
                  action({ fundId: fund.id, redirectUrl });
                });
              }}
            >
              Delete
            </Button>
          </>
        }
      >
        <Stack spacing={2}>
          <Typography>
            Are you sure you want to delete the fund &quot;{fund.name}&quot;?
          </Typography>
          <ErrorAlert
            errorMessage={state.errorTitle ?? null}
            unmappedErrors={state.unmappedErrors ?? null}
          />
        </Stack>
      </Dialog>
    </>
  );
};

export default DeleteFundForm;
