"use client";

import { Button, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState, useEffect } from "react";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import deleteExpectedIncomeSource from "@/accounting-periods/workspace/deleteExpectedIncomeSource";
import { useRouter } from "next/navigation";

/**
 * Props for the ExpectedIncomeSourceDeleteDialog component.
 */
interface ExpectedIncomeSourceDeleteDialogProps {
  readonly source: Readonly<{ id: string; name: string }>;
  readonly accountingPeriodId: string;
  readonly redirectUrl: string;
  readonly open: boolean;
  readonly onClose: () => void;
}

/**
 * Confirms and removes an expected-income source.
 */
const ExpectedIncomeSourceDeleteDialog = function ({
  source,
  accountingPeriodId,
  redirectUrl,
  open,
  onClose,
}: ExpectedIncomeSourceDeleteDialogProps): JSX.Element {
  const [state, action, pending] = useActionState(
    deleteExpectedIncomeSource,
    {},
  );
  const router = useRouter();

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
      maxWidth="sm"
      title="Delete Expected Income Source"
      actions={
        <>
          <Button disabled={pending} onClick={onClose}>
            Cancel
          </Button>
          <Button
            color="error"
            variant="contained"
            loading={pending}
            onClick={() => {
              startTransition(() => {
                action({
                  accountingPeriodId,
                  expectedIncomeSourceId: source.id,
                  redirectUrl,
                });
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
          Are you sure you want to delete &quot;{source.name}&quot;?
        </Typography>
        <ErrorAlert
          errorMessage={state.errorTitle ?? null}
          unmappedErrors={state.unmappedErrors ?? null}
        />
      </Stack>
    </Dialog>
  );
};

export default ExpectedIncomeSourceDeleteDialog;
