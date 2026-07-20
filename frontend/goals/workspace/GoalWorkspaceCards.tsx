"use client";

import {
  EndingBalanceStatus,
  type FundPlanWithProgress,
  FundedBalanceStatus,
} from "@/goals/types";
import { Stack, Typography } from "@mui/material";
import type { AccountingPeriod } from "@/accounting-periods/types";
import GoalProgress from "@/goals/workspace/GoalProgress";
import type { GoalWorkspaceSearchParams } from "@/goals/workspace/GoalWorkspace";
import type { JSX } from "react";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import WorkspaceCard from "@/framework/view/WorkspaceCard";
import propertyName from "@/framework/data/propertyName";
import routes from "@/goals/routes";
import { useSearchParams } from "next/navigation";

/**
 * Props for the GoalWorkspaceCards component.
 */
interface GoalWorkspaceCardsProps {
  readonly accountingPeriod: AccountingPeriod | null;
  readonly fundPlans: FundPlanWithProgress[];
}

/**
 * Displays paired goal progress as navigable workspace cards.
 */
const GoalWorkspaceCards = function ({
  accountingPeriod,
  fundPlans,
}: GoalWorkspaceCardsProps): JSX.Element {
  const searchParams = useSearchParams();
  const search = (
    searchParams.get(propertyName<GoalWorkspaceSearchParams>("search")) ?? ""
  )
    .trim()
    .toLowerCase();
  const filtered = fundPlans.filter((plan) =>
    plan.fund.name.toLowerCase().includes(search),
  );
  if (!filtered.length) {
    return (
      <Typography color="text.secondary">
        No goals match the selected accounting period and search filters.
      </Typography>
    );
  }
  return (
    <ResponsiveGrid minimumColumnWidth={340} spacing={2}>
      {filtered.map((plan) => {
        const configured = [
          plan.progress.contribution,
          plan.progress.fundedBalance,
          plan.progress.endingBalance,
        ].filter(Boolean).length;
        const satisfied = [
          plan.progress.contribution?.isSatisfied,
          plan.progress.fundedBalance?.status ===
            FundedBalanceStatus.WithinRange,
          plan.progress.endingBalance?.status === EndingBalanceStatus.AtTarget,
        ].filter(Boolean).length;
        const detailSearchParams: GoalWorkspaceSearchParams = {
          ...(accountingPeriod
            ? { accountingPeriodId: accountingPeriod.id }
            : {}),
          ...(search ? { search } : {}),
        };
        const ids = searchParams.getAll(
          propertyName<GoalWorkspaceSearchParams>("fundIds"),
        );
        if (ids.length) {
          detailSearchParams.fundIds = ids;
        }
        return (
          <WorkspaceCard
            key={plan.id}
            title={plan.fund.name}
            href={routes.workspaceDetail(plan.fund.id, detailSearchParams)}
            color={
              configured === 0
                ? "info"
                : satisfied === configured
                  ? "success"
                  : "warning"
            }
          >
            <Stack spacing={2}>
              <Typography variant="body2" color="text.secondary">
                {accountingPeriod?.name ?? "No accounting period"}
              </Typography>
              {plan.progress.contribution ? (
                <GoalProgress
                  label="Monthly contribution"
                  current={plan.progress.contribution.assignedAmount}
                  target={plan.progress.contribution.targetAmount}
                  satisfied={plan.progress.contribution.isSatisfied}
                />
              ) : (
                <Typography variant="body2">
                  No contribution goal configured
                </Typography>
              )}
              <Typography variant="body2" color="text.secondary">
                Available goal dimensions: {configured} of 3
              </Typography>
            </Stack>
          </WorkspaceCard>
        );
      })}
    </ResponsiveGrid>
  );
};
export default GoalWorkspaceCards;
