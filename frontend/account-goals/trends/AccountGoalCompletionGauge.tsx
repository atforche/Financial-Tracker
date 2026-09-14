"use client";

import { Box, Typography } from "@mui/material";
import type { JSX } from "react";
import { useTheme } from "@mui/material/styles";

/** Circular gauge for the share of goal-period results achieved. */
const AccountGoalCompletionGauge = function ({
  percentage,
}: {
  readonly percentage: number | null;
}): JSX.Element {
  const theme = useTheme();
  const clamped = Math.min(100, Math.max(0, percentage ?? 0));
  const isComplete = percentage !== null && clamped === 100;
  const displayedPercentage = isComplete
    ? "100"
    : clamped >= 99.5
      ? (Math.floor(clamped * 10) / 10).toFixed(1)
      : clamped > 0 && clamped < 0.5
        ? "<1"
        : Math.round(clamped).toString();
  return (
    <Box
      sx={{ position: "relative", width: 176, maxWidth: "100%", mx: "auto" }}
    >
      <svg
        viewBox="0 0 176 176"
        width="100%"
        style={{ display: "block" }}
        role="img"
        aria-label={
          percentage === null
            ? "No goal results"
            : `${displayedPercentage} percent of goal-period results achieved`
        }
      >
        <circle
          cx="88"
          cy="88"
          r="72"
          pathLength="100"
          fill="none"
          stroke={
            percentage === null
              ? theme.palette.divider
              : isComplete
                ? theme.palette.success.main
                : theme.palette.error.main
          }
          strokeWidth="14"
        />
        {percentage === null || isComplete ? null : (
          <circle
            cx="88"
            cy="88"
            r="72"
            pathLength="100"
            fill="none"
            stroke={theme.palette.success.main}
            strokeWidth="14"
            strokeDasharray={`${clamped} 100`}
            transform="rotate(-90 88 88)"
          />
        )}
      </svg>
      <Typography
        variant="h4"
        component="div"
        sx={{
          position: "absolute",
          left: 0,
          right: 0,
          top: "50%",
          textAlign: "center",
          transform: "translateY(-50%)",
        }}
      >
        {percentage === null ? "—" : `${displayedPercentage}%`}
      </Typography>
    </Box>
  );
};

export default AccountGoalCompletionGauge;
