import { Stack, Typography } from "@mui/material";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import ContentSurface from "@/framework/view/ContentSurface";
import type { JSX } from "react";
import PeriodProgressRow from "@/accounting-periods/PeriodProgressRow";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the AccountingPeriodProgressCard component.
 */
interface AccountingPeriodProgressCardProps {
  readonly accountingPeriod: AccountingPeriodWithBalance;
  readonly totalSpending: number;
}

/**
 * Compares an accounting period's income, goal contributions, and spending against what it expected.
 */
const AccountingPeriodProgressCard = function ({
  accountingPeriod,
  totalSpending,
}: AccountingPeriodProgressCardProps): JSX.Element {
  const { expectedIncome, actualIncome } = accountingPeriod;
  return (
    <ContentSurface>
      <Stack spacing={2}>
        <Typography variant="h5">Progress</Typography>
        <PeriodProgressRow
          label="Tracked income"
          actual={actualIncome.tracked}
          expected={expectedIncome.tracked}
        />
        <PeriodProgressRow
          label="Untracked income"
          actual={actualIncome.untracked}
          expected={expectedIncome.untracked}
        />
        <Stack spacing={1}>
          <PeriodProgressRow
            label="Goal contributions"
            actual={accountingPeriod.actualGoalContributions}
            expected={accountingPeriod.expectedGoalContributions}
          />
          <Stack
            direction="row"
            spacing={2}
            sx={{ alignItems: "baseline", justifyContent: "space-between" }}
          >
            <Typography variant="subtitle2">Extra contributions</Typography>
            <Typography variant="body2" sx={{ fontWeight: 500 }}>
              {formatCurrency(accountingPeriod.actualExtraGoalContributions)}
            </Typography>
          </Stack>
        </Stack>
        <PeriodProgressRow
          label="Spending"
          actual={totalSpending}
          expected={actualIncome.tracked}
          isExtraGood={false}
        />
      </Stack>
    </ContentSurface>
  );
};

export default AccountingPeriodProgressCard;
