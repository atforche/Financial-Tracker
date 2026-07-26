"use client";

import {
  EndingBalanceStatus,
  type FundGoalWithProgress,
  FundedBalanceStatus,
} from "@/fund-goals/types";
import { Stack, Typography } from "@mui/material";
import type { AccountingPeriod } from "@/accounting-periods/types";
import CardResponsiveGrid from "@/framework/view/CardResponsiveGrid";
import FundGoalProgressBars from "@/fund-goals/workspace/FundGoalProgressBars";
import type { FundGoalWorkspaceSearchParams } from "@/fund-goals/workspace/FundGoalWorkspace";
import type { JSX } from "react";
import WorkspaceCard from "@/framework/view/WorkspaceCard";
import propertyName from "@/framework/data/propertyName";
import routes from "@/fund-goals/routes";
import { useSearchParams } from "next/navigation";

/**
 * Props for the FundGoalWorkspaceCards component.
 */
interface FundGoalWorkspaceCardsProps {
  readonly accountingPeriod: AccountingPeriod | null;
  readonly fundGoals: FundGoalWithProgress[];
}

/**
 * Displays Fund Goal progress as navigable workspace cards.
 */
const FundGoalWorkspaceCards = function ({
  accountingPeriod,
  fundGoals,
}: FundGoalWorkspaceCardsProps): JSX.Element {
  const searchParams = useSearchParams();
  const search = (
    searchParams.get(propertyName<FundGoalWorkspaceSearchParams>("search")) ??
    ""
  )
    .trim()
    .toLowerCase();
  const filtered = fundGoals.filter((fundGoal) =>
    fundGoal.fund.name.toLowerCase().includes(search),
  );
  if (!filtered.length) {
    return (
      <Typography color="text.secondary">
        No Fund Goals match the selected accounting period and search filters.
      </Typography>
    );
  }
  return (
    <CardResponsiveGrid minimumColumnWidth={340} spacing={2}>
      {filtered.map((fundGoal) => {
        const configured = [
          fundGoal.progress.contribution,
          fundGoal.progress.fundedBalance?.minimumBalance,
          fundGoal.progress.fundedBalance?.maximumBalance,
          fundGoal.progress.endingBalance,
        ].filter((value) => value !== null && value !== undefined).length;
        const satisfied = [
          fundGoal.progress.contribution?.isSatisfied,
          fundGoal.progress.fundedBalance?.minimumBalance !== null &&
            fundGoal.progress.fundedBalance?.minimumBalance !== undefined &&
            fundGoal.progress.fundedBalance.status !==
              FundedBalanceStatus.BelowMinimum,
          fundGoal.progress.fundedBalance?.maximumBalance !== null &&
            fundGoal.progress.fundedBalance?.maximumBalance !== undefined &&
            fundGoal.progress.fundedBalance.status !==
              FundedBalanceStatus.AboveMaximum,
          fundGoal.progress.endingBalance?.status ===
            EndingBalanceStatus.AtTarget,
        ].filter(Boolean).length;
        const detailSearchParams: FundGoalWorkspaceSearchParams = {
          ...(accountingPeriod
            ? { accountingPeriodId: accountingPeriod.id }
            : {}),
          ...(search ? { search } : {}),
        };
        const ids = searchParams.getAll(
          propertyName<FundGoalWorkspaceSearchParams>("fundIds"),
        );
        if (ids.length) {
          detailSearchParams.fundIds = ids;
        }
        return (
          <WorkspaceCard
            key={fundGoal.id}
            title={fundGoal.fund.name}
            href={routes.workspaceDetail(fundGoal.fund.id, detailSearchParams)}
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
              <FundGoalProgressBars progress={fundGoal.progress} />
            </Stack>
          </WorkspaceCard>
        );
      })}
    </CardResponsiveGrid>
  );
};
export default FundGoalWorkspaceCards;
