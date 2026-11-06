import { Divider, Stack, Typography } from "@mui/material";
import type { FundGoal, FundGoalProgress } from "@/fund-goals/types";
import {
  compareCurrencyAmounts,
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import type { AccountingPeriod } from "@/accounting-periods/types";
import ContentSurface from "@/framework/view/ContentSurface";
import ContributionReductionRow from "@/accounting-periods/workspace/ContributionReductionRow";
import GoalDetailRow from "@/accounting-periods/workspace/GoalDetailRow";
import GoalEndingBalanceSection from "@/accounting-periods/workspace/GoalEndingBalanceSection";
import type { JSX } from "react";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import SegmentedBar from "@/framework/view/SegmentedBar";
import UpdateFundGoalForm from "@/fund-goals/workspace/UpdateFundGoalForm";
import { breakdownValueInset } from "@/framework/view/BreakdownSection";

/**
 * Shows how much of a period's expected contribution has been made and how that amount was reached.
 */
const ContributionSection = function ({
  contribution,
  endingBalance,
}: {
  readonly contribution: FundGoalProgress["contribution"];
  readonly endingBalance: FundGoalProgress["endingBalance"];
}): JSX.Element {
  if (contribution === null || contribution === undefined) {
    return (
      <Stack spacing={1}>
        <Typography variant="subtitle2">Contribution</Typography>
        <Typography variant="body2" color="text.secondary">
          No contribution is planned.
        </Typography>
      </Stack>
    );
  }
  const {
    assignedAmount,
    expectedAmount,
    plannedAmount,
    remainingAmount,
    amountReducedByMaximumEndingBalance: reduced,
  } = contribution;
  const limit = endingBalance.maximumBalance ?? null;
  const isReduced = compareCurrencyAmounts(reduced, 0) > 0 && limit !== null;
  const isRemaining = compareCurrencyAmounts(remainingAmount, 0) > 0;
  const isNothingExpected =
    compareCurrencyAmounts(expectedAmount, 0) === 0 &&
    compareCurrencyAmounts(assignedAmount, 0) === 0;
  return (
    <Stack spacing={1}>
      <Typography variant="subtitle2">Contribution</Typography>
      <Typography variant="h5" component="p">
        {formatCurrency(assignedAmount)} of {formatCurrency(expectedAmount)}
      </Typography>
      <SegmentedBar
        segments={[
          {
            key: "assigned",
            amount: Math.min(assignedAmount, expectedAmount),
            color: "primary.main",
          },
          {
            key: "extra",
            amount: Math.max(assignedAmount - expectedAmount, 0),
            color: "success.main",
          },
        ]}
        total={Math.max(assignedAmount, expectedAmount)}
      />
      <Typography
        variant="body2"
        color={
          isRemaining || isNothingExpected ? "text.secondary" : "success.main"
        }
      >
        {isRemaining
          ? `${formatCurrency(remainingAmount)} remaining`
          : isNothingExpected
            ? "Nothing needed this period"
            : "Fully contributed"}
      </Typography>
      <Divider />
      <GoalDetailRow
        label="Planned"
        value={formatCurrency(plannedAmount)}
        valueInset={isReduced ? breakdownValueInset : 0}
      />
      {isReduced ? (
        <ContributionReductionRow
          reduction={reduced}
          currentBalance={endingBalance.endingBalance}
          plannedRemaining={Math.max(
            getCurrencyDifference(plannedAmount, assignedAmount),
            0,
          )}
          limit={limit}
          limitName={
            compareCurrencyAmounts(endingBalance.minimumBalance, limit) === 0
              ? "target"
              : "maximum"
          }
        />
      ) : null}
    </Stack>
  );
};

/**
 * Props for the PeriodFundGoalCard component.
 */
interface PeriodFundGoalCardProps {
  readonly goal: FundGoal;
  readonly progress: FundGoalProgress;
  readonly accountingPeriod: AccountingPeriod;
  readonly redirectUrl: string;
}

/**
 * Answers the two questions a fund's goal asks of a period: whether enough was contributed, and whether the balance
 * ended where it should.
 */
const PeriodFundGoalCard = function ({
  goal,
  progress,
  accountingPeriod,
  redirectUrl,
}: PeriodFundGoalCardProps): JSX.Element {
  return (
    <ContentSurface>
      <Stack spacing={2}>
        <Stack
          direction="row"
          spacing={2}
          sx={{ alignItems: "center", justifyContent: "space-between" }}
        >
          <Typography variant="h5">Goal</Typography>
          {accountingPeriod.isOpen ? (
            <UpdateFundGoalForm
              fundGoal={goal}
              accountingPeriodId={accountingPeriod.id}
              redirectUrl={redirectUrl}
            />
          ) : null}
        </Stack>
        <ResponsiveGrid columns={{ xs: 1, md: 2 }} spacing={4}>
          <ContributionSection
            contribution={progress.contribution}
            endingBalance={progress.endingBalance}
          />
          <GoalEndingBalanceSection
            {...progress.endingBalance}
            balance={progress.endingBalance.endingBalance}
          />
        </ResponsiveGrid>
      </Stack>
    </ContentSurface>
  );
};

export default PeriodFundGoalCard;
