import { Divider, Stack, Typography } from "@mui/material";
import {
  compareCurrencyAmounts,
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import type { JSX } from "react";

interface FundGoalContributionAdjustmentProps {
  readonly expectedAmount: number;
  readonly plannedAmount: number;
}

/**
 * Displays how a planned contribution was adjusted to its expected amount.
 */
const FundGoalContributionAdjustment = function ({
  expectedAmount,
  plannedAmount,
}: FundGoalContributionAdjustmentProps): JSX.Element {
  const adjustment = getCurrencyDifference(plannedAmount, expectedAmount);
  const hasAdjustment = compareCurrencyAmounts(adjustment, 0) !== 0;

  return (
    <Stack spacing={0.75}>
      <Stack direction="row" justifyContent="space-between" gap={2}>
        <Typography variant="body2" fontWeight={700}>
          Planned Contribution
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {formatCurrency(plannedAmount)}
        </Typography>
      </Stack>
      {hasAdjustment ? (
        <Stack
          direction="row"
          justifyContent="space-between"
          gap={2}
          sx={{ pl: 2 }}
        >
          <Typography variant="body2" color="text.secondary">
            Planned Ending Balance Overage
          </Typography>
          <Typography variant="body2" color="text.secondary">
            −{formatCurrency(adjustment)}
          </Typography>
        </Stack>
      ) : null}
      <Divider sx={{ ml: 2 }} />
    </Stack>
  );
};
export default FundGoalContributionAdjustment;
