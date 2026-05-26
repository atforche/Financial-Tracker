import { Box, Paper, Stack, Typography } from "@mui/material";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { JSX } from "react";
import dayjs from "dayjs";
import formatCurrency from "@/framework/formatCurrency";

/**
 * Props for the AccountingPeriodBalanceTrendCard component.
 */
interface AccountingPeriodBalanceTrendCardProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly selectedRange: number;
}

interface Point {
  readonly x: number;
  readonly y: number;
}

const chartWidth = 720;
const chartHeight = 240;
const chartPadding = {
  top: 24,
  right: 18,
  bottom: 34,
  left: 18,
};

/**
 * Builds chart points for the supplied accounting periods.
 */
const buildPoints = function (accountingPeriods: AccountingPeriod[]): Point[] {
  if (accountingPeriods.length === 1) {
    return [
      {
        x: chartWidth / 2,
        y: (chartHeight - chartPadding.bottom + chartPadding.top) / 2,
      },
    ];
  }

  const closingBalances = accountingPeriods.map(
    (accountingPeriod: AccountingPeriod) => accountingPeriod.closingBalance,
  );
  const minimumBalance = Math.min(...closingBalances);
  const maximumBalance = Math.max(...closingBalances);
  const balanceRange = Math.max(maximumBalance - minimumBalance, 1);
  const usableWidth = chartWidth - chartPadding.left - chartPadding.right;
  const usableHeight = chartHeight - chartPadding.top - chartPadding.bottom;

  return accountingPeriods.map(
    (accountingPeriod: AccountingPeriod, index: number): Point => ({
      x:
        chartPadding.left +
        (usableWidth * index) / (accountingPeriods.length - 1),
      y:
        chartPadding.top +
        usableHeight -
        ((accountingPeriod.closingBalance - minimumBalance) / balanceRange) *
          usableHeight,
    }),
  );
};

/**
 * Renders a lightweight balance trend chart for accounting periods.
 */
const AccountingPeriodBalanceTrendCard = function ({
  accountingPeriods,
  selectedRange,
}: AccountingPeriodBalanceTrendCardProps): JSX.Element {
  const hasPeriods = accountingPeriods.length > 0;

  if (!hasPeriods) {
    return (
      <Paper
        sx={{
          border: "1px solid",
          borderColor: "divider",
          p: 3,
        }}
      >
        <Stack spacing={1}>
          <Typography variant="h5">Balance trend</Typography>
          <Typography variant="body2" color="text.secondary">
            Add an accounting period to start charting closing balances over
            time.
          </Typography>
        </Stack>
      </Paper>
    );
  }

  const points = buildPoints(accountingPeriods);
  const linePath = points
    .map(
      (point: Point, index: number): string =>
        `${index === 0 ? "M" : "L"} ${point.x} ${point.y}`,
    )
    .join(" ");
  const firstPoint = points.at(0);
  const lastPoint = points.at(-1);
  const earliestPeriod = accountingPeriods.at(0);
  const latestPeriod = accountingPeriods.at(-1);

  if (
    typeof firstPoint === "undefined" ||
    typeof lastPoint === "undefined" ||
    typeof earliestPeriod === "undefined" ||
    typeof latestPeriod === "undefined"
  ) {
    throw new Error("Failed to build accounting period balance trend");
  }

  const areaPath = `${linePath} L ${lastPoint.x} ${chartHeight - chartPadding.bottom} L ${firstPoint.x} ${chartHeight - chartPadding.bottom} Z`;
  const netChange = latestPeriod.closingBalance - earliestPeriod.closingBalance;

  return (
    <Paper
      sx={{
        border: "1px solid",
        borderColor: "divider",
        p: 3,
      }}
    >
      <Stack spacing={2}>
        <Stack
          direction={{ xs: "column", md: "row" }}
          justifyContent="space-between"
          spacing={1}
        >
          <Stack spacing={0.5}>
            <Typography variant="h5">Balance trend</Typography>
            <Typography variant="body2" color="text.secondary">
              Closing balance across the last {selectedRange} accounting
              periods.
            </Typography>
          </Stack>
          <Stack alignItems={{ xs: "flex-start", md: "flex-end" }}>
            <Typography variant="overline" color="text.secondary">
              Window change
            </Typography>
            <Typography
              variant="h6"
              color={netChange >= 0 ? "success.main" : "error.main"}
            >
              {netChange >= 0 ? "+" : "-"}
              {formatCurrency(Math.abs(netChange))}
            </Typography>
          </Stack>
        </Stack>
        <Box>
          <Box
            component="svg"
            viewBox={`0 0 ${chartWidth} ${chartHeight}`}
            sx={{ width: "100%", height: "auto", display: "block" }}
          >
            <defs>
              <linearGradient
                id="accounting-period-balance-fill"
                x1="0"
                x2="0"
                y1="0"
                y2="1"
              >
                <stop offset="0%" stopColor="rgba(0, 150, 136, 0.32)" />
                <stop offset="100%" stopColor="rgba(0, 150, 136, 0.02)" />
              </linearGradient>
            </defs>
            <line
              x1={chartPadding.left}
              y1={chartHeight - chartPadding.bottom}
              x2={chartWidth - chartPadding.right}
              y2={chartHeight - chartPadding.bottom}
              stroke="currentColor"
              opacity="0.18"
            />
            <path d={areaPath} fill="url(#accounting-period-balance-fill)" />
            <path
              d={linePath}
              fill="none"
              stroke="currentColor"
              strokeWidth="3"
              vectorEffect="non-scaling-stroke"
            />
            {points.map((point: Point, index: number) => {
              const accountingPeriod = accountingPeriods[index];
              if (typeof accountingPeriod === "undefined") {
                return null;
              }

              return (
                <g key={accountingPeriod.id}>
                  <circle
                    cx={point.x}
                    cy={point.y}
                    r={5}
                    fill="currentColor"
                    opacity={index === points.length - 1 ? 1 : 0.75}
                  />
                  <text
                    x={point.x}
                    y={chartHeight - 10}
                    textAnchor={
                      index === 0
                        ? "start"
                        : index === points.length - 1
                          ? "end"
                          : "middle"
                    }
                    fontSize="12"
                    fill="currentColor"
                    opacity="0.7"
                  >
                    {dayjs(accountingPeriod.name, "MMMM YYYY").format("MMM YY")}
                  </text>
                </g>
              );
            })}
          </Box>
        </Box>
        <Stack direction={{ xs: "column", md: "row" }} spacing={2}>
          <Box sx={{ flex: 1 }}>
            <Typography variant="overline" color="text.secondary">
              Starting point
            </Typography>
            <Typography variant="body1">{earliestPeriod.name}</Typography>
            <Typography variant="body2" color="text.secondary">
              {formatCurrency(earliestPeriod.closingBalance)} closing balance
            </Typography>
          </Box>
          <Box sx={{ flex: 1 }}>
            <Typography variant="overline" color="text.secondary">
              Latest point
            </Typography>
            <Typography variant="body1">{latestPeriod.name}</Typography>
            <Typography variant="body2" color="text.secondary">
              {formatCurrency(latestPeriod.closingBalance)} closing balance
            </Typography>
          </Box>
        </Stack>
      </Stack>
    </Paper>
  );
};

export default AccountingPeriodBalanceTrendCard;
