import { Box, Stack, Typography } from "@mui/material";
import type { JSX } from "react";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the FundPlanProgress component.
 */
interface FundPlanProgressProps {
  readonly label: string;
  readonly current: number;
  readonly target: number;
  readonly satisfied: boolean;
  readonly percent?: number;
  readonly currentDescription?: string;
  readonly statusDescription?: string;
}

/**
 * Displays progress toward one Funding Plan target.
 */
const FundPlanProgress = function ({
  label,
  current,
  target,
  satisfied,
  percent: percentOverride,
  currentDescription,
  statusDescription,
}: FundPlanProgressProps): JSX.Element {
  const percent =
    percentOverride ??
    (target === 0 ? 100 : Math.min(Math.max((current / target) * 100, 0), 100));
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
          {currentDescription ??
            `${formatCurrency(current)} of ${formatCurrency(target)}`}
        </Typography>
        <Typography variant="caption" color="text.secondary">
          {statusDescription ??
            (satisfied
              ? "Satisfied"
              : `${formatCurrency(Math.max(target - current, 0))} remaining`)}
        </Typography>
      </Stack>
    </Stack>
  );
};
export default FundPlanProgress;
