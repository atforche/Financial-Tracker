"use client";

import type {
  AccountingPeriodWithBalance,
  ExpectedIncomeSource,
} from "@/accounting-periods/types";
import { Button, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState, useEffect } from "react";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import { toRequest } from "@/accounting-periods/workspace/ExpectedIncomeSourceForm";
import updateExpectedIncomeSources from "@/accounting-periods/workspace/updateExpectedIncomeSources";
import { useRouter } from "next/navigation";

/**
 * Props for the DeleteExpectedIncomeSourceDialog component.
 */
interface DeleteExpectedIncomeSourceDialogProps {
  readonly accountingPeriod: AccountingPeriodWithBalance;
  readonly source: ExpectedIncomeSource;
  readonly redirectUrl: string;
  readonly onClose: () => void;
}

/**
 * Confirms deletion of one expected-income source.
 */
const DeleteExpectedIncomeSourceDialog = function ({
  accountingPeriod,
  source,
  redirectUrl,
  onClose,
}: DeleteExpectedIncomeSourceDialogProps): JSX.Element {
  const router = useRouter();
  const [state, action, pending] = useActionState(
    updateExpectedIncomeSources,
    {},
  );
  useEffect(() => {
    if (state.success === true) {
      onClose();
      router.replace(redirectUrl, { scroll: false });
    }
  }, [onClose, redirectUrl, router, state.success]);
  return (
    <Dialog
      open
      onClose={pending ? undefined : onClose}
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
                  accountingPeriodId: accountingPeriod.id,
                  redirectUrl,
                  sources: accountingPeriod.expectedIncomeSources
                    .filter((item) => item.id !== source.id)
                    .map(toRequest),
                });
              });
            }}
          >
            Delete
          </Button>
        </>
      }
    >
      <Typography>
        Delete {source.name} from {accountingPeriod.name}?
      </Typography>
      <ErrorAlert
        errorMessage={state.errorTitle ?? null}
        unmappedErrors={state.unmappedErrors ?? null}
      />
    </Dialog>
  );
};

export default DeleteExpectedIncomeSourceDialog;
