"use client";

import {
  EndingBalanceStatus,
  type FundPlanWithProgress,
  FundedBalanceStatus,
} from "@/fund-plans/types";
import { Stack, Typography } from "@mui/material";
import type { AccountingPeriod } from "@/accounting-periods/types";
import FundPlanProgressBars from "@/fund-plans/workspace/FundPlanProgressBars";
import type { FundPlanWorkspaceSearchParams } from "@/fund-plans/workspace/FundPlanWorkspace";
import type { JSX } from "react";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import WorkspaceCard from "@/framework/view/WorkspaceCard";
import propertyName from "@/framework/data/propertyName";
import routes from "@/fund-plans/routes";
import { useSearchParams } from "next/navigation";

/**
 * Props for the FundPlanWorkspaceCards component.
 */
interface FundPlanWorkspaceCardsProps {
  readonly accountingPeriod: AccountingPeriod | null;
  readonly fundPlans: FundPlanWithProgress[];
}

/**
 * Displays Funding Plan progress as navigable workspace cards.
 */
const FundPlanWorkspaceCards = function ({
  accountingPeriod,
  fundPlans,
}: FundPlanWorkspaceCardsProps): JSX.Element {
  const searchParams = useSearchParams();
  const search = (
    searchParams.get(propertyName<FundPlanWorkspaceSearchParams>("search")) ??
    ""
  )
    .trim()
    .toLowerCase();
  const filtered = fundPlans.filter((plan) =>
    plan.fund.name.toLowerCase().includes(search),
  );
  if (!filtered.length) {
    return (
      <Typography color="text.secondary">
        No Funding Plans match the selected accounting period and search
        filters.
      </Typography>
    );
  }
  return (
    <ResponsiveGrid minimumColumnWidth={340} spacing={2}>
      {filtered.map((plan) => {
        const configured = [
          plan.progress.contribution,
          plan.progress.fundedBalance?.minimumBalance,
          plan.progress.fundedBalance?.maximumBalance,
          plan.progress.endingBalance,
        ].filter((value) => value !== null && value !== undefined).length;
        const satisfied = [
          plan.progress.contribution?.isSatisfied,
          plan.progress.fundedBalance?.minimumBalance !== null &&
            plan.progress.fundedBalance?.minimumBalance !== undefined &&
            plan.progress.fundedBalance.status !==
              FundedBalanceStatus.BelowMinimum,
          plan.progress.fundedBalance?.maximumBalance !== null &&
            plan.progress.fundedBalance?.maximumBalance !== undefined &&
            plan.progress.fundedBalance.status !==
              FundedBalanceStatus.AboveMaximum,
          plan.progress.endingBalance?.status === EndingBalanceStatus.AtTarget,
        ].filter(Boolean).length;
        const detailSearchParams: FundPlanWorkspaceSearchParams = {
          ...(accountingPeriod
            ? { accountingPeriodId: accountingPeriod.id }
            : {}),
          ...(search ? { search } : {}),
        };
        const ids = searchParams.getAll(
          propertyName<FundPlanWorkspaceSearchParams>("fundIds"),
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
              <FundPlanProgressBars progress={plan.progress} />
              {configured === 0 ? (
                <Typography variant="body2">
                  No plan targets configured
                </Typography>
              ) : null}
            </Stack>
          </WorkspaceCard>
        );
      })}
    </ResponsiveGrid>
  );
};
export default FundPlanWorkspaceCards;
