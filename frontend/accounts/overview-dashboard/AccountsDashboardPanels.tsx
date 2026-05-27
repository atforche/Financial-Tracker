import {
  type AccountDashboardAccount,
  type AccountTypeBalance,
  formatAccountType,
  isPositiveChangeInBalance,
} from "@/accounts/types";
import { Box, Chip, Divider, Paper, Stack, Typography } from "@mui/material";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";

/**
 * Point in the balance trend visualization.
 */
interface AccountBalanceTrendPoint {
  readonly label: string;
  readonly totalBalance: number;
  readonly trackedBalance: number;
  readonly untrackedBalance: number;
}

/**
 * Props for the AccountBalanceTrendPanel component.
 */
interface AccountBalanceTrendPanelProps {
  readonly points: readonly AccountBalanceTrendPoint[];
  readonly modeLabel: string;
}

/**
 * Props for the AccountTypeComparisonPanel component.
 */
interface AccountTypeComparisonPanelProps {
  readonly startLabel: string;
  readonly endLabel: string;
  readonly startingBalances: readonly AccountTypeBalance[];
  readonly endingBalances: readonly AccountTypeBalance[];
}

/**
 * Props for the AccountLargestMoversPanel component.
 */
interface AccountLargestMoversPanelProps {
  readonly accounts: readonly AccountDashboardAccount[];
}

const chartWidth = 640;
const chartHeight = 220;
const chartPadding = 20;

const formatShortCurrency = function (value: number): string {
  const absoluteValue = Math.abs(value);
  if (absoluteValue >= 1_000_000) {
    return `${value < 0 ? "-" : ""}$${(absoluteValue / 1_000_000).toFixed(1)}M`;
  }
  if (absoluteValue >= 1_000) {
    return `${value < 0 ? "-" : ""}$${(absoluteValue / 1_000).toFixed(1)}K`;
  }
  return formatCurrency(value);
};

const buildLinePath = function (
  values: readonly number[],
  minimumValue: number,
  maximumValue: number,
): string {
  if (values.length === 0) {
    return "";
  }

  const usableWidth = chartWidth - chartPadding * 2;
  const usableHeight = chartHeight - chartPadding * 2;
  const denominator = values.length === 1 ? 1 : values.length - 1;
  const range = maximumValue - minimumValue || 1;

  return values
    .map((value, index) => {
      const x = chartPadding + (usableWidth * index) / denominator;
      const y =
        chartPadding +
        usableHeight -
        ((value - minimumValue) / range) * usableHeight;
      return `${index === 0 ? "M" : "L"}${x.toFixed(2)} ${y.toFixed(2)}`;
    })
    .join(" ");
};

/**
 * Shows total, tracked, and untracked balances across the selected dashboard range.
 */
