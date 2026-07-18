"use client";

import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodConfirmationForm from "@/accounting-periods/workspace/AccountingPeriodConfirmationForm";
import type { JSX } from "react";
import closeAccountingPeriod from "@/accounting-periods/workspace/closeAccountingPeriod";

/**
 * Props for the CloseAccountingPeriodForm component.
 */
interface CloseAccountingPeriodFormProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly redirectUrl: string;
}

/**
 * Component that displays the form for closing an accounting period.
 */
const CloseAccountingPeriodForm = function ({
  accountingPeriod,
  redirectUrl,
}: CloseAccountingPeriodFormProps): JSX.Element {
  return (
    <AccountingPeriodConfirmationForm
      accountingPeriod={accountingPeriod}
      redirectUrl={redirectUrl}
      action={closeAccountingPeriod}
      actionLabel="Close"
      actionVerb="close"
    />
  );
};

export default CloseAccountingPeriodForm;
