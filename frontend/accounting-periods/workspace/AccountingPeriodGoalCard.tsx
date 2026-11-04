import { Stack, Typography } from "@mui/material";
import ContentSurface from "@/framework/view/ContentSurface";
import type { JSX } from "react";
import SegmentedBar from "@/framework/view/SegmentedBar";
import type { components } from "@/framework/data/api";

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
 * Props for the AccountingPeriodGoalCard component.
 */
interface AccountingPeriodGoalCardProps {
  readonly type: "account" | "fund";
  readonly summary: GoalSummary;
}

/**
 * Summarizes an accounting period's Fund Goals or Account Goals above the table listing them.
 */
const AccountingPeriodGoalCard = function ({
  type,
  summary,
}: AccountingPeriodGoalCardProps): JSX.Element {
  const remainingContributionCount =
    summary.contributionGoalCount - summary.contributedGoalCount;
  const content =
    type === "account" ? (
      summary.accountGoalCount === 0 ? (
        <Typography variant="body2" color="text.secondary">
          No Account Goals for this period.
        </Typography>
      ) : (
        <EndingBalanceRow
          count={summary.accountGoalCount}
          belowMinimumCount={summary.accountGoalsBelowMinimumCount}
          aboveMaximumCount={summary.accountGoalsAboveMaximumCount}
        />
      )
    ) : summary.fundGoalCount === 0 ? (
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
    );
  return (
    <ContentSurface>
      <Stack spacing={2}>
        <Typography variant="h5">
          {type === "account" ? "Account Goals" : "Fund Goals"}
        </Typography>
        {content}
      </Stack>
    </ContentSurface>
  );
};

export default AccountingPeriodGoalCard;
