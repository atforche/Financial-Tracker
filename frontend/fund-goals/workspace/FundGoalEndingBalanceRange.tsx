import { Box, Stack, Typography, alpha } from "@mui/material";
import {
  FundGoalEndingBalanceStatus,
  type FundGoalProgress,
} from "@/fund-goals/types";
import {
  compareCurrencyAmounts,
  formatCurrency,
} from "@/framework/currencyHelpers";
import type { JSX } from "react";

interface FundGoalEndingBalanceRangeProps {
  readonly endingBalance: FundGoalProgress["endingBalance"];
}

/**
 * Shows the current ending balance against its configured bounds on one scale.
 */
const FundGoalEndingBalanceRange = function ({
  endingBalance,
}: FundGoalEndingBalanceRangeProps): JSX.Element {
  const current = endingBalance.endingBalance;
  const minimum = endingBalance.minimumBalance;
  const maximum = endingBalance.maximumBalance;
  const hasMaximum = maximum !== null && maximum !== undefined;
  const isExactTarget =
    hasMaximum && compareCurrencyAmounts(minimum, maximum) === 0;
  const lower = Math.min(current, minimum, maximum ?? minimum);
  const upper = Math.max(current, minimum, maximum ?? minimum);
  const spread = Math.max(
    upper - lower,
    Math.max(Math.abs(current), Math.abs(minimum), 1) * 0.25,
  );
  const padding = spread * 0.15;
  const scaleStart = lower - padding;
  const scaleWidth = upper - lower + padding * 2;
  const position = (amount: number): number =>
    ((amount - scaleStart) / scaleWidth) * 100;
  const minimumPosition = position(minimum);
  const maximumPosition = hasMaximum ? position(maximum) : 100;
  const currentPosition = position(current);
  const status =
    endingBalance.status === FundGoalEndingBalanceStatus.BelowMinimum
      ? `${formatCurrency(endingBalance.amountBelowMinimum)} below ${isExactTarget ? "target" : "minimum"}`
      : endingBalance.status === FundGoalEndingBalanceStatus.AboveMaximum
        ? `${formatCurrency(endingBalance.amountAboveMaximum)} above ${isExactTarget ? "target" : "maximum"}`
        : isExactTarget
          ? "At target"
          : !hasMaximum
            ? "Minimum met"
            : "Within target range";
  const markerColor =
    endingBalance.status === FundGoalEndingBalanceStatus.WithinRange
      ? "success.main"
      : "error.main";

  return (
    <Stack spacing={2}>
      <Stack direction="row" justifyContent="space-between" gap={2}>
        <Typography variant="body2" fontWeight={700}>
          Ending Balance
        </Typography>
        <Typography variant="body2" fontWeight={700}>
          {formatCurrency(current)}
        </Typography>
      </Stack>
      <Box
        role="img"
        aria-label={`Ending balance ${formatCurrency(current)}; ${isExactTarget ? `target ${formatCurrency(minimum)}` : `minimum ${formatCurrency(minimum)}; ${hasMaximum ? `maximum ${formatCurrency(maximum)}` : "no maximum"}`}; ${status}`}
        sx={{ position: "relative", height: 18 }}
      >
        <Box
          sx={{
            position: "absolute",
            inset: "4px 0",
            borderRadius: 999,
            bgcolor: "action.hover",
          }}
        />
        {!isExactTarget ? (
          <Box
            sx={(theme) => ({
              position: "absolute",
              top: 4,
              bottom: 4,
              left: `${minimumPosition}%`,
              width: `${maximumPosition - minimumPosition}%`,
              borderRadius: 999,
              bgcolor: alpha(theme.palette.success.main, 0.35),
            })}
          />
        ) : null}
        {[
          {
            name: isExactTarget ? "target" : "minimum",
            position: minimumPosition,
          },
          ...(!hasMaximum || isExactTarget
            ? []
            : [{ name: "maximum", position: maximumPosition }]),
        ].map((threshold) => (
          <Box
            key={threshold.name}
            sx={{
              position: "absolute",
              left: `${threshold.position}%`,
              ...(isExactTarget
                ? {
                    top: -3,
                    width: 24,
                    height: 24,
                    border: "2px solid",
                    borderColor: "text.secondary",
                    borderRadius: "50%",
                  }
                : {
                    top: -6,
                    bottom: -6,
                    width: 2,
                    bgcolor: "text.secondary",
                  }),
              transform: "translateX(-50%)",
            }}
          />
        ))}
        <Box
          sx={{
            position: "absolute",
            left: `${currentPosition}%`,
            top: 0,
            width: 18,
            height: 18,
            borderRadius: "50%",
            bgcolor: markerColor,
            border: "2px solid",
            borderColor: "background.paper",
            transform: "translateX(-50%)",
            boxShadow: 1,
          }}
        />
      </Box>
      {isExactTarget ? (
        <Typography variant="caption" color="text.secondary" textAlign="center">
          Target {formatCurrency(minimum)}
        </Typography>
      ) : (
        <Stack direction="row" justifyContent="space-between" gap={2}>
          <Typography variant="caption" color="text.secondary">
            Minimum {formatCurrency(minimum)}
          </Typography>
          <Typography
            variant="caption"
            color="text.secondary"
            textAlign="right"
          >
            {hasMaximum ? `Maximum ${formatCurrency(maximum)}` : "No maximum"}
          </Typography>
        </Stack>
      )}
    </Stack>
  );
};

export default FundGoalEndingBalanceRange;
