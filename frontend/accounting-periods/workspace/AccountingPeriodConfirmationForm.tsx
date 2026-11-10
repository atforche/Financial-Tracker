"use client";

import { type ButtonProps, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState, useEffect } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { AccountingPeriodServerAction } from "@/accounting-periods/workspace/accountingPeriodAction";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import FormDialog from "@/framework/dialog/FormDialog";
import { useRouter } from "next/navigation";

/**
 * Props for the AccountingPeriodConfirmationForm component.
 */
interface AccountingPeriodConfirmationFormProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly onClose: () => void;
  readonly redirectUrl: string;
  readonly action: AccountingPeriodServerAction;
  readonly actionLabel: string;
  readonly actionVerb: string;
  readonly color?: ButtonProps["color"];
  readonly preserveScroll?: boolean;
}

/**
 * Displays a confirmation form for an action on an accounting period.
 */
const AccountingPeriodConfirmationForm = function ({
  accountingPeriod,
  onClose,
  redirectUrl,
  action: serverAction,
  actionLabel,
  actionVerb,
  color = "primary",
  preserveScroll = true,
}: AccountingPeriodConfirmationFormProps): JSX.Element {
  const [state, action, pending] = useActionState(serverAction, {});
  const router = useRouter();

  useEffect(() => {
    if (state.success === true) {
      onClose();
      router.replace(redirectUrl, { scroll: !preserveScroll });
    }
  }, [onClose, preserveScroll, redirectUrl, router, state.success]);

  return (
    <FormDialog
      open
      title={`${actionLabel} Accounting Period`}
      onClose={onClose}
      pending={pending}
      submitLabel={actionLabel}
      onSubmit={() => {
        startTransition(() => {
          action({
            accountingPeriodId: accountingPeriod.id,
            redirectUrl,
          });
        });
      }}
      submitButtonProps={{ color }}
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
    </FormDialog>
  );
};

export default AccountingPeriodConfirmationForm;
