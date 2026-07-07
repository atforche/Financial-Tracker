"use client";

import { Button, Paper, Stack, Typography } from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { Account } from "@/accounts/types";
import type { AccountWorkspaceAction } from "@/accounts/workspace/AccountWorkspace";
import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateAccountForm from "@/accounts/workspace/CreateAccountForm";
import type { JSX } from "react";
import OnboardAccountForm from "@/accounts/workspace/OnboardAccountForm";
import ViewAccountForm from "@/accounts/workspace/ViewAccountForm";

/**
 * Props for the AccountWorkspaceActions component.
 */
interface AccountWorkspaceActionsProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly isInOnboardingMode: boolean;
  readonly selectedAccount: Account | null;
  readonly requestedAction: AccountWorkspaceAction | null;
}

const buildUrl = function (pathname: string, params: URLSearchParams): string {
  const query = params.toString();
  return query === "" ? pathname : `${pathname}?${query}`;
};

/**
 * Displays the available account actions for the current workspace selection.
 */
const AccountWorkspaceActions = function ({
  accountingPeriods,
  isInOnboardingMode,
  selectedAccount,
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

  const currentParams = new URLSearchParams(searchParams.toString());
  const dialogParams = new URLSearchParams(searchParams.toString());
  dialogParams.delete("action");
  const currentUrl = buildUrl(pathname, currentParams);
  const dialogRedirectUrl = buildUrl(pathname, dialogParams);
  const isCreateDialogOpen =
    selectedAccount === null &&
    !isInOnboardingMode &&
    requestedAction === "create";
  const isOnboardDialogOpen =
    selectedAccount === null &&
    isInOnboardingMode &&
    requestedAction === "onboard";

  if (selectedAccount !== null) {
    return (
      <ViewAccountForm account={selectedAccount} redirectUrl={currentUrl} />
    );
  }

  return (
    <>
      <Paper
        sx={{
          border: "1px solid",
          borderColor: "divider",
          borderRadius: 3,
          p: { xs: 2.5, md: 3 },
        }}
      >
        <Stack
          spacing={2}
          direction={{ xs: "column", sm: "row" }}
          justifyContent="space-between"
          alignItems={{ xs: "stretch", sm: "center" }}
        >
          <Stack spacing={0.75}>
            <Typography variant="h6">
              {isInOnboardingMode
                ? "Onboard Your First Account"
                : "Add Account"}
            </Typography>
            <Typography color="text.secondary">
              {isInOnboardingMode
                ? "Start the workspace by onboarding your first account."
                : "Create a new account without leaving the workspace."}
            </Typography>
          </Stack>
          <Button
            variant="contained"
            onClick={() => {
              setAction(isInOnboardingMode ? "onboard" : "create");
            }}
          >
            {isInOnboardingMode ? "Onboard account" : "Create account"}
          </Button>
        </Stack>
      </Paper>
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
