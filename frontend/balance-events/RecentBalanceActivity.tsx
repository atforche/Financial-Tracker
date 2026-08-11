"use client";

import {
  type BalanceTrendDateSummary,
  buildDateChartPoints,
} from "@/framework/charts/balanceTrendHelpers";
import BalanceTrendChart from "@/framework/charts/BalanceTrendChart";
import { Button } from "@mui/material";
import ChangeValue from "@/framework/view/ChangeValue";
import type { JSX } from "react";
import Link from "next/link";
import PageLayout from "@/framework/view/PageLayout";
import type { Route } from "next";
import SummaryCard from "@/framework/view/SummaryCard";
import SummaryCardGrid from "@/framework/view/SummaryCardGrid";
import dayjs from "dayjs";
import { formatCurrency } from "@/framework/currencyHelpers";
import { formatLongDate } from "@/framework/dateHelpers";

/**
 * The balance information needed to visualize a recent balance event.
 */
interface RecentBalanceActivityEvent {
  readonly description: string;
  readonly eventDate?: string | null;
  readonly eventDateSequence?: number | null;
  readonly isPosted: boolean;
  readonly transactionDate: string;
}

/**
 * Props for the RecentBalanceActivity component.
 */
interface RecentBalanceActivityProps<T extends RecentBalanceActivityEvent> {
  readonly data: readonly T[];
  readonly getPreviousBalance: (event: T) => number;
  readonly getNewBalance: (event: T) => number;
  readonly dailyBalances?: readonly BalanceTrendDateSummary[];
  readonly title?: string;
  readonly balanceLabel?: string;
  readonly trendsHref?: Route;
}

/**
 * Shows a recent balance trend and its most useful movement summaries.
 */
const RecentBalanceActivity = function <T extends RecentBalanceActivityEvent>({
  data,
  getPreviousBalance,
  getNewBalance,
  dailyBalances,
  title = "Recent Activity",
  balanceLabel = "Posted Balance",
  trendsHref,
}: RecentBalanceActivityProps<T>): JSX.Element {
  const postedEvents = data
    .filter((event) => event.isPosted && typeof event.eventDate === "string")
    .toSorted((left, right) => {
      const dateComparison = (
        left.eventDate ?? left.transactionDate
      ).localeCompare(right.eventDate ?? right.transactionDate);
      return dateComparison === 0
        ? (left.eventDateSequence ?? 0) - (right.eventDateSequence ?? 0)
        : dateComparison;
    });
  const [firstEvent] = postedEvents;
  const lastEvent = postedEvents.at(-1);
  const eventChanges = postedEvents.map((event) => ({
    event,
    amount: getNewBalance(event) - getPreviousBalance(event),
  }));
  const dailyChanges = (dailyBalances ?? []).flatMap(
    (summary, index, balances) => {
      const previous = balances[index - 1];
      return previous === undefined
        ? []
        : [{ amount: summary.totalBalance - previous.totalBalance }];
    },
  );
  const changes =
    typeof dailyBalances === "undefined" ? eventChanges : dailyChanges;
  const totalInflow = changes.reduce(
    (total, change) => total + Math.max(change.amount, 0),
    0,
  );
  const totalOutflow = changes.reduce(
    (total, change) => total + Math.abs(Math.min(change.amount, 0)),
    0,
  );
  const firstBalance =
    typeof dailyBalances === "undefined"
      ? firstEvent === undefined
        ? undefined
        : getPreviousBalance(firstEvent)
      : dailyBalances[0]?.totalBalance;
  const lastBalance =
    typeof dailyBalances === "undefined"
      ? lastEvent === undefined
        ? undefined
        : getNewBalance(lastEvent)
      : dailyBalances.at(-1)?.totalBalance;
  const activityDescription =
    typeof dailyBalances === "undefined"
      ? "Across posted events in this recent window."
      : "Across daily balances in this recent window.";
  const getDateLabel = (event: T): string => {
    const date = event.eventDate ?? event.transactionDate;
    return formatLongDate(new Date(`${date}T00:00:00`));
  };
  const eventChartPoints = [
    ...(firstEvent === undefined
      ? []
      : [
          {
            tickLabel: dayjs(
              firstEvent.eventDate ?? firstEvent.transactionDate,
            ).format("MMM D"),
            tooltipLabel: `Opening balance before ${getDateLabel(firstEvent)}`,
            balance: getPreviousBalance(firstEvent),
          },
        ]),
    ...postedEvents.map((event) => ({
      tickLabel: dayjs(event.eventDate ?? event.transactionDate).format(
        "MMM D",
      ),
      tooltipLabel: `${getDateLabel(event)} · Event ${event.eventDateSequence ?? 0}`,
      balance: getNewBalance(event),
      description: event.description,
    })),
  ];
  const chartPoints =
    typeof dailyBalances === "undefined"
      ? eventChartPoints
      : buildDateChartPoints(dailyBalances);

  return (
    <PageLayout>
      <BalanceTrendChart
        title={title}
        headerContent={
          trendsHref === undefined ? undefined : (
            <Button
              component={Link}
              href={trendsHref}
              size="small"
              variant="outlined"
            >
              View full trends
            </Button>
          )
        }
        emptyMessage="No posted balance activity is available in this recent window."
        chartPoints={chartPoints}
        xAxisLabel="Date"
        yAxisLabel={balanceLabel}
      />
      <SummaryCardGrid>
        <SummaryCard
          title="Net Change"
          value={
            firstBalance === undefined || lastBalance === undefined ? (
              "—"
            ) : (
              <ChangeValue
                startingValue={firstBalance}
                endingValue={lastBalance}
              />
            )
          }
          description={activityDescription}
        />
        <SummaryCard
          title="Total Inflow"
          value={formatCurrency(totalInflow)}
          description={activityDescription}
        />
        <SummaryCard
          title="Total Outflow"
          value={formatCurrency(totalOutflow)}
          description={activityDescription}
        />
      </SummaryCardGrid>
    </PageLayout>
  );
};

export default RecentBalanceActivity;
