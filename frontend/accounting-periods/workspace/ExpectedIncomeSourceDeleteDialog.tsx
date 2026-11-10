"use client";

import { type JSX, startTransition, useActionState, useEffect } from "react";
import { Stack, Typography } from "@mui/material";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import FormDialog from "@/framework/dialog/FormDialog";
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
      router.replace(redirectUrl);
    }
  }, [onClose, redirectUrl, router, state.success]);

  return (
    <FormDialog
      open={open}
      title="Delete Expected Income Source"
      onClose={onClose}
      pending={pending}
      submitLabel="Delete"
      onSubmit={() => {
        startTransition(() => {
          action({
            accountingPeriodId,
            expectedIncomeSourceId: source.id,
            redirectUrl,
          });
        });
      }}
      submitButtonProps={{ color: "error" }}
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
    </FormDialog>
  );
};

export default ExpectedIncomeSourceDeleteDialog;
