"use client";

/* eslint-disable sort-imports */

import BarMetricChart from "@/framework/charts/BarMetricChart";
import { buildRangeMetricChartPoints } from "@/framework/charts/chartPointHelpers";
import { getSignedChartColor } from "@/framework/charts/barMetricHelpers";
import {
  formatCompactCurrency,
  formatSignedCurrency,
} from "@/framework/currencyHelpers";
import {
  asAccountTransaction,
  asIncomeTransaction,
  asSpendingTransaction,
  type Transaction,
} from "@/transactions/types";
import dayjs from "dayjs";
import type { JSX } from "react";

interface LocationCashFlowChartProps {
  readonly transactions: readonly Transaction[];
  readonly mode: "Date" | "AccountingPeriod";
  readonly startDate?: string;
  readonly endDate?: string;
}

/** Displays money received from and paid to Locations as signed cash flow. */
const LocationCashFlowChart = function ({
  transactions,
  mode,
  startDate,
  endDate,
}: LocationCashFlowChartProps): JSX.Element {
  const totals = new Map<string, number>();
  transactions.forEach((transaction) => {
    const income = asIncomeTransaction(transaction);
    const spending = asSpendingTransaction(transaction);
    const account = asAccountTransaction(transaction);
    const amount =
      income?.source.location?.amount ??
      spending?.destinations.reduce(
        (total, destination) => total + (destination.location?.amount ?? 0),
        0,
      ) ??
      (account === null
        ? 0
        : (account.source.location?.amount ?? 0) +
          account.destinations.reduce(
            (total, destination) => total + (destination.location?.amount ?? 0),
            0,
          ));
    if (amount === 0) {
      return;
    }
    const name =
      mode === "Date" ? transaction.date : transaction.accountingPeriodName;
    totals.set(name, (totals.get(name) ?? 0) + amount);
  });
  const values =
    mode === "Date" &&
    typeof startDate === "string" &&
    typeof endDate === "string"
      ? ((): { name: string; value: number }[] => {
          const dates: { name: string; value: number }[] = [];
          let date = dayjs(startDate);
          const lastDate = dayjs(endDate);
          while (!date.isAfter(lastDate, "day")) {
            const name = date.format("YYYY-MM-DD");
            dates.push({ name, value: totals.get(name) ?? 0 });
            date = date.add(1, "day");
          }
          return dates;
        })()
      : [...totals.entries()].map(([name, value]) => ({ name, value }));
  const chartPoints = buildRangeMetricChartPoints({
    mode,
    accountingPeriods: values.map(({ name, value }) => ({ name, value })),
    dates: values.map(({ name, value }) => ({ date: name, value })),
    getColor: getSignedChartColor,
  });

  return (
    <BarMetricChart
      title="Location Cash Flow"
      emptyMessage="No incoming or outgoing money is available for this range."
      chartPoints={chartPoints}
      xAxisLabel={mode === "Date" ? "Date" : "Accounting Period"}
      yAxisLabel="Cash Flow"
      tickFormatter={(value) => formatCompactCurrency(value, true)}
      valueFormatter={formatSignedCurrency}
      getTooltipDescription={({ value }) =>
        value > 0
          ? "Incoming money from selected Locations"
          : "Outgoing money to selected Locations"
      }
      showZeroLine
    />
  );
};

export default LocationCashFlowChart;
