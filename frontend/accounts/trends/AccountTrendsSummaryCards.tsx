"use client";

import type {
  AccountBalanceSummaryByDate,
  AccountBalanceSummaryByPeriod,
} from "@/accounts/types";
import {
  type AccountTrendsDataMode,
  type AccountTypeBreakdownDetail,
  getAccountTrendsSnapshot,
  getAccountTypeBreakdownDetails,
} from "@/accounts/trends/helpers";
import { type JSX, type ReactNode, useState } from "react";
import { formatAccountType, isTrackedAccountType } from "@/accounts/helpers";
import AccountSummaryCard from "@/accounts/AccountSummaryCard";
import type { BreakdownDetailRow } from "@/framework/view/BreakdownSection";
import ChangeValue from "@/framework/view/ChangeValue";
import SummaryCardGrid from "@/framework/view/SummaryCardGrid";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Defines the structure of a summary card for account trends.
 */
interface CardDefinition {
  readonly title: string;
  readonly value: ReactNode;
  readonly trackedValue: ReactNode;
  readonly untrackedValue: ReactNode;
  readonly trackedDetailRows: readonly BreakdownDetailRow[];
  readonly untrackedDetailRows: readonly BreakdownDetailRow[];
}

/**
 * Converts account-type breakdown details into display rows.
 */
const toDetailRows = function (
  details: readonly AccountTypeBreakdownDetail[],
  getValue: (detail: AccountTypeBreakdownDetail) => ReactNode,
): BreakdownDetailRow[] {
  return details.map((detail) => ({
    key: detail.accountType,
    label: formatAccountType(detail.accountType),
    value: getValue(detail),
  }));
};

/**
 * Props for the AccountTrendsSummaryCards component.
 */
interface AccountTrendsSummaryCardsProps {
  readonly mode: AccountTrendsDataMode;
  readonly accountingPeriods: readonly AccountBalanceSummaryByPeriod[];
  readonly dates: readonly AccountBalanceSummaryByDate[];
}

/**
 * Displays account balances for the selected trends range.
 */
const AccountTrendsSummaryCards = function ({
  mode,
  accountingPeriods,
  dates,
}: AccountTrendsSummaryCardsProps): JSX.Element {
  const snapshot = getAccountTrendsSnapshot(mode, accountingPeriods, dates);
  const [expanded, setExpanded] = useState(false);
  const [trackedTypesExpanded, setTrackedTypesExpanded] = useState(false);
  const [untrackedTypesExpanded, setUntrackedTypesExpanded] = useState(false);

  const details = getAccountTypeBreakdownDetails(
    snapshot.startingBalancesByType,
    snapshot.endingBalancesByType,
  );
  const trackedDetails = details.filter(({ accountType }) =>
    isTrackedAccountType(accountType),
  );
  const untrackedDetails = details.filter(
    ({ accountType }) => !isTrackedAccountType(accountType),
  );
  const toggleTracked = function (): void {
    setTrackedTypesExpanded((value) => !value);
  };
  const toggleUntracked = function (): void {
    setUntrackedTypesExpanded((value) => !value);
  };

  const makeBreakdowns = function (
    getGroupValue: (tracked: boolean) => ReactNode,
    getDetailValue: (detail: AccountTypeBreakdownDetail) => ReactNode,
  ): Pick<
    CardDefinition,
    | "trackedValue"
    | "untrackedValue"
    | "trackedDetailRows"
    | "untrackedDetailRows"
  > {
    return {
      trackedValue: getGroupValue(true),
      untrackedValue: getGroupValue(false),
      trackedDetailRows: toDetailRows(trackedDetails, getDetailValue),
      untrackedDetailRows: toDetailRows(untrackedDetails, getDetailValue),
    };
  };

  const cards: readonly CardDefinition[] = [
    {
      title: `Starting Balance (${snapshot.startLabel})`,
      value: formatCurrency(snapshot.totalStartingBalance),
      ...makeBreakdowns(
        (tracked) =>
          formatCurrency(
            tracked
              ? snapshot.trackedStartingBalance
              : snapshot.untrackedStartingBalance,
          ),
        ({ startingBalance }) => formatCurrency(startingBalance),
      ),
    },
    {
      title: `Ending Balance (${snapshot.endLabel})`,
      value: formatCurrency(snapshot.totalEndingBalance),
      ...makeBreakdowns(
        (tracked) =>
          formatCurrency(
            tracked
              ? snapshot.trackedEndingBalance
              : snapshot.untrackedEndingBalance,
          ),
        ({ endingBalance }) => formatCurrency(endingBalance),
      ),
    },
    {
      title: "Net Change",
      value: (
        <ChangeValue
          startingValue={snapshot.totalStartingBalance}
          endingValue={snapshot.totalEndingBalance}
        />
      ),
      ...makeBreakdowns(
        (tracked) => (
          <ChangeValue
            startingValue={
              tracked
                ? snapshot.trackedStartingBalance
                : snapshot.untrackedStartingBalance
            }
            endingValue={
              tracked
                ? snapshot.trackedEndingBalance
                : snapshot.untrackedEndingBalance
            }
          />
        ),
        ({ startingBalance, endingBalance }) => (
          <ChangeValue
            startingValue={startingBalance}
            endingValue={endingBalance}
          />
        ),
      ),
    },
  ];

  const toggleExpanded = function (): void {
    setExpanded((value) => !value);
  };
  return (
    <SummaryCardGrid>
      {cards.map((card) => (
        <AccountSummaryCard
          key={card.title}
          {...card}
          expanded={expanded}
          onToggle={toggleExpanded}
          trackedExpanded={trackedTypesExpanded}
          onTrackedToggle={toggleTracked}
          untrackedExpanded={untrackedTypesExpanded}
          onUntrackedToggle={toggleUntracked}
        />
      ))}
    </SummaryCardGrid>
  );
};

export default AccountTrendsSummaryCards;
