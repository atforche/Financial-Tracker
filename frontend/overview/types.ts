import type { AccountBalanceSummary } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { BalanceTrendChartPoint } from "@/framework/charts/balanceTrendHelpers";
import type { FundBalanceSummary } from "@/funds/types";

/**
 * Account balance data displayed on the overview page.
 */
type AccountOverviewSummary = Pick<
  AccountBalanceSummary,
  "totalBalance" | "balanceByAccountType"
>;

/**
 * Fund balance data displayed on the overview page.
 */
type FundOverviewSummary = Pick<
  FundBalanceSummary,
  "totalBalance" | "totalAssignedBalance" | "totalUnassignedBalance"
>;

/**
 * Aggregated data required to render the overview page.
 */
interface OverviewData {
  readonly accountSummary: AccountOverviewSummary;
  readonly fundSummary: FundOverviewSummary;
  readonly accountBalanceTrend: readonly BalanceTrendChartPoint[];
  readonly fundBalanceTrend: readonly BalanceTrendChartPoint[];
  readonly latestAccountingPeriod: AccountingPeriod | null;
  readonly currentAccountingPeriod: AccountingPeriod | null;
}

export type { AccountOverviewSummary, FundOverviewSummary, OverviewData };
