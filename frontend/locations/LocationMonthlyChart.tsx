"use client";

import {
  formatCompactCurrency,
  formatCurrency,
} from "@/framework/currencyHelpers";
import BarMetricChart from "@/framework/charts/BarMetricChart";
import type { JSX } from "react";
import type { LocationDetailRange } from "@/locations/types";
import dayjs from "dayjs";

/**
 * Charts how much was spent at a location in each month of the range.
 */
const LocationMonthlyChart = function ({
  months,
}: {
  readonly months: LocationDetailRange["months"];
}): JSX.Element {
  return (
    <BarMetricChart
      title="Spending by Month"
      variant="surface"
      emptyMessage="Nothing was spent here in this range."
      hasData={months.some((month) => month.spent !== 0)}
      chartPoints={months.map((month) => {
        const date = dayjs(new Date(month.year, month.month - 1, 1));
        return {
          tickLabel: date.format("MMM YY"),
          tooltipLabel: date.format("MMMM YYYY"),
          value: month.spent,
        };
      })}
      xAxisLabel="Month"
      yAxisLabel="Spent"
      tickFormatter={(value) => formatCompactCurrency(value)}
      valueFormatter={formatCurrency}
    />
  );
};

export default LocationMonthlyChart;
