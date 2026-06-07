"use client";

import { Paper, Stack, ToggleButton, ToggleButtonGroup } from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { AccountingPeriodWorkspaceAction } from "@/accounting-periods/workspace/AccountingPeriodWorkspace";
import CloseAccountingPeriodForm from "@/accounting-periods/workspace/CloseAccountingPeriodForm";
import CreateAccountingPeriodForm from "@/accounting-periods/workspace/CreateAccountingPeriodForm";
import DeleteAccountingPeriodForm from "@/accounting-periods/workspace/DeleteAccountingPeriodForm";
import type { JSX } from "react";
import ReopenAccountingPeriodForm from "@/accounting-periods/workspace/ReopenAccountingPeriodForm";

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
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const allActions: readonly AccountingPeriodWorkspaceAction[] = [
    "create",
    "close",
    "reopen",
    "delete",
  ];
  const availableActions: readonly AccountingPeriodWorkspaceAction[] =
    selectedAccountingPeriod === null
      ? ["create"]
      : selectedAccountingPeriod.isOpen
        ? ["close", "delete", "create"]
        : ["reopen", "delete", "create"];
  const activeAction =
    requestedAction !== null && availableActions.includes(requestedAction)
      ? requestedAction
      : availableActions[0];

  const setAction = function (
    action: AccountingPeriodWorkspaceAction | null,
  ): void {
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
          onChange={(_, nextValue: AccountingPeriodWorkspaceAction | null) => {
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
                : action === "close"
                  ? "Close"
                  : action === "reopen"
                    ? "Reopen"
                    : "Delete"}
            </ToggleButton>
          ))}
        </ToggleButtonGroup>
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
      </Stack>
    </Paper>
  );
};

export default AccountingPeriodWorkspaceActions;
export type { AccountingPeriodWorkspaceActionsProps };
