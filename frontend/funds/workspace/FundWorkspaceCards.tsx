"use client";

import { Button, Stack, Typography } from "@mui/material";
import {
  compareCurrencyAmounts,
  formatCurrency,
} from "@/framework/currencyHelpers";
import { useRouter, useSearchParams } from "next/navigation";
import CardResponsiveGrid from "@/framework/view/CardResponsiveGrid";
import type { FundWithBalance } from "@/funds/types";
import type { FundWorkspaceSearchParams } from "@/funds/workspace/types";
import type { JSX } from "react";
import WorkspaceCard from "@/framework/view/WorkspaceCard";
import propertyName from "@/framework/data/propertyName";
import routes from "@/funds/routes";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Props for the FundWorkspaceCards component.
 */
interface FundWorkspaceCardsProps {
  readonly data: FundWithBalance[] | null;
  readonly isInOnboardingMode: boolean;
}

/**
 * Displays the fund workspace as a collection of clickable cards.
 */
const FundWorkspaceCards = function ({
  data,
  isInOnboardingMode,
}: FundWorkspaceCardsProps): JSX.Element {
  const router = useRouter();
  const searchParams = useSearchParams();

  const searchParamName = propertyName<FundWorkspaceSearchParams>("search");
  const hasSearch = (searchParams.get(searchParamName) ?? "").trim() !== "";
  const funds = data ?? [];
  const updateParams = useSearchParamUpdater([]);

  const clearSearch = function (): void {
    updateParams((params) => {
      params.delete(searchParamName);
    });
  };

  const openFund = function (fundId: string): void {
    const search = searchParams.get(searchParamName);
    const detailSearchParams: FundWorkspaceSearchParams =
      search === null ? {} : { search };
    router.push(routes.workspaceDetail(fundId, detailSearchParams), {
      scroll: false,
    });
  };

  if (funds.length === 0) {
    return (
      <Stack spacing={2} alignItems="flex-start">
        <Typography color="text.secondary">
          {hasSearch
            ? "No funds match the current search. Try a different name."
            : isInOnboardingMode
              ? "Use onboarding to add the first fund to your workspace."
              : "Create a fund to start building your workspace."}
        </Typography>
        {hasSearch ? (
          <Button variant="contained" onClick={clearSearch}>
            Clear search
          </Button>
        ) : null}
      </Stack>
    );
  }

  return (
    <CardResponsiveGrid minimumColumnWidth={280} spacing={2}>
      {funds.map((fund) => (
        <WorkspaceCard
          key={fund.id}
          title={fund.name}
          color={
            compareCurrencyAmounts(fund.currentBalance.postedBalance, 0) >= 0
              ? "info"
              : "error"
          }
          onClick={() => {
            openFund(fund.id);
          }}
        >
          <Stack spacing={0.5}>
            <Typography
              variant="overline"
              sx={{ color: "text.secondary", fontWeight: 700 }}
            >
              Current balance
            </Typography>
            <Typography variant="h5">
              {formatCurrency(fund.currentBalance.postedBalance)}
            </Typography>
          </Stack>
        </WorkspaceCard>
      ))}
    </CardResponsiveGrid>
  );
};

export default FundWorkspaceCards;
