"use client";

import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { AccountWorkspaceAction } from "@/accounts/workspace/AccountWorkspace";
import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateAccountForm from "@/accounts/workspace/CreateAccountForm";
import type { JSX } from "react";
import OnboardAccountForm from "@/accounts/workspace/OnboardAccountForm";

/**
 * Props for the AccountWorkspaceActions component.
 */
interface AccountWorkspaceActionsProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly isInOnboardingMode: boolean;
  readonly requestedAction: AccountWorkspaceAction | null;
}

const buildUrl = function (pathname: string, params: URLSearchParams): string {
  const query = params.toString();
  return query === "" ? pathname : `${pathname}?${query}`;
};

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
  const router = useRouter();

  const setAction = function (action: AccountWorkspaceAction | null): void {
    const params = new URLSearchParams(searchParams.toString());

    if (action === null) {
      params.delete("action");
    } else {
      params.set("action", action);
    }

    router.replace(buildUrl(pathname, params), { scroll: false });
  };

  const dialogParams = new URLSearchParams(searchParams.toString());
  dialogParams.delete("action");
  const dialogRedirectUrl = buildUrl(pathname, dialogParams);
  const isCreateDialogOpen =
    !isInOnboardingMode && requestedAction === "create";
  const isOnboardDialogOpen =
    isInOnboardingMode && requestedAction === "onboard";

  return (
    <>
      <CreateAccountForm
        accountingPeriods={accountingPeriods}
        open={isCreateDialogOpen}
        onClose={() => {
          setAction(null);
        }}
        redirectUrl={dialogRedirectUrl}
      />
      <OnboardAccountForm
        open={isOnboardDialogOpen}
        onClose={() => {
          setAction(null);
        }}
        redirectUrl={dialogRedirectUrl}
      />
    </>
  );
};

export default AccountWorkspaceActions;
