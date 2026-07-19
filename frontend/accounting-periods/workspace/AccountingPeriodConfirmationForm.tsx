"use client";

import { Button, type ButtonProps } from "@mui/material";
import { type JSX, startTransition, useActionState } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { AccountingPeriodServerAction } from "@/accounting-periods/workspace/accountingPeriodAction";
import ConfirmActionDialog from "@/framework/dialog/ConfirmActionDialog";

/**
 * Props for the AccountingPeriodConfirmationForm component.
 */
interface AccountingPeriodConfirmationFormProps {
  readonly accountingPeriod: AccountingPeriod;
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
  redirectUrl,
  action: serverAction,
  actionLabel,
  actionVerb,
  color = "primary",
}: AccountingPeriodConfirmationFormProps): JSX.Element {
  const [state, action, pending] = useActionState(serverAction, {});

  return (
    <ConfirmActionDialog
      trigger={(openDialog) => (
        <Button color={color} variant="contained" onClick={openDialog}>
          {actionLabel}
        </Button>
      )}
      title={`${actionLabel} Accounting Period`}
      confirmationCopy={
        <>
          Are you sure you want to {actionVerb} the accounting period &quot;
          {accountingPeriod.name}&quot;?
        </>
      }
      confirmLabel={actionLabel}
      confirmButtonProps={{ color }}
      pending={pending}
      errorTitle={state.errorTitle}
      unmappedErrors={state.unmappedErrors}
      onConfirm={() => {
        startTransition(() => {
          action({ accountingPeriodId: accountingPeriod.id, redirectUrl });
        });
      }}
    />
  );
};

export default AccountingPeriodConfirmationForm;
