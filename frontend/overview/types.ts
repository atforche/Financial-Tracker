import type { AccountSummary } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { FundSummary } from "@/funds/types";
import type { Transaction } from "@/transactions/types";

/**
 * Aggregated data required to render the overview page.
 */
interface OverviewData {
  readonly accountSummary: AccountSummary;
  readonly fundSummary: FundSummary;
  readonly currentAccountingPeriod: AccountingPeriod | null;
  readonly openAccountingPeriods: AccountingPeriod[];
  readonly totalAccountingPeriods: number;
  readonly totalAccounts: number;
  readonly totalFunds: number;
  readonly unpostedTransactions: Transaction[];
  readonly unpostedTransactionTotalCount: number;
}

export type { OverviewData };
