"use client";

import AccountGoalHistoryListFrame, {
  type HistoryPoint,
} from "@/account-goals/trends/AccountGoalHistoryListFrame";
import {
  Area,
  CartesianGrid,
  ComposedChart,
  Line,
  ReferenceDot,
  ResponsiveContainer,
  Tooltip,
  type TooltipContentProps,
  XAxis,
  YAxis,
} from "recharts";
import { Paper, Stack, Typography } from "@mui/material";
import {
  compareCurrencyAmounts,
  formatCompactCurrency,
  formatCurrency,
} from "@/framework/currencyHelpers";
import type { AccountGoalPeriodProgress } from "@/account-goals/trends/accountGoalProgressTrends";
import type { AccountingPeriod } from "@/accounting-periods/types";
import ChartFrame from "@/framework/charts/ChartFrame";
import type { JSX } from "react";
import { useTheme } from "@mui/material/styles";

/** Shows how one account's bounds and balance changed across periods. */
const AccountGoalHistory = function ({
  periods,
  entries,
  accountName,
}: {
  readonly periods: readonly AccountingPeriod[];
  readonly entries: readonly AccountGoalPeriodProgress[];
  readonly accountName: string;
}): JSX.Element {
  const theme = useTheme();
  const byPeriod = new Map(
    entries.map((entry) => [entry.accountGoal.accountingPeriod?.id, entry]),
  );
  const points: HistoryPoint[] = periods.map((period) => {
    const entry = byPeriod.get(period.id);
    const minimum = entry?.progress.endingBalance.minimumBalance ?? null;
    const maximum = entry?.progress.endingBalance.maximumBalance ?? null;
    return {
      periodId: period.id,
      period: period.name,
      periodOrder: period.year * 12 + period.month,
      isOpen: period.isOpen,
      minimum,
      maximum,
      span: minimum === null || maximum === null ? null : maximum - minimum,
      balance: entry?.progress.endingBalance.currentBalance ?? null,
      satisfied: entry?.progress.isSatisfied ?? null,
    };
  });
  const amounts = points.flatMap((point) =>
    [point.minimum, point.maximum, point.balance].filter(
      (amount): amount is number => amount !== null,
    ),
  );
  const padding = Math.max(1, ...amounts.map(Math.abs)) * 0.05;
  const yDomain: [number, number] = [
    Math.min(0, ...amounts) - padding,
    Math.max(0, ...amounts) + padding,
  ];
  const exactTargets = points.flatMap((point) =>
    point.minimum !== null &&
    point.maximum !== null &&
    compareCurrencyAmounts(point.minimum, point.maximum) === 0
      ? [
          {
            period: point.period,
            target: point.minimum,
            balance: point.balance,
          },
        ]
      : [],
  );

  return (
    <Stack spacing={2}>
      <Typography variant="h5">{accountName}</Typography>
      <ChartFrame
        title="Goal Progress"
        emptyMessage="No goal history for this account in the selected periods."
        hasData={entries.length > 0}
        xAxisLabel="Accounting period"
        yAxisLabel="Balance"
      >
        <ResponsiveContainer width="100%" height="100%">
          <ComposedChart
            data={points}
            margin={{ top: 16, right: 20, bottom: 4, left: 12 }}
          >
            <CartesianGrid
              strokeDasharray="4 4"
              vertical={false}
              opacity={0.24}
            />
            <XAxis dataKey="period" minTickGap={24} />
            <YAxis
              tickFormatter={(value: number) => formatCompactCurrency(value)}
              width={76}
              domain={yDomain}
            />
            <Tooltip
              content={(props: TooltipContentProps) => {
                const point =
                  typeof props.label === "string"
                    ? points.find((item) => item.period === props.label)
                    : undefined;
                if (!props.active || point === undefined) {
                  return null;
                }
                const goalRows: readonly (readonly [string, string])[] =
                  point.minimum !== null &&
                  point.maximum !== null &&
                  compareCurrencyAmounts(point.minimum, point.maximum) === 0
                    ? [["Target", formatCurrency(point.minimum)]]
                    : [
                        [
                          "Minimum",
                          point.minimum === null
                            ? "—"
                            : formatCurrency(point.minimum),
                        ],
                        [
                          "Maximum",
                          point.minimum === null
                            ? "—"
                            : point.maximum === null
                              ? "No maximum"
                              : formatCurrency(point.maximum),
                        ],
                      ];
                return (
                  <Paper
                    elevation={3}
                    sx={{
                      border: "1px solid",
                      borderColor: "divider",
                      bgcolor: "background.paper",
                      minWidth: 200,
                      p: 1.5,
                    }}
                  >
                    <Stack spacing={0.5}>
                      <Typography variant="overline" color="text.secondary">
                        {point.period}
                        {point.isOpen ? " · open" : ""}
                      </Typography>
                      {(
                        [
                          [
                            "Balance",
                            point.balance === null
                              ? "—"
                              : formatCurrency(point.balance),
                          ],
                          ...goalRows,
                        ] as const
                      ).map(([label, value]) => (
                        <Stack
                          key={label}
                          direction="row"
                          justifyContent="space-between"
                          spacing={2}
                        >
                          <Typography variant="body2" color="text.secondary">
                            {label}
                          </Typography>
                          <Typography variant="body2" fontWeight={600}>
                            {value}
                          </Typography>
                        </Stack>
                      ))}
                    </Stack>
                  </Paper>
                );
              }}
            />
            <Area
              dataKey="minimum"
              stackId="range"
              stroke="none"
              fill="transparent"
              name="Minimum"
              connectNulls={false}
              activeDot={false}
            />
            <Area
              dataKey="span"
              stackId="range"
              stroke="none"
              fill={theme.palette.success.main}
              fillOpacity={0.35}
              name="Goal range width"
              connectNulls={false}
              activeDot={false}
            />
            <Line
              dataKey="balance"
              stroke={theme.palette.primary.main}
              strokeWidth={3}
              name="Balance"
              connectNulls={false}
              activeDot={false}
            />
            <Line
              dataKey="minimum"
              stroke={theme.palette.success.main}
              strokeDasharray="5 4"
              dot={false}
              name="Minimum"
              connectNulls={false}
              activeDot={false}
            />
            <Line
              dataKey="maximum"
              stroke={theme.palette.success.main}
              strokeDasharray="5 4"
              dot={false}
              name="Maximum"
              connectNulls={false}
              activeDot={false}
            />
            {exactTargets.map((point) => (
              <ReferenceDot
                key={`${point.period}-target`}
                x={point.period}
                y={point.target}
                r={7}
                fill="none"
                stroke={theme.palette.success.main}
                strokeWidth={2}
              />
            ))}
            {exactTargets.map((point) =>
              point.balance === null ? null : (
                <ReferenceDot
                  key={`${point.period}-balance`}
                  x={point.period}
                  y={point.balance}
                  r={3}
                  fill={theme.palette.primary.main}
                  stroke="none"
                />
              ),
            )}
          </ComposedChart>
        </ResponsiveContainer>
      </ChartFrame>
      <AccountGoalHistoryListFrame points={points} />
    </Stack>
  );
};

export default AccountGoalHistory;
