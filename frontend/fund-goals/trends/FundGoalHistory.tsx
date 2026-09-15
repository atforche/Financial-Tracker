"use client";

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
import FundGoalHistoryListFrame, {
  type HistoryPoint,
} from "@/fund-goals/trends/FundGoalHistoryListFrame";
import {
  type FundGoalPeriodProgress,
  isFundGoalSatisfied,
} from "@/fund-goals/trends/fundGoalProgressTrends";
import { Paper, Stack, Typography } from "@mui/material";
import {
  compareCurrencyAmounts,
  formatCompactCurrency,
  formatCurrency,
} from "@/framework/currencyHelpers";
import type { AccountingPeriod } from "@/accounting-periods/types";
import ChartFrame from "@/framework/charts/ChartFrame";
import type { JSX } from "react";
import { useTheme } from "@mui/material/styles";

/** Shows how one fund's bounds, balance, and contributions changed across periods. */
const FundGoalHistory = function ({
  periods,
  entries,
  fundName,
}: {
  readonly periods: readonly AccountingPeriod[];
  readonly entries: readonly FundGoalPeriodProgress[];
  readonly fundName: string;
}): JSX.Element {
  const theme = useTheme();
  const byPeriod = new Map(
    entries.map((entry) => [entry.fundGoal.accountingPeriod?.id, entry]),
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
      expected: entry?.progress.contribution?.expectedAmount ?? null,
      assigned: entry?.progress.contribution?.assignedAmount ?? null,
      minimum,
      maximum,
      span: minimum === null || maximum === null ? null : maximum - minimum,
      balance: entry?.progress.endingBalance.endingBalance ?? null,
      satisfied: entry === undefined ? null : isFundGoalSatisfied(entry),
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
      ? [{ period: point.period, target: point.minimum }]
      : [],
  );
  const balanceDots = points.flatMap((point) => {
    if (point.balance === null) {
      return [];
    }
    const outsideRange =
      point.minimum !== null &&
      (compareCurrencyAmounts(point.balance, point.minimum) < 0 ||
        (point.maximum !== null &&
          compareCurrencyAmounts(point.balance, point.maximum) > 0));
    const color =
      point.minimum === null
        ? theme.palette.primary.main
        : outsideRange
          ? theme.palette.error.main
          : theme.palette.success.main;
    return [{ period: point.period, balance: point.balance, color }];
  });

  return (
    <Stack spacing={2}>
      <Typography variant="h5">{fundName}</Typography>
      <ChartFrame
        title="Goal Progress"
        emptyMessage="No goal history for this fund in the selected periods."
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
                const matchesMinimum =
                  point.balance !== null &&
                  point.minimum !== null &&
                  compareCurrencyAmounts(point.balance, point.minimum) === 0;
                const matchesMaximum =
                  point.balance !== null &&
                  point.maximum !== null &&
                  compareCurrencyAmounts(point.balance, point.maximum) === 0;
                const matchLabel =
                  matchesMinimum && matchesMaximum
                    ? "At target"
                    : matchesMinimum
                      ? "At minimum"
                      : matchesMaximum
                        ? "At maximum"
                        : null;
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
                          ...(matchLabel === null
                            ? []
                            : ([["Balance position", matchLabel]] as const)),
                          ...(point.expected === null
                            ? []
                            : ([
                                [
                                  "Contribution",
                                  `${formatCurrency(point.assigned ?? 0)} / ${formatCurrency(point.expected)}`,
                                ],
                              ] as const)),
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
              fillOpacity={0.16}
              name="Goal range width"
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
            <Line
              dataKey="balance"
              stroke={theme.palette.primary.main}
              strokeWidth={3}
              name="Balance"
              connectNulls={false}
              dot={false}
              activeDot={false}
            />
            {exactTargets.map((point) => (
              <ReferenceDot
                key={`${point.period}-target`}
                x={point.period}
                y={point.target}
                shape={({ cx, cy }) => (
                  <line
                    x1={(cx ?? 0) - 8}
                    x2={(cx ?? 0) + 8}
                    y1={cy}
                    y2={cy}
                    stroke={theme.palette.success.main}
                    strokeWidth={3}
                  />
                )}
              />
            ))}
            {balanceDots.map((point) => (
              <ReferenceDot
                key={`${point.period}-balance`}
                x={point.period}
                y={point.balance}
                r={5}
                fill={point.color}
                stroke="none"
              />
            ))}
          </ComposedChart>
        </ResponsiveContainer>
      </ChartFrame>
      <FundGoalHistoryListFrame points={points} />
    </Stack>
  );
};

export default FundGoalHistory;
