import type { AccountBalanceSummary } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { FundBalanceSummary } from "@/funds/types";

/**
 * Aggregated data required to render the overview page.
 */
interface OverviewData {
  readonly accountSummary: AccountBalanceSummary;
  readonly fundSummary: FundBalanceSummary;
  readonly currentAccountingPeriod: AccountingPeriod | null;
  readonly openAccountingPeriods: AccountingPeriod[];
  readonly totalAccountingPeriods: number;
  readonly totalAccounts: number;
  readonly totalFunds: number;
}

export type { OverviewData };