const AccountBalanceTrendPanel = function ({
  points,
  modeLabel,
}: AccountBalanceTrendPanelProps): JSX.Element {
  const series = [
    {
      color: "#1f7a4d",
      label: "Total balance",
      values: points.map((point) => point.totalBalance),
    },
    {
      color: "#0d5ea6",
      label: "Tracked",
      values: points.map((point) => point.trackedBalance),
    },
    {
      color: "#b45309",
      label: "Untracked",
      values: points.map((point) => point.untrackedBalance),
    },
  ];
  const allValues = series.flatMap((entry) => entry.values);
  const minimumValue = Math.min(...allValues, 0);
  const maximumValue = Math.max(...allValues, 0);
  const labelIndexes = [
    0,
    Math.floor((points.length - 1) / 2),
    points.length - 1,
  ].filter(
    (value, index, array) => value >= 0 && array.indexOf(value) === index,
  );

  return (
    <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
      <Stack spacing={2.5}>
        <Stack spacing={0.5}>
          <Typography variant="h5">Balance trend</Typography>
          <Typography variant="body2" color="text.secondary">
            {points.length <= 1
              ? `The ${modeLabel.toLowerCase()} range is currently scoped to a single point in time.`
              : `Track how total, tracked, and untracked balances move across the selected ${modeLabel.toLowerCase()} range.`}
          </Typography>
        </Stack>
        {points.length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            No trend data is available for the current dashboard selection.
          </Typography>
        ) : (
          <>
            <Stack direction="row" spacing={1.5} flexWrap="wrap" useFlexGap>
              {series.map((entry) => (
                <Stack
                  key={entry.label}
                  direction="row"
                  spacing={1}
                  alignItems="center"
                >
                  <Box
                    sx={{
                      width: 12,
                      height: 12,
                      borderRadius: "50%",
                      bgcolor: entry.color,
                    }}
                  />
                  <Typography variant="body2" color="text.secondary">
                    {entry.label}
                  </Typography>
                </Stack>
              ))}
            </Stack>
            <Box sx={{ overflowX: "auto" }}>
              <Box sx={{ minWidth: 480 }}>
                <svg
                  viewBox={`0 0 ${chartWidth} ${chartHeight}`}
                  width="100%"
                  height="220"
                  role="img"
                >
                  <line
                    x1={chartPadding}
                    y1={chartHeight / 2}
                    x2={chartWidth - chartPadding}
                    y2={chartHeight / 2}
                    stroke="rgba(0, 0, 0, 0.08)"
                    strokeDasharray="4 4"
                  />
                  {series.map((entry) => (
                    <path
                      key={entry.label}
                      d={buildLinePath(
                        entry.values,
                        minimumValue,
                        maximumValue,
                      )}
                      fill="none"
                      stroke={entry.color}
                      strokeWidth="3"
                      strokeLinecap="round"
                      strokeLinejoin="round"
                    />
                  ))}
                </svg>
                <Stack direction="row" justifyContent="space-between" gap={2}>
                  {labelIndexes.map((index) => (
                    <Stack
                      key={`${points[index]?.label}-${index}`}
                      spacing={0.25}
                      sx={{ minWidth: 0 }}
                    >
                      <Typography variant="caption" color="text.secondary">
                        {points[index]?.label}
                      </Typography>
                      <Typography variant="caption" fontWeight={700}>
                        {formatShortCurrency(points[index]?.totalBalance ?? 0)}
                      </Typography>
                    </Stack>
                  ))}
                </Stack>
              </Box>
            </Box>
          </>
        )}
      </Stack>
    </Paper>
  );
};

/**
 * Compares account-type balances at the beginning and end of the selected range.
 */
const AccountTypeComparisonPanel = function ({
  startLabel,
  endLabel,
  startingBalances,
  endingBalances,
}: AccountTypeComparisonPanelProps): JSX.Element {
  const startingMap = new Map(
    startingBalances.map((balance) => [
      balance.accountType,
      balance.totalBalance,
    ]),
  );
  const endingMap = new Map(
    endingBalances.map((balance) => [
      balance.accountType,
      balance.totalBalance,
    ]),
  );
  const accountTypes = Array.from(
    new Set([...startingMap.keys(), ...endingMap.keys()]),
  );
  const rows = accountTypes
    .map((accountType) => {
      const startingBalance = startingMap.get(accountType) ?? 0;
      const endingBalance = endingMap.get(accountType) ?? 0;
      return {
        accountType,
        startingBalance,
        endingBalance,
        changeInBalance: endingBalance - startingBalance,
      };
    })
    .sort(
      (left, right) =>
        Math.abs(right.endingBalance) - Math.abs(left.endingBalance),
    );
  const maxBalance = Math.max(
    ...rows.flatMap((row) => [
      Math.abs(row.startingBalance),
      Math.abs(row.endingBalance),
    ]),
    0,
  );

  return (
    <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
      <Stack spacing={2.5}>
        <Stack spacing={0.5}>
          <Typography variant="h5">Type balance comparison</Typography>
          <Typography variant="body2" color="text.secondary">
            Compare where balance exposure started versus where it finished
            across each account type.
          </Typography>
        </Stack>
        {rows.length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            No account-type composition is available for this dashboard range.
          </Typography>
        ) : (
          <Stack spacing={2} divider={<Divider flexItem />}>
            {rows.map((row) => (
              <Stack key={row.accountType} spacing={1.25}>
                <Stack direction="row" justifyContent="space-between" gap={2}>
                  <Typography variant="body1">
                    {formatAccountType(row.accountType)}
                  </Typography>
                  <Chip
                    label={formatCurrency(row.changeInBalance)}
                    color={row.changeInBalance >= 0 ? "success" : "error"}
                    size="small"
                    variant="outlined"
                  />
                </Stack>
                <Stack spacing={0.75}>
                  <Typography variant="caption" color="text.secondary">
                    {startLabel}
                  </Typography>
                  <Box sx={{ display: "flex", alignItems: "center", gap: 1.5 }}>
                    <Box
                      sx={{
                        height: 8,
                        borderRadius: 999,
                        bgcolor: "grey.300",
                        width: `${maxBalance === 0 ? 0 : (Math.abs(row.startingBalance) / maxBalance) * 100}%`,
                        minWidth: row.startingBalance === 0 ? 0 : 12,
                      }}
                    />
                    <Typography variant="body2" fontWeight={600}>
                      {formatCurrency(row.startingBalance)}
                    </Typography>
                  </Box>
                </Stack>
                <Stack spacing={0.75}>
                  <Typography variant="caption" color="text.secondary">
                    {endLabel}
                  </Typography>
                  <Box sx={{ display: "flex", alignItems: "center", gap: 1.5 }}>
                    <Box
                      sx={{
                        height: 8,
                        borderRadius: 999,
                        bgcolor: "success.light",
                        width: `${maxBalance === 0 ? 0 : (Math.abs(row.endingBalance) / maxBalance) * 100}%`,
                        minWidth: row.endingBalance === 0 ? 0 : 12,
                      }}
                    />
                    <Typography variant="body2" fontWeight={600}>
                      {formatCurrency(row.endingBalance)}
                    </Typography>
                  </Box>
                </Stack>
              </Stack>
            ))}
          </Stack>
        )}
      </Stack>
    </Paper>
  );
};

