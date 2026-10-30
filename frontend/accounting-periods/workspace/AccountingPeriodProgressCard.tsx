import { Stack, Typography } from "@mui/material";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import ContentSurface from "@/framework/view/ContentSurface";
import type { JSX } from "react";
import PeriodProgressRow from "@/accounting-periods/PeriodProgressRow";
import { getCurrencyDifference } from "@/framework/currencyHelpers";

/**
 * The income and Fund Goal contribution totals the progress card compares, for one accounting period or a range.
 */
type AccountingPeriodProgress = Pick<
  AccountingPeriodWithBalance,
  | "expectedIncome"
  | "actualIncome"
  | "unplannedIncome"
  | "expectedGoalContributions"
  | "actualGoalContributions"
  | "actualUnplannedGoalContributions"
>;

/**
 * Props for the AccountingPeriodProgressCard component.
 */
interface AccountingPeriodProgressCardProps {
  readonly accountingPeriod: AccountingPeriodProgress;
  readonly totalSpending: number;
}

/**
 * Compares an accounting period's, or a range's, planned income, goal contributions, and spending against what it expected, with
 * unplanned income and the goal contributions made from it shown as segments of their own.
 */
const AccountingPeriodProgressCard = function ({
  accountingPeriod,
  totalSpending,
}: AccountingPeriodProgressCardProps): JSX.Element {
  const { expectedIncome, actualIncome, unplannedIncome } = accountingPeriod;
  return (
    <ContentSurface>
      <Stack spacing={2}>
        <Typography variant="h5">Progress</Typography>
        <PeriodProgressRow
          label="Tracked income"
          overLabel="Extra planned income"
          actual={getCurrencyDifference(
            actualIncome.tracked,
            unplannedIncome.tracked,
          )}
          expected={expectedIncome.tracked}
          unplanned={unplannedIncome.tracked}
        />
        <PeriodProgressRow
          label="Untracked income"
          overLabel="Extra planned income"
          actual={getCurrencyDifference(
            actualIncome.untracked,
            unplannedIncome.untracked,
          )}
          expected={expectedIncome.untracked}
          unplanned={unplannedIncome.untracked}
        />
        <PeriodProgressRow
          label="Fund contributions"
          overLabel="Extra planned contributions"
          actual={accountingPeriod.actualGoalContributions}
          expected={accountingPeriod.expectedGoalContributions}
          unplanned={accountingPeriod.actualUnplannedGoalContributions}
          actualLabel="Contributed"
        />
        <PeriodProgressRow
          label="Spending"
          actual={totalSpending}
          expected={actualIncome.tracked}
          actualLabel="Spending"
          expectedLabel="Tracked income"
          overLabel="Overspent"
          underLabel="Left over"
          isOverGood={false}
        />
      </Stack>
    </ContentSurface>
  );
};

export type { AccountingPeriodProgress };
export default AccountingPeriodProgressCard;
