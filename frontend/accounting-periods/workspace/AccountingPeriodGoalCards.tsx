"use client";

import type { JSX, ReactNode } from "react";
import { Paper, Stack, Typography } from "@mui/material";
import ChevronRight from "@mui/icons-material/ChevronRight";
import Link from "next/link";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import type { Route } from "next";
import SegmentedBar from "@/framework/view/SegmentedBar";
import accountGoalRoutes from "@/account-goals/routes";
import type { components } from "@/framework/data/api";
import fundGoalRoutes from "@/fund-goals/routes";

/**
 * Type representing counts of an accounting period's goals by their progress.
 */
type GoalSummary = components["schemas"]["AccountingPeriodGoalSummaryModel"];

/**
 * Props for the GoalCountRow component.
 */
interface GoalCountRowProps {
  readonly label: string;
  readonly value: string;
  readonly segments: Parameters<typeof SegmentedBar>[0]["segments"];
  readonly caption: string;
}

/**
 * Displays a count of goals as a labeled bar with a caption, laid out like a period progress row.
 */
const GoalCountRow = function ({
  label,
  value,
  segments,
  caption,
}: GoalCountRowProps): JSX.Element {
  return (
    <Stack spacing={0.75}>
      <Stack
        direction="row"
        spacing={2}
        sx={{ alignItems: "baseline", justifyContent: "space-between" }}
      >
        <Typography variant="subtitle2" noWrap sx={{ minWidth: 0 }}>
          {label}
        </Typography>
        <Typography
          variant="body2"
          sx={{ fontWeight: 500, whiteSpace: "nowrap" }}
        >
          {value}
        </Typography>
      </Stack>
      <SegmentedBar segments={segments} />
      <Typography variant="caption" color="text.secondary">
        {caption}
      </Typography>
    </Stack>
  );
};

/**
 * Props for the EndingBalanceRow component.
 */
interface EndingBalanceRowProps {
  readonly count: number;
  readonly belowMinimumCount: number;
  readonly aboveMaximumCount: number;
}

/**
 * Displays how many goals end the period within their balance range, and how many end below or above it.
 */
const EndingBalanceRow = function ({
  count,
  belowMinimumCount,
  aboveMaximumCount,
}: EndingBalanceRowProps): JSX.Element {
  const inRangeCount = count - belowMinimumCount - aboveMaximumCount;
  const outOfRange = [
    ...(belowMinimumCount > 0
      ? [`${belowMinimumCount.toString()} below minimum`]
      : []),
    ...(aboveMaximumCount > 0
      ? [`${aboveMaximumCount.toString()} above maximum`]
      : []),
  ];
  return (
    <GoalCountRow
      label="Ending balances"
      value={`${inRangeCount.toString()} of ${count.toString()} in range`}
      segments={[
        { key: "in-range", amount: inRangeCount, color: "primary.main" },
        { key: "below", amount: belowMinimumCount, color: "error.main" },
        { key: "above", amount: aboveMaximumCount, color: "warning.main" },
      ]}
      caption={
        outOfRange.length === 0 ? "All within range" : outOfRange.join(" · ")
      }
    />
  );
};

/**
 * Props for the GoalCard component.
 */
interface GoalCardProps {
  readonly title: string;
  readonly href: Route;
  readonly children: ReactNode;
}

/**
 * Displays a titled card that links to a goal workspace, with a chevron showing that the card is clickable.
 */
const GoalCard = function ({
  title,
  href,
  children,
}: GoalCardProps): JSX.Element {
  return (
    <Paper
      component={Link}
      href={href}
      sx={{
        display: "flex",
        flexDirection: "column",
        gap: 2,
        p: { xs: 2, md: 2.5 },
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        color: "inherit",
        textDecoration: "none",
        "&:hover": { borderColor: "primary.main" },
      }}
    >
      <Stack
        direction="row"
        spacing={1}
        sx={{ alignItems: "center", justifyContent: "space-between" }}
      >
        <Typography variant="h5">{title}</Typography>
        <ChevronRight aria-hidden sx={{ color: "text.secondary" }} />
      </Stack>
      {children}
    </Paper>
  );
};

/**
 * Props for the AccountingPeriodGoalCards component.
 */
interface AccountingPeriodGoalCardsProps {
  readonly accountingPeriodId: string;
  readonly summary: GoalSummary;
  readonly returnUrl: string;
}

/**
 * Summarizes an accounting period's Fund Goals and Account Goals as two cards linking to their workspaces filtered to
 * the period.
 */
const AccountingPeriodGoalCards = function ({
  accountingPeriodId,
  summary,
  returnUrl,
}: AccountingPeriodGoalCardsProps): JSX.Element {
  const remainingContributionCount =
    summary.contributionGoalCount - summary.contributedGoalCount;
  return (
    <ResponsiveGrid columns={{ xs: 1, md: 2 }}>
      <GoalCard
        title="Fund Goals"
        href={fundGoalRoutes.workspace({ accountingPeriodId, returnUrl })}
      >
        {summary.fundGoalCount === 0 ? (
          <Typography variant="body2" color="text.secondary">
            No Fund Goals for this period.
          </Typography>
        ) : (
          <>
            {summary.contributionGoalCount === 0 ? null : (
              <GoalCountRow
                label="Contributions"
                value={`${summary.contributedGoalCount.toString()} of ${summary.contributionGoalCount.toString()} assigned`}
                segments={[
                  {
                    key: "assigned",
                    amount: summary.contributedGoalCount,
                    color: "primary.main",
                  },
                  { key: "remaining", amount: remainingContributionCount },
                ]}
                caption={
                  remainingContributionCount === 0
                    ? "All assigned"
                    : `${remainingContributionCount.toString()} still to assign`
                }
              />
            )}
            <EndingBalanceRow
              count={summary.fundGoalCount}
              belowMinimumCount={summary.fundGoalsBelowMinimumCount}
              aboveMaximumCount={summary.fundGoalsAboveMaximumCount}
            />
          </>
        )}
      </GoalCard>
      <GoalCard
        title="Account Goals"
        href={accountGoalRoutes.workspace({ accountingPeriodId, returnUrl })}
      >
        {summary.accountGoalCount === 0 ? (
          <Typography variant="body2" color="text.secondary">
            No Account Goals for this period.
          </Typography>
        ) : (
          <EndingBalanceRow
            count={summary.accountGoalCount}
            belowMinimumCount={summary.accountGoalsBelowMinimumCount}
            aboveMaximumCount={summary.accountGoalsAboveMaximumCount}
          />
        )}
      </GoalCard>
    </ResponsiveGrid>
  );
};

export default AccountingPeriodGoalCards;
