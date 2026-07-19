"use client";

import {
  type AccountingPeriodWorkspaceAction,
  accountingPeriodWorkspaceActionLabels,
  accountingPeriodWorkspaceActions,
  getAvailableAccountingPeriodWorkspaceActions,
} from "@/accounting-periods/workspace/helpers";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { AccountingPeriodWorkspaceSearchParams } from "@/accounting-periods/workspace/AccountingPeriodWorkspace";
import CloseAccountingPeriodForm from "@/accounting-periods/workspace/CloseAccountingPeriodForm";
import CreateAccountingPeriodForm from "@/accounting-periods/workspace/CreateAccountingPeriodForm";
import DeleteAccountingPeriodForm from "@/accounting-periods/workspace/DeleteAccountingPeriodForm";
import Frame from "@/framework/view/Frame";
import type { JSX } from "react";
import ReopenAccountingPeriodForm from "@/accounting-periods/workspace/ReopenAccountingPeriodForm";
import ToggleButtonSelector from "@/framework/forms/ToggleButtonSelector";
import propertyName from "@/framework/data/propertyName";
import { usePathname } from "next/navigation";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Props for the AccountingPeriodWorkspaceActions component.
 */
interface AccountingPeriodWorkspaceActionsProps {
  readonly isInOnboardingMode: boolean;
  readonly selectedAccountingPeriod: AccountingPeriod | null;
  readonly requestedAction: AccountingPeriodWorkspaceAction | null;
}

/**
 * Displays the available accounting period actions for the current workspace selection.
 */
const AccountingPeriodWorkspaceActions = function ({
  isInOnboardingMode,
  selectedAccountingPeriod,
  requestedAction,
}: AccountingPeriodWorkspaceActionsProps): JSX.Element {
  const pathname = usePathname();
  const updateParams = useSearchParamUpdater([]);

  const actionParamName =
    propertyName<AccountingPeriodWorkspaceSearchParams>("action");
  const availableActions = getAvailableAccountingPeriodWorkspaceActions(
    selectedAccountingPeriod,
  );
  const activeAction =
    requestedAction !== null && availableActions.includes(requestedAction)
      ? requestedAction
      : (availableActions[0] ?? "create");

  const setAction = function (action: AccountingPeriodWorkspaceAction): void {
    updateParams((params) => {
      params.set(actionParamName, action);
    });
  };

  return (
    <Frame
      title="Actions"
      headerContent={
        <ToggleButtonSelector
          value={activeAction}
          onChange={setAction}
          options={accountingPeriodWorkspaceActions.map((action) => ({
            value: action,
            label: accountingPeriodWorkspaceActionLabels[action],
            disabled: !availableActions.includes(action),
          }))}
        />
      }
    >
      {activeAction === "create" ? (
        <CreateAccountingPeriodForm
          isInOnboardingMode={isInOnboardingMode}
          redirectUrl={pathname}
        />
      ) : null}
      {activeAction === "close" && selectedAccountingPeriod !== null ? (
        <CloseAccountingPeriodForm
          accountingPeriod={selectedAccountingPeriod}
          redirectUrl={pathname}
        />
      ) : null}
      {activeAction === "reopen" && selectedAccountingPeriod !== null ? (
        <ReopenAccountingPeriodForm
          accountingPeriod={selectedAccountingPeriod}
          redirectUrl={pathname}
        />
      ) : null}
      {activeAction === "delete" && selectedAccountingPeriod !== null ? (
        <DeleteAccountingPeriodForm
          accountingPeriod={selectedAccountingPeriod}
          redirectUrl={pathname}
        />
      ) : null}
    </Frame>
  );
};

export default AccountingPeriodWorkspaceActions;
export type { AccountingPeriodWorkspaceActionsProps };
