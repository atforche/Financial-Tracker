import { Box } from "@mui/material";
import type { JSX } from "react";

/**
 * Props for the GoalRangeBar component.
 */
interface GoalRangeBarProps {
  readonly minimum: number | null;
  readonly maximum: number | null;
  readonly balance: number;
  readonly color: string;
}

/**
 * Shows a balance against its goal range on one compact scale.
 */
const GoalRangeBar = function ({
  minimum,
  maximum,
  balance,
  color,
}: GoalRangeBarProps): JSX.Element | null {
  if (minimum === null) {
    return null;
  }
  const lower = Math.min(balance, minimum);
  // Without a maximum the range is open-ended, so leave room for it rather than ending the scale at the minimum
  const upper =
    maximum === null
      ? Math.max(balance, minimum) +
        Math.max(Math.abs(balance - minimum), Math.abs(minimum), 1)
      : Math.max(balance, maximum);
  const padding = Math.max(upper - lower, Math.abs(minimum), 1) * 0.15;
  const position = (amount: number): number =>
    ((amount - lower + padding) / (upper - lower + padding * 2)) * 100;
  return (
    <Box sx={{ position: "relative", height: 14 }}>
      <Box
        sx={{
          position: "absolute",
          inset: "3px 0",
          borderRadius: 999,
          bgcolor: "action.hover",
        }}
      />
      <Box
        sx={{
          position: "absolute",
          top: 3,
          bottom: 3,
          left: `${position(minimum)}%`,
          width: `${(maximum === null ? 100 : position(maximum)) - position(minimum)}%`,
          borderRadius: 999,
          bgcolor: "success.main",
          opacity: 0.35,
        }}
      />
      <Box
        sx={{
          position: "absolute",
          left: `${position(balance)}%`,
          width: 14,
          height: 14,
          borderRadius: "50%",
          border: "2px solid",
          borderColor: "background.paper",
          bgcolor: color,
          transform: "translateX(-50%)",
        }}
      />
    </Box>
  );
};

export default GoalRangeBar;
