import type { JSX, ReactNode } from "react";
import { Stack, Typography } from "@mui/material";
import {
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import type { BalanceEventTotals } from "@/balance-events/types";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import ReturnLink from "@/framework/view/ReturnLink";
import type { Route } from "next";
import SignedChange from "@/framework/view/SignedChange";
import StatCard from "@/framework/view/StatCard";
import UrlTabs from "@/framework/view/UrlTabs";
import { periodTab } from "@/balance-events/resolveEntityHistory";

/**
 * Balances and activity across the selected range, or null when there is nothing to summarize.
 */
interface EntityHistorySummary {
  readonly openingBalance: number;
  readonly closingBalance: number;
  readonly totals: Pick<BalanceEventTotals, "totalInflow" | "totalOutflow">;
  readonly goalsMet?: { readonly met: number; readonly total: number };
}

/**
 * Props for the EntityHistoryLayout component.
 */
interface EntityHistoryLayoutProps {
  readonly entityName: "account" | "fund";
  readonly name: string;
  readonly subtitle: string | null;
  readonly back: { readonly href: Route; readonly label: string };
  readonly isIncreaseGood?: boolean;
  readonly actions: ReactNode;
  readonly view: "date" | "period";
  readonly dateFilter: ReactNode;
  readonly periodFilter: ReactNode;
  readonly summary: EntityHistorySummary | null;
  readonly children: ReactNode;
}

/**
 * Lays out one account or fund across a range: its history by date or by accounting period.
 */
const EntityHistoryLayout = function ({
  entityName,
  name,
  subtitle,
  back,
  isIncreaseGood = true,
  actions,
  view,
  dateFilter,
  periodFilter,
  summary,
  children,
}: EntityHistoryLayoutProps): JSX.Element {
  const content = (
    <Stack spacing={2}>
      {summary === null ? null : (
        <ResponsiveGrid
          columns={{
            xs: 1,
            sm: 2,
            lg: summary.goalsMet === undefined ? 4 : 5,
          }}
          spacing={2}
        >
          <StatCard
            label={view === "date" ? "Starting balance" : "Opening balance"}
            value={formatCurrency(summary.openingBalance)}
            caption="at the start of the range"
          />
          <StatCard
            label="Total inflow"
            value={formatCurrency(summary.totals.totalInflow)}
            caption="posted in this range"
          />
          <StatCard
            label="Total outflow"
            value={formatCurrency(summary.totals.totalOutflow)}
            caption="posted in this range"
          />
          <StatCard
            label={view === "date" ? "Ending balance" : "Closing balance"}
            value={formatCurrency(summary.closingBalance)}
            caption={
              <>
                <SignedChange
                  amount={getCurrencyDifference(
                    summary.closingBalance,
                    summary.openingBalance,
                  )}
                  isIncreaseGood={isIncreaseGood}
                />{" "}
                over this range
              </>
            }
          />
          {summary.goalsMet === undefined ? null : (
            <StatCard
              label="Goals met"
              value={`${summary.goalsMet.met.toString()} of ${summary.goalsMet.total.toString()}`}
              caption="periods in this range"
            />
          )}
        </ResponsiveGrid>
      )}
      {children}
    </Stack>
  );

  return (
    <PageLayout>
      <Stack spacing={2}>
        <ReturnLink returnUrl={undefined} fallback={back} />
        <Stack
          direction={{ xs: "column", sm: "row" }}
          spacing={2}
          sx={{
            alignItems: { xs: "stretch", sm: "flex-start" },
            justifyContent: "space-between",
          }}
        >
          <Stack spacing={0.5} sx={{ minWidth: 0 }}>
            <Typography variant="h4" noWrap>
              {name}
            </Typography>
            {subtitle === null ? null : (
              <Typography variant="subtitle1" color="text.secondary">
                {subtitle}
              </Typography>
            )}
          </Stack>
          {actions}
        </Stack>
      </Stack>
      <UrlTabs
        label={`${entityName === "account" ? "Account" : "Fund"} views`}
        paramName="tab"
        tabs={[
          {
            value: "date",
            label: "By Date",
            content: view === "date" ? content : null,
            controls: dateFilter,
          },
          {
            value: periodTab,
            label: "By Period",
            content: view === "period" ? content : null,
            controls: periodFilter,
          },
        ]}
      />
    </PageLayout>
  );
};

export type { EntityHistorySummary };
export default EntityHistoryLayout;
