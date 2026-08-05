"use client";

import {
  type AccountingPeriodWorkspaceAction,
  getAvailableAccountingPeriodWorkspaceActions,
} from "@/accounting-periods/workspace/helpers";
import { usePathname, useSearchParams } from "next/navigation";
import type { AccountingPeriod } from "@/accounting-periods/types";
import CloseAccountingPeriodForm from "@/accounting-periods/workspace/CloseAccountingPeriodForm";
import CreateAccountingPeriodForm from "@/accounting-periods/workspace/CreateAccountingPeriodForm";
import DeleteAccountingPeriodForm from "@/accounting-periods/workspace/DeleteAccountingPeriodForm";
import type { JSX } from "react";
import ReopenAccountingPeriodForm from "@/accounting-periods/workspace/ReopenAccountingPeriodForm";
import { buildUrl } from "@/framework/routes/helpers";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the AccountingPeriodWorkspaceActions component.
 */
interface AccountingPeriodWorkspaceActionsProps {
  readonly isInOnboardingMode: boolean;
  readonly latestAccountingPeriod: AccountingPeriod | null;
  readonly selectedAccountingPeriod: AccountingPeriod | null;
  readonly requestedAction: AccountingPeriodWorkspaceAction | null;
}

/**
 * Displays the available accounting period actions for the current workspace selection.
 */
const AccountingPeriodWorkspaceActions = function ({
  isInOnboardingMode,
  latestAccountingPeriod,
  selectedAccountingPeriod,
  requestedAction,
}: AccountingPeriodWorkspaceActionsProps): JSX.Element | null {
  const canWrite = useWriteAccess();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const updateParams = useSearchParamUpdater([]);
  const availableActions = getAvailableAccountingPeriodWorkspaceActions(
    selectedAccountingPeriod,
  );
  const activeAction =
    requestedAction !== null && availableActions.includes(requestedAction)
      ? requestedAction
      : null;

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

  return (
    <>
      {activeAction === "create" ? (
        <CreateAccountingPeriodForm
          isInOnboardingMode={isInOnboardingMode}
          latestAccountingPeriod={latestAccountingPeriod}
          open
          onClose={() => {
            setAction(null);
          }}
          redirectUrl={dialogRedirectUrl}
        />
      ) : null}
      {activeAction === "close" && selectedAccountingPeriod !== null ? (
        <CloseAccountingPeriodForm
          accountingPeriod={selectedAccountingPeriod}
          open
          onClose={() => {
            setAction(null);
          }}
          redirectUrl={dialogRedirectUrl}
        />
      ) : null}
      {activeAction === "reopen" && selectedAccountingPeriod !== null ? (
        <ReopenAccountingPeriodForm
          accountingPeriod={selectedAccountingPeriod}
          open
          onClose={() => {
            setAction(null);
          }}
          redirectUrl={dialogRedirectUrl}
        />
      ) : null}
      {activeAction === "delete" && selectedAccountingPeriod !== null ? (
        <DeleteAccountingPeriodForm
          accountingPeriod={selectedAccountingPeriod}
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

export default AccountingPeriodWorkspaceActions;
export type { AccountingPeriodWorkspaceActionsProps };
