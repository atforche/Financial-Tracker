"use client";

import type { AccountingPeriod } from "@/accounting-periods/types";
import CardResponsiveGrid from "@/framework/view/CardResponsiveGrid";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import FundGoalWorkspaceCard from "@/fund-goals/workspace/FundGoalWorkspaceCard";
import type { FundGoalWorkspaceSearchParams } from "@/fund-goals/workspace/FundGoalWorkspace";
import type { JSX } from "react";
import { Typography } from "@mui/material";
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
          <FundGoalWorkspaceCard
            key={fundGoal.id}
            accountingPeriod={accountingPeriod}
            fundGoal={fundGoal}
            detailHref={routes.workspaceDetail(
              fundGoal.fund.id,
              detailSearchParams,
            )}
          />
        );
      })}
    </CardResponsiveGrid>
  );
};
export default FundGoalWorkspaceCards;
