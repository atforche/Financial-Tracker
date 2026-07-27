"use client";

import { Button, type ButtonProps, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState, useEffect } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { AccountingPeriodServerAction } from "@/accounting-periods/workspace/accountingPeriodAction";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import { useRouter } from "next/navigation";

/**
 * Props for the AccountingPeriodConfirmationForm component.
 */
interface AccountingPeriodConfirmationFormProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly open: boolean;
  readonly onClose: () => void;
  readonly redirectUrl: string;
  readonly action: AccountingPeriodServerAction;
  readonly actionLabel: string;
  readonly actionVerb: string;
  readonly color?: ButtonProps["color"];
}

/**
 * Displays a confirmation form for an action on an accounting period.
 */
const AccountingPeriodConfirmationForm = function ({
  accountingPeriod,
  open,
  onClose,
  redirectUrl,
  action: serverAction,
  actionLabel,
  actionVerb,
  color = "primary",
}: AccountingPeriodConfirmationFormProps): JSX.Element {
  const [state, action, pending] = useActionState(serverAction, {});
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
      title={`${actionLabel} Accounting Period`}
      actions={
        <>
          <Button disabled={pending} onClick={onClose}>
            Cancel
          </Button>
          <Button
            color={color}
            variant="contained"
            loading={pending}
            onClick={() => {
              startTransition(() => {
                action({
                  accountingPeriodId: accountingPeriod.id,
                  redirectUrl,
                });
              });
            }}
          >
            {actionLabel}
          </Button>
        </>
      }
    >
      <Stack spacing={2}>
        <Typography>
          Are you sure you want to {actionVerb} the accounting period &quot;
          {accountingPeriod.name}&quot;?
        </Typography>
        <ErrorAlert
          errorMessage={state.errorTitle ?? null}
          unmappedErrors={state.unmappedErrors ?? null}
        />
      </Stack>
    </Dialog>
  );
};

export default AccountingPeriodConfirmationForm;
