"use client";

import { Paper, Stack, ToggleButton, ToggleButtonGroup } from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateFundForm from "@/funds/workspace/CreateFundForm";
import DeleteFundForm from "@/funds/workspace/DeleteFundForm";
import type { Fund } from "@/funds/types";
import type { FundWorkspaceAction } from "@/funds/workspace/FundWorkspace";
import type { JSX } from "react";
import OnboardFundForm from "@/funds/workspace/OnboardFundForm";
import UpdateFundForm from "@/funds/workspace/UpdateFundForm";

/**
 * Props for the FundWorkspaceActions component.
 */
interface FundWorkspaceActionsProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly isInOnboardingMode: boolean;
  readonly selectedFund: Fund | null;
  readonly unassignedBalance: number | null;
  readonly requestedAction: FundWorkspaceAction | null;
}

/**
 * Displays the available fund actions for the current workspace selection.
 */
const FundWorkspaceActions = function ({
  accountingPeriods,
  isInOnboardingMode,
  selectedFund,
  unassignedBalance,
  requestedAction,
}: FundWorkspaceActionsProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const allActions: readonly FundWorkspaceAction[] = isInOnboardingMode
    ? ["onboard", "update", "delete"]
    : ["create", "update", "delete"];
  const availableActions: readonly FundWorkspaceAction[] =
    selectedFund === null
      ? isInOnboardingMode
        ? ["onboard"]
        : ["create"]
      : ["update", "delete"];
  const activeAction =
    requestedAction !== null && availableActions.includes(requestedAction)
      ? requestedAction
      : availableActions[0];

  const setAction = function (action: FundWorkspaceAction | null): void {
    const params = new URLSearchParams(searchParams.toString());

    if (action === null) {
      params.delete("action");
    } else {
      params.set("action", action);
    }

    router.replace(`${pathname}?${params.toString()}`);
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
          onChange={(_, nextValue: FundWorkspaceAction | null) => {
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
          <CreateFundForm
            accountingPeriods={accountingPeriods}
            redirectUrl={pathname}
          />
        ) : null}
        {activeAction === "onboard" ? (
          <OnboardFundForm
            redirectUrl={pathname}
            unassignedBalance={unassignedBalance}
          />
        ) : null}
        {activeAction === "update" && selectedFund !== null ? (
          <UpdateFundForm fund={selectedFund} redirectUrl={pathname} />
        ) : null}
        {activeAction === "delete" && selectedFund !== null ? (
          <DeleteFundForm fund={selectedFund} redirectUrl={pathname} />
        ) : null}
      </Stack>
    </Paper>
  );
};

export default FundWorkspaceActions;
