"use client";

import { usePathname, useSearchParams } from "next/navigation";
import type { AccountWorkspaceAction } from "@/accounts/workspace/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateAccountForm from "@/accounts/workspace/CreateAccountForm";
import type { JSX } from "react";
import OnboardAccountForm from "@/accounts/workspace/OnboardAccountForm";
import { accountWorkspaceParamNames } from "@/accounts/workspace/searchParams";
import { buildUrl } from "@/framework/routes/helpers";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Props for the AccountWorkspaceActions component.
 */
interface AccountWorkspaceActionsProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly isInOnboardingMode: boolean;
  readonly requestedAction: AccountWorkspaceAction | null;
}

/**
 * Displays the dialog-backed account actions for the workspace page.
 */
const AccountWorkspaceActions = function ({
  accountingPeriods,
  isInOnboardingMode,
  requestedAction,
}: AccountWorkspaceActionsProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const updateParams = useSearchParamUpdater([]);

  const actionParamName = accountWorkspaceParamNames.action;
  const setAction = function (action: AccountWorkspaceAction | null): void {
    updateParams((params) => {
      if (action === null) {
        params.delete(actionParamName);
      } else {
        params.set(actionParamName, action);
      }
    });
  };

  const dialogParams = new URLSearchParams(searchParams.toString());
  dialogParams.delete(actionParamName);
  const dialogRedirectUrl = buildUrl(pathname, dialogParams);
  const isCreateDialogOpen =
    !isInOnboardingMode && requestedAction === "create";
  const isOnboardDialogOpen =
    isInOnboardingMode && requestedAction === "onboard";

  return (
    <>
      {isCreateDialogOpen ? (
        <CreateAccountForm
          accountingPeriods={accountingPeriods}
          open
          onClose={() => {
            setAction(null);
          }}
          redirectUrl={dialogRedirectUrl}
        />
      ) : null}
      {isOnboardDialogOpen ? (
        <OnboardAccountForm
          open
          onClose={() => {
            setAction(null);
          }}
          redirectUrl={dialogRedirectUrl}
        />
      ) : null}
    </>
  );
};

export default AccountWorkspaceActions;
