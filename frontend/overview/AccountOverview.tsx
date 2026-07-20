"use client";

import { type JSX, useState } from "react";
import { formatAccountType, isTrackedAccountType } from "@/accounts/helpers";
import type { AccountOverviewSummary } from "@/overview/types";
import AccountSummaryCard from "@/accounts/AccountSummaryCard";
import type { BreakdownDetailRow } from "@/framework/view/BreakdownSection";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the AccountOverview component.
 */
interface AccountOverviewProps {
  readonly summary: AccountOverviewSummary;
}

/**
 * Overview component for Accounts.
 */
const AccountOverview = function ({
  summary,
}: AccountOverviewProps): JSX.Element {
  const [expanded, setExpanded] = useState(false);
  const [trackedExpanded, setTrackedExpanded] = useState(false);
  const [untrackedExpanded, setUntrackedExpanded] = useState(false);
  const trackedBalances = summary.balanceByAccountType.filter(
    ({ accountType }) => isTrackedAccountType(accountType),
  );
  const untrackedBalances = summary.balanceByAccountType.filter(
    ({ accountType }) => !isTrackedAccountType(accountType),
  );
  const toDetailRows = (
    balances: typeof summary.balanceByAccountType,
  ): BreakdownDetailRow[] =>
    balances.map(({ accountType, totalBalance }) => ({
      key: accountType,
      label: formatAccountType(accountType),
      value: formatCurrency(totalBalance),
    }));
  const total = (balances: typeof summary.balanceByAccountType): number =>
    balances.reduce((sum, { totalBalance }) => sum + totalBalance, 0);

  return (
    <AccountSummaryCard
      title="Current Total Account Balances"
      value={formatCurrency(summary.totalBalance)}
      trackedValue={formatCurrency(total(trackedBalances))}
      untrackedValue={formatCurrency(total(untrackedBalances))}
      trackedDetailRows={toDetailRows(trackedBalances)}
      untrackedDetailRows={toDetailRows(untrackedBalances)}
      expanded={expanded}
      onToggle={() => {
        setExpanded((current) => !current);
      }}
      trackedExpanded={trackedExpanded}
      onTrackedToggle={() => {
        setTrackedExpanded((current) => !current);
      }}
      untrackedExpanded={untrackedExpanded}
      onUntrackedToggle={() => {
        setUntrackedExpanded((current) => !current);
      }}
    />
  );
};

export default AccountOverview;
