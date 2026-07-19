"use client";

import type {
  AssignmentGoalTypeSummary,
  GoalRangeSummary,
  GoalTrendsView,
  SpendingGoalTypeSummary,
} from "@/goals/trends/goalTrendsTypes";
import BreakdownSection, {
  type BreakdownDetailRow,
} from "@/framework/view/BreakdownSection";
import { Divider, Stack } from "@mui/material";
import { type JSX, type ReactNode, useState } from "react";
import {
  formatAssignmentGoalType,
  formatSpendingGoalType,
} from "@/goals/helpers";
import ExpandableSummaryCard from "@/framework/view/ExpandableSummaryCard";
import SummaryCardGrid from "@/framework/view/SummaryCardGrid";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the GoalTrendsSummaryCards component.
 */
interface GoalTrendsSummaryCardsProps {
  readonly trends: GoalRangeSummary;
  readonly view: GoalTrendsView;
}

/**
 * Defines the structure of a summary card, including its title, value, and detailed breakdown rows.
 */
interface SummaryCardDefinition {
  readonly title: string;
  readonly value: ReactNode;
  readonly detailLabel: string;
  readonly detailValue: ReactNode;
  readonly detailRows: readonly BreakdownDetailRow[];
}

/**
 * Formats the provided number as a percentage.
 */
const formatPercentage = function (value: number): string {
  return `${value.toFixed(2)}%`;
};

/**
 * Generates breakdown rows for assignment goal types based on the provided summaries and value extraction function.
 */
const getAssignmentRows = function (
  typeSummaries: readonly AssignmentGoalTypeSummary[],
  getValue: (summary: AssignmentGoalTypeSummary) => ReactNode,
): BreakdownDetailRow[] {
  return typeSummaries.map((summary) => ({
    key: summary.assignmentGoalType,
    label: formatAssignmentGoalType(summary.assignmentGoalType),
    value: getValue(summary),
  }));
};

/**
 * Generates breakdown rows for spending goal types based on the provided summaries and value extraction function.
 */
const getSpendingRows = function (
  typeSummaries: readonly SpendingGoalTypeSummary[],
  getValue: (summary: SpendingGoalTypeSummary) => ReactNode,
): BreakdownDetailRow[] {
  return typeSummaries.map((summary) => ({
    key: summary.spendingGoalType,
    label: formatSpendingGoalType(summary.spendingGoalType),
    value: getValue(summary),
  }));
};

/**
 * Displays the main summary cards for the goal trends with goal-type breakdowns.
 */
const GoalTrendsSummaryCards = function ({
  trends,
  view,
}: GoalTrendsSummaryCardsProps): JSX.Element {
  const [expanded, setExpanded] = useState(false);

  const handleToggleExpanded = function (): void {
    setExpanded((currentValue) => !currentValue);
  };

  const cardDefinitions: readonly SummaryCardDefinition[] =
    view === "assignment"
      ? [
          {
            title: "Total amount to assign",
            value: formatCurrency(trends.totalAmountToAssign),
            detailLabel: "Goal type breakdown",
            detailValue: formatCurrency(trends.totalAmountToAssign),
            detailRows: getAssignmentRows(
              trends.assignmentGoalTypes,
              (summary) => formatCurrency(summary.totalAmountToAssign),
            ),
          },
          {
            title: "Total amount assigned",
            value: formatCurrency(trends.totalAmountAssigned),
            detailLabel: "Goal type breakdown",
            detailValue: formatCurrency(trends.totalAmountAssigned),
            detailRows: getAssignmentRows(
              trends.assignmentGoalTypes,
              (summary) => formatCurrency(summary.totalAmountAssigned),
            ),
          },
          {
            title: "Goals met",
            value: `${trends.percentageOfAssignmentGoalsMet.metCount} / ${trends.percentageOfAssignmentGoalsMet.totalCount} (${formatPercentage(trends.percentageOfAssignmentGoalsMet.percentageMet)})`,
            detailLabel: "Goal type breakdown",
            detailValue: `${trends.percentageOfAssignmentGoalsMet.metCount} / ${trends.percentageOfAssignmentGoalsMet.totalCount} (${formatPercentage(trends.percentageOfAssignmentGoalsMet.percentageMet)})`,
            detailRows: getAssignmentRows(
              trends.assignmentGoalTypes,
              (summary) =>
                `${summary.percentageOfGoalsMet.metCount} / ${summary.percentageOfGoalsMet.totalCount} (${formatPercentage(summary.percentageOfGoalsMet.percentageMet)})`,
            ),
          },
        ]
      : [
          {
            title: "Total amount to spend",
            value: formatCurrency(trends.totalAmountToSpend),
            detailLabel: "Goal type breakdown",
            detailValue: formatCurrency(trends.totalAmountToSpend),
            detailRows: getSpendingRows(trends.spendingGoalTypes, (summary) =>
              formatCurrency(summary.totalAmountToSpend),
            ),
          },
          {
            title: "Total amount spent",
            value: formatCurrency(trends.totalAmountSpent),
            detailLabel: "Goal type breakdown",
            detailValue: formatCurrency(trends.totalAmountSpent),
            detailRows: getSpendingRows(trends.spendingGoalTypes, (summary) =>
              formatCurrency(summary.totalAmountSpent),
            ),
          },
          {
            title: "Goals met",
            value: `${trends.percentageOfSpendingGoalsMet.metCount} / ${trends.percentageOfSpendingGoalsMet.totalCount} (${formatPercentage(trends.percentageOfSpendingGoalsMet.percentageMet)})`,
            detailLabel: "Goal type breakdown",
            detailValue: `${trends.percentageOfSpendingGoalsMet.metCount} / ${trends.percentageOfSpendingGoalsMet.totalCount} (${formatPercentage(trends.percentageOfSpendingGoalsMet.percentageMet)})`,
            detailRows: getSpendingRows(
              trends.spendingGoalTypes,
              (summary) =>
                `${summary.percentageOfGoalsMet.metCount} / ${summary.percentageOfGoalsMet.totalCount} (${formatPercentage(summary.percentageOfGoalsMet.percentageMet)})`,
            ),
          },
        ];

  return (
    <SummaryCardGrid>
      {cardDefinitions.map((card) => (
        <ExpandableSummaryCard
          key={card.title}
          title={card.title}
          value={card.value}
          expanded={expanded}
          onToggle={handleToggleExpanded}
        >
          <Stack spacing={1.25} divider={<Divider flexItem />}>
            <BreakdownSection
              label={card.detailLabel}
              value={card.detailValue}
              detailRows={card.detailRows}
              expanded={expanded}
              onToggle={handleToggleExpanded}
            />
          </Stack>
        </ExpandableSummaryCard>
      ))}
    </SummaryCardGrid>
  );
};

export default GoalTrendsSummaryCards;
