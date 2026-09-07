"use client";

import { usePathname, useSearchParams } from "next/navigation";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import type { AccountingPeriodWorkspaceAction } from "@/accounting-periods/workspace/helpers";
import CreateAccountingPeriodForm from "@/accounting-periods/workspace/CreateAccountingPeriodForm";
import type { JSX } from "react";
import { buildUrl } from "@/framework/routes/helpers";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the AccountingPeriodWorkspaceActions component.
 */
interface AccountingPeriodWorkspaceActionsProps {
  readonly isInOnboardingMode: boolean;
  readonly latestAccountingPeriod: AccountingPeriodWithBalance | null;
  readonly requestedAction: AccountingPeriodWorkspaceAction | null;
}

/**
 * Displays the available accounting period actions for the current workspace selection.
 */
const AccountingPeriodWorkspaceActions = function ({
  isInOnboardingMode,
  latestAccountingPeriod,
  requestedAction,
}: AccountingPeriodWorkspaceActionsProps): JSX.Element | null {
  const canWrite = useWriteAccess();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const updateParams = useSearchParamUpdater([]);
  const setAction = function (
    action: AccountingPeriodWorkspaceAction | null,
  ): void {
    updateParams((params) => {
      if (action === null) {
        params.delete("action");
      } else {
        params.set("action", action);
      }
    });
  };
  const dialogParams = new URLSearchParams(searchParams.toString());
  dialogParams.delete("action");
  const dialogRedirectUrl = buildUrl(pathname, dialogParams);

  if (!canWrite) {
    return null;
  }

  return requestedAction === "create" ? (
    <CreateAccountingPeriodForm
      isInOnboardingMode={isInOnboardingMode}
      latestAccountingPeriod={latestAccountingPeriod}
      onClose={() => {
        setAction(null);
      }}
      redirectUrl={dialogRedirectUrl}
    />
  ) : null;
};

export default AccountingPeriodWorkspaceActions;
export type { AccountingPeriodWorkspaceActionsProps };
