import { Divider, Stack, Typography } from "@mui/material";
import type { JSX } from "react";
import { formatCurrency } from "@/framework/currencyHelpers";

interface FundGoalContributionAdjustmentProps {
  readonly plannedAmount: number;
  readonly adjustmentAmount: number;
}

/**
 * Displays how a planned contribution was adjusted to its expected amount.
 */
const FundGoalContributionAdjustment = function ({
  plannedAmount,
  adjustmentAmount,
}: FundGoalContributionAdjustmentProps): JSX.Element {
  const hasAdjustment = adjustmentAmount > 0;

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
            −{formatCurrency(adjustmentAmount)}
          </Typography>
        </Stack>
      ) : null}
      <Divider sx={{ ml: 2 }} />
    </Stack>
  );
};
export default FundGoalContributionAdjustment;
