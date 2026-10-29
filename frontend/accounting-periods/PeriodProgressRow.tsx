import { Box, Stack, Typography } from "@mui/material";
import {
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import type { JSX } from "react";
import SignedChange from "@/framework/view/SignedChange";

/**
 * Props for the PeriodProgressRow component.
 */
interface PeriodProgressRowProps {
  readonly label: string;
  readonly actual: number;
  readonly expected: number;
  readonly isExtraGood?: boolean;
}

/**
 * Shows an amount against what is expected for the period as one bar, with anything beyond what was expected as its
 * own segment, colored as good or bad.
 */
const PeriodProgressRow = function ({
  label,
  actual,
  expected,
  isExtraGood = true,
}: PeriodProgressRowProps): JSX.Element {
  const extra = getCurrencyDifference(actual, expected);
  const extraColor = isExtraGood ? "success.main" : "error.main";
  // The bar spans whichever is larger, so anything beyond what was expected shows as its own segment
  const total = Math.max(actual, expected, 0);
  const metShare =
    total > 0 ? Math.max(Math.min(actual, expected), 0) / total : 0;
  const extraShare =
    total > 0 ? Math.min(Math.max(extra, 0) / total, 1 - metShare) : 0;
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
        <Typography variant="body2" sx={{ whiteSpace: "nowrap" }}>
          <SignedChange amount={extra} isIncreaseGood={isExtraGood} />
        </Typography>
      </Stack>
      <Box
        sx={{
          display: "flex",
          height: 12,
          borderRadius: 6,
          overflow: "hidden",
          bgcolor: "action.hover",
        }}
      >
        <Box sx={{ width: `${metShare * 100}%`, bgcolor: "primary.main" }} />
        <Box sx={{ width: `${extraShare * 100}%`, bgcolor: extraColor }} />
      </Box>
      <Typography variant="caption" color="text.secondary">
        {formatCurrency(actual)} of {formatCurrency(expected)}
      </Typography>
    </Stack>
  );
};

export default PeriodProgressRow;