/**
 * Highlights the largest balance movers in the current dashboard page.
 */
const AccountLargestMoversPanel = function ({
  accounts,
}: AccountLargestMoversPanelProps): JSX.Element {
  const movers = [...accounts]
    .map((account) => ({
      ...account,
      changeInBalance: account.endingBalance - account.startingBalance,
    }))
    .sort(
      (left, right) =>
        Math.abs(right.changeInBalance) - Math.abs(left.changeInBalance),
    )
    .slice(0, 5);
  const maxChange = Math.max(
    ...movers.map((mover) => Math.abs(mover.changeInBalance)),
    0,
  );

  return (
    <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
      <Stack spacing={2.5}>
        <Stack spacing={0.5}>
          <Typography variant="h5">Largest balance shifts</Typography>
          <Typography variant="body2" color="text.secondary">
            Review the biggest opening-to-ending balance changes on the current
            dashboard page.
          </Typography>
        </Stack>
        {movers.length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            No account movements are available for this page.
          </Typography>
        ) : (
          <Stack spacing={1.75}>
            {movers.map((account) => {
              const isPositive = isPositiveChangeInBalance(
                account.type,
                account.changeInBalance,
              );

              return (
                <Stack key={account.id} spacing={0.75}>
                  <Stack direction="row" justifyContent="space-between" gap={2}>
                    <Stack spacing={0.25} sx={{ minWidth: 0 }}>
                      <Typography variant="body2" fontWeight={700} noWrap>
                        {account.name}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {formatAccountType(account.type)}
                      </Typography>
                    </Stack>
                    <Typography
                      variant="body2"
                      fontWeight={700}
                      color={isPositive ? "success.main" : "error.main"}
                    >
                      {formatCurrency(account.changeInBalance)}
                    </Typography>
                  </Stack>
                  <Box
                    sx={{
                      height: 8,
                      borderRadius: 999,
                      bgcolor: isPositive ? "success.light" : "error.light",
                      width: `${maxChange === 0 ? 0 : (Math.abs(account.changeInBalance) / maxChange) * 100}%`,
                      minWidth: account.changeInBalance === 0 ? 0 : 12,
                    }}
                  />
                  <Typography variant="caption" color="text.secondary">
                    {formatCurrency(account.startingBalance)} to{" "}
                    {formatCurrency(account.endingBalance)}
                  </Typography>
                </Stack>
              );
            })}
          </Stack>
        )}
      </Stack>
    </Paper>
  );
};

export {
  type AccountBalanceTrendPoint,
  AccountBalanceTrendPanel,
  AccountLargestMoversPanel,
  AccountTypeComparisonPanel,
};
