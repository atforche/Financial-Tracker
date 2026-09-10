"use client";

import {
  type AccountTypeBreakdownDetail,
  getAccountTypeBreakdownDetails,
} from "@/accounts/balanceSummaryHelpers";
import { type JSX, type ReactNode, useState } from "react";
import { formatAccountType, isTrackedAccountType } from "@/accounts/helpers";
import type { AccountBalanceSummary } from "@/accounts/types";
import AccountSummaryCard from "@/accounts/AccountSummaryCard";
import type { BreakdownDetailRow } from "@/framework/view/BreakdownSection";
import ChangeValue from "@/framework/view/ChangeValue";
import SummaryCardGrid from "@/framework/view/SummaryCardGrid";
import { formatCurrency } from "@/framework/currencyHelpers";

interface CardDefinition {
  readonly title: string;
  readonly value: ReactNode;
  readonly trackedValue: ReactNode;
  readonly untrackedValue: ReactNode;
  readonly trackedDetailRows: readonly BreakdownDetailRow[];
  readonly untrackedDetailRows: readonly BreakdownDetailRow[];
}

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

interface AccountBalanceSummaryCardsProps {
  readonly startingLabel: string;
  readonly endingLabel: string;
  readonly showLabels?: boolean;
  readonly titlePrefix?: string;
  readonly startingBalance: AccountBalanceSummary;
  readonly endingBalance: AccountBalanceSummary;
}

/**
 * Displays starting, ending, and net balance cards with account-type details.
 */
const AccountBalanceSummaryCards = function ({
  startingLabel,
  endingLabel,
  showLabels = true,
  titlePrefix,
  startingBalance,
  endingBalance,
}: AccountBalanceSummaryCardsProps): JSX.Element {
  const [expanded, setExpanded] = useState(false);
  const [trackedTypesExpanded, setTrackedTypesExpanded] = useState(false);
  const [untrackedTypesExpanded, setUntrackedTypesExpanded] = useState(false);
  const getTitle = function (title: string, label?: string): string {
    const prefixedTitle =
      typeof titlePrefix === "undefined" ? title : `${titlePrefix} ${title}`;
    return showLabels && typeof label !== "undefined"
      ? `${prefixedTitle} (${label})`
      : prefixedTitle;
  };
  const details = getAccountTypeBreakdownDetails(
    startingBalance,
    endingBalance,
  );
  const trackedDetails = details.filter(({ accountType }) =>
    isTrackedAccountType(accountType),
  );
  const untrackedDetails = details.filter(
    ({ accountType }) => !isTrackedAccountType(accountType),
  );
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
      title: getTitle("Starting Balance", startingLabel),
      value: formatCurrency(startingBalance.totalBalance),
      ...makeBreakdowns(
        (tracked) =>
          formatCurrency(
            tracked
              ? startingBalance.totalTrackedBalance
              : startingBalance.totalUntrackedBalance,
          ),
        ({ startingBalance: value }) => formatCurrency(value),
      ),
    },
    {
      title: getTitle("Ending Balance", endingLabel),
      value: formatCurrency(endingBalance.totalBalance),
      ...makeBreakdowns(
        (tracked) =>
          formatCurrency(
            tracked
              ? endingBalance.totalTrackedBalance
              : endingBalance.totalUntrackedBalance,
          ),
        ({ endingBalance: value }) => formatCurrency(value),
      ),
    },
    {
      title: getTitle("Net Change"),
      value: (
        <ChangeValue
          startingValue={startingBalance.totalBalance}
          endingValue={endingBalance.totalBalance}
        />
      ),
      ...makeBreakdowns(
        (tracked) => (
          <ChangeValue
            startingValue={
              tracked
                ? startingBalance.totalTrackedBalance
                : startingBalance.totalUntrackedBalance
            }
            endingValue={
              tracked
                ? endingBalance.totalTrackedBalance
                : endingBalance.totalUntrackedBalance
            }
          />
        ),
        ({ startingBalance: start, endingBalance: end }) => (
          <ChangeValue startingValue={start} endingValue={end} />
        ),
      ),
    },
  ];
  return (
    <SummaryCardGrid>
      {cards.map((card) => (
        <AccountSummaryCard
          key={card.title}
          {...card}
          expanded={expanded}
          onToggle={() => {
            setExpanded((value) => !value);
          }}
          trackedExpanded={trackedTypesExpanded}
          onTrackedToggle={() => {
            setTrackedTypesExpanded((value) => !value);
          }}
          untrackedExpanded={untrackedTypesExpanded}
          onUntrackedToggle={() => {
            setUntrackedTypesExpanded((value) => !value);
          }}
        />
      ))}
    </SummaryCardGrid>
  );
};

export type { AccountBalanceSummaryCardsProps };
export default AccountBalanceSummaryCards;
