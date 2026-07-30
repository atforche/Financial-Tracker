"use client";

import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodConfirmationForm from "@/accounting-periods/workspace/AccountingPeriodConfirmationForm";
import type { JSX } from "react";
import deleteAccountingPeriod from "@/accounting-periods/workspace/deleteAccountingPeriod";

/**
 * Props for the DeleteAccountingPeriodForm component.
 */
interface DeleteAccountingPeriodFormProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly open: boolean;
  readonly onClose: () => void;
  readonly redirectUrl: string;
}

/**
 * Component that displays the form for deleting an accounting period.
 */
const DeleteAccountingPeriodForm = function ({
  accountingPeriod,
  open,
  onClose,
  redirectUrl,
}: DeleteAccountingPeriodFormProps): JSX.Element {
  return (
    <AccountingPeriodConfirmationForm
      accountingPeriod={accountingPeriod}
      open={open}
      onClose={onClose}
      redirectUrl={redirectUrl}
      action={deleteAccountingPeriod}
      actionLabel="Delete"
      actionVerb="delete"
      color="error"
    />
  );
};

export default DeleteAccountingPeriodForm;
