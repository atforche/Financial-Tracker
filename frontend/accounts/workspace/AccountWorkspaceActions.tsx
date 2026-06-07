"use client";

import { Paper, Stack, ToggleButton, ToggleButtonGroup } from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { Account } from "@/accounts/types";
import type { AccountWorkspaceAction } from "@/accounts/workspace/AccountWorkspace";
import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateAccountForm from "@/accounts/workspace/CreateAccountForm";
import DeleteAccountForm from "@/accounts/workspace/DeleteAccountForm";
import type { JSX } from "react";
import OnboardAccountForm from "@/accounts/workspace/OnboardAccountForm";
import UpdateAccountForm from "@/accounts/workspace/UpdateAccountForm";

/**
 * Props for the AccountWorkspaceActions component.
 */
interface AccountWorkspaceActionsProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly isInOnboardingMode: boolean;
  readonly selectedAccount: Account | null;
  readonly requestedAction: AccountWorkspaceAction | null;
}

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

  const allActions: readonly AccountWorkspaceAction[] = isInOnboardingMode
    ? ["onboard", "update", "delete"]
    : ["create", "update", "delete"];
  const availableActions: readonly AccountWorkspaceAction[] =
    selectedAccount === null
      ? isInOnboardingMode
        ? ["onboard"]
        : ["create"]
      : ["update", "delete"];
  const activeAction =
    requestedAction !== null && availableActions.includes(requestedAction)
      ? requestedAction
      : availableActions[0];

  const setAction = function (action: AccountWorkspaceAction | null): void {
    const params = new URLSearchParams(searchParams.toString());

    if (action === null) {
      params.delete("action");
    } else {
      params.set("action", action);
    }

    router.replace(`${pathname}?${params.toString()}`, { scroll: false });
  };

  return (
    <Paper
      sx={{
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        p: { xs: 2.5, md: 3 },
      }}
    >
      <Stack spacing={3}>
        <ToggleButtonGroup
          value={activeAction}
          exclusive
          onChange={(_, nextValue: AccountWorkspaceAction | null) => {
            setAction(nextValue);
          }}
          sx={{ flexWrap: "wrap" }}
        >
          {allActions.map((action) => (
            <ToggleButton
              key={action}
              value={action}
              disabled={!availableActions.includes(action)}
            >
              {action === "create"
                ? "Create"
                : action === "onboard"
                  ? "Onboard"
                  : action === "update"
                    ? "Update"
                    : "Delete"}
            </ToggleButton>
          ))}
        </ToggleButtonGroup>
        {activeAction === "create" ? (
          <CreateAccountForm
            accountingPeriods={accountingPeriods}
            redirectUrl={pathname}
          />
        ) : null}
        {activeAction === "onboard" ? (
          <OnboardAccountForm redirectUrl={pathname} />
        ) : null}
        {activeAction === "update" && selectedAccount !== null ? (
          <UpdateAccountForm account={selectedAccount} redirectUrl={pathname} />
        ) : null}
        {activeAction === "delete" && selectedAccount !== null ? (
          <DeleteAccountForm account={selectedAccount} redirectUrl={pathname} />
        ) : null}
      </Stack>
    </Paper>
  );
};

export default AccountWorkspaceActions;
