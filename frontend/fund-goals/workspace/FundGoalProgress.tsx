import { Box, Stack, Typography } from "@mui/material";
import {
  formatCurrency,
  getCurrencyDifference,
  getMaximumCurrencyAmount,
} from "@/framework/currencyHelpers";
import type { JSX } from "react";

/**
 * Props for the FundGoalProgress component.
 */
interface FundGoalProgressProps {
  readonly label: string;
  readonly current: number;
  readonly target: number;
  readonly satisfied: boolean;
  readonly statusDescription?: string;
}

/**
 * Displays progress toward one Fund Goal target.
 */
const FundGoalProgress = function ({
  label,
  current,
  target,
  satisfied,
  statusDescription,
}: FundGoalProgressProps): JSX.Element {
  const percent =
    target === 0
      ? satisfied
        ? 100
        : 0
      : Math.min(Math.max((current / target) * 100, 0), 100);
  return (
    <Stack spacing={0.75}>
      <Stack direction="row" justifyContent="space-between" gap={2}>
        <Typography variant="body2" fontWeight={700}>
          {label}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {formatCurrency(target)}
        </Typography>
      </Stack>
      <Box
        sx={{
          height: 8,
          overflow: "hidden",
          borderRadius: 999,
          bgcolor: "action.hover",
        }}
      >
        <Box
          sx={{
            width: `${percent}%`,
            height: "100%",
            borderRadius: 999,
            bgcolor: satisfied ? "success.main" : "primary.main",
          }}
        />
      </Box>
      <Stack direction="row" justifyContent="space-between" gap={2}>
        <Typography variant="caption" color="text.secondary">
          {formatCurrency(current)} of {formatCurrency(target)}
        </Typography>
        <Typography variant="caption" color="text.secondary">
          {statusDescription ??
            (satisfied
              ? "Satisfied"
              : `${formatCurrency(
                  getMaximumCurrencyAmount(
                    getCurrencyDifference(target, current),
                    0,
                  ),
                )} remaining`)}
        </Typography>
      </Stack>
    </Stack>
  );
};
export default FundGoalProgress;
