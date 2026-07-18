"use client";

import type {
  AccountBalanceSummaryByDate,
  AccountBalanceSummaryByPeriod,
} from "@/accounts/types";
import AccountBreakdownSection, {
  type BreakdownDetailRow,
} from "@/framework/view/BreakdownSection";
import {
  type AccountTrendsDataMode,
  type AccountTypeBreakdownDetail,
  getAccountTrendsSnapshot,
  getAccountTypeBreakdownDetails,
} from "@/accounts/trends/helpers";
import { Divider, Stack } from "@mui/material";
import { type JSX, type ReactNode, useState } from "react";
import { formatAccountType, isTrackedAccountType } from "@/accounts/helpers";
import ChangeValue from "@/framework/view/ChangeValue";
import ExpandableSummaryCard from "@/framework/view/ExpandableSummaryCard";
import SummaryCardGrid from "@/framework/view/SummaryCardGrid";
import { formatCurrency } from "@/framework/currencyHelpers";

interface BreakdownDefinition {
  readonly label: string;
  readonly value: ReactNode;
  readonly detailRows: readonly BreakdownDetailRow[];
  readonly expanded: boolean;
  readonly onToggle: () => void;
}

interface CardDefinition {
  readonly title: string;
  readonly value: ReactNode;
  readonly breakdowns: readonly BreakdownDefinition[];
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
  ): readonly BreakdownDefinition[] {
    return [
      {
        label: "Tracked",
        value: getGroupValue(true),
        detailRows: toDetailRows(trackedDetails, getDetailValue),
        expanded: trackedTypesExpanded,
        onToggle: toggleTracked,
      },
      {
        label: "Untracked",
        value: getGroupValue(false),
        detailRows: toDetailRows(untrackedDetails, getDetailValue),
        expanded: untrackedTypesExpanded,
        onToggle: toggleUntracked,
      },
    ];
  };

  const cards: readonly CardDefinition[] = [
    {
      title: `Starting balance (${snapshot.startLabel})`,
      value: formatCurrency(snapshot.totalStartingBalance),
      breakdowns: makeBreakdowns(
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
      title: `Ending balance (${snapshot.endLabel})`,
      value: formatCurrency(snapshot.totalEndingBalance),
      breakdowns: makeBreakdowns(
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
      title: "Net change",
      value: (
        <ChangeValue
          startingValue={snapshot.totalStartingBalance}
          endingValue={snapshot.totalEndingBalance}
        />
      ),
      breakdowns: makeBreakdowns(
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
        <ExpandableSummaryCard
          key={card.title}
          title={card.title}
          value={card.value}
          expanded={expanded}
          onToggle={toggleExpanded}
        >
          <Stack spacing={1.25} divider={<Divider flexItem />}>
            {card.breakdowns.map((breakdown) => (
              <AccountBreakdownSection key={breakdown.label} {...breakdown} />
            ))}
          </Stack>
        </ExpandableSummaryCard>
      ))}
    </SummaryCardGrid>
  );
};

export default AccountTrendsSummaryCards;
