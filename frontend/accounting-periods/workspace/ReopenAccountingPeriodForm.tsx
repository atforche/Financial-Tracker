"use client";

import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodConfirmationForm from "@/accounting-periods/workspace/AccountingPeriodConfirmationForm";
import type { JSX } from "react";
import reopenAccountingPeriod from "@/accounting-periods/workspace/reopenAccountingPeriod";

/**
 * Props for the ReopenAccountingPeriodForm component.
 */
interface ReopenAccountingPeriodFormProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly open: boolean;
  readonly onClose: () => void;
  readonly redirectUrl: string;
}

/**
 * Component that displays the form for reopening an accounting period.
 */
const ReopenAccountingPeriodForm = function ({
  accountingPeriod,
  open,
  onClose,
  redirectUrl,
}: ReopenAccountingPeriodFormProps): JSX.Element {
  return (
    <AccountingPeriodConfirmationForm
      accountingPeriod={accountingPeriod}
      open={open}
      onClose={onClose}
      redirectUrl={redirectUrl}
      action={reopenAccountingPeriod}
      actionLabel="Reopen"
      actionVerb="reopen"
    />
  );
};

export default ReopenAccountingPeriodForm;
