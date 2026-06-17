"use client";

import {
  Box,
  Collapse,
  Divider,
  IconButton,
  Stack,
  Typography,
} from "@mui/material";
import {
  type GoalTrends,
  type GoalTrendsAssignmentGoalTypeSummary,
  type GoalTrendsSpendingGoalTypeSummary,
  formatAssignmentGoalType,
  formatSpendingGoalType,
} from "@/goals/types";
import { type JSX, type ReactNode, useState } from "react";
import ExpandMore from "@mui/icons-material/ExpandMore";
import type { GoalTrendsView } from "@/goals/trends/goalTrendsTypes";
import SummaryCard from "@/framework/view/SummaryCard";
import formatCurrency from "@/framework/formatCurrency";

interface GoalTrendsSummaryCardsProps {
  readonly trends: GoalTrends;
  readonly view: GoalTrendsView;
}

interface GoalTypeBreakdownDetailRow {
  readonly key: string;
  readonly label: string;
  readonly value: ReactNode;
}

interface GoalTypeBreakdownSectionProps {
  readonly label: string;
  readonly value: ReactNode;
  readonly detailRows: readonly GoalTypeBreakdownDetailRow[];
  readonly expanded: boolean;
  readonly onToggle: () => void;
}

interface SummaryCardDefinition {
  readonly title: string;
  readonly value: ReactNode;
  readonly detailLabel: string;
  readonly detailValue: ReactNode;
  readonly detailRows: readonly GoalTypeBreakdownDetailRow[];
}

const expandToggleSlotSize = 26;

const GoalTypeBreakdownSection = function ({
  label,
  value,
  detailRows,
  expanded,
  onToggle,
}: GoalTypeBreakdownSectionProps): JSX.Element {
  return (
    <Stack spacing={0.75}>
      <Stack
        direction="row"
        justifyContent="space-between"
        alignItems="center"
        gap={1.5}
      >
        <Typography variant="body2">{label}</Typography>
        <Stack direction="row" alignItems="center" gap={0.5}>
          <Typography
            variant="body2"
            fontWeight={600}
            sx={{ textAlign: "right" }}
          >
            {value}
          </Typography>
          <Box
            sx={{
              width: expandToggleSlotSize,
              height: expandToggleSlotSize,
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
            }}
          >
            {detailRows.length > 0 && (
              <IconButton
                size="small"
                onClick={onToggle}
                sx={{
                  p: 0.25,
                  transform: expanded ? "rotate(180deg)" : "rotate(0deg)",
                  transition: "transform 0.3s ease-in-out",
                }}
              >
                <ExpandMore fontSize="small" />
              </IconButton>
            )}
          </Box>
        </Stack>
      </Stack>
      {detailRows.length > 0 && (
        <Collapse in={expanded} timeout="auto" unmountOnExit>
          <Stack spacing={0.75} sx={{ pl: 1.5 }}>
            {detailRows.map((detailRow) => (
              <Stack
                key={detailRow.key}
                direction="row"
                justifyContent="space-between"
                alignItems="baseline"
                gap={1.5}
              >
                <Typography variant="caption" color="text.secondary">
                  {detailRow.label}
                </Typography>
                <Stack direction="row" alignItems="center" gap={0.5}>
                  <Typography
                    variant="caption"
                    fontWeight={600}
                    sx={{ textAlign: "right" }}
                  >
                    {detailRow.value}
                  </Typography>
                  <Box
                    sx={{
                      width: expandToggleSlotSize,
                      height: expandToggleSlotSize,
                      flexShrink: 0,
                    }}
                  />
                </Stack>
              </Stack>
            ))}
          </Stack>
        </Collapse>
      )}
    </Stack>
  );
};

interface GoalTypeBreakdownProps {
  readonly expanded: boolean;
  readonly children: ReactNode;
}

const GoalTypeBreakdown = function ({
  expanded,
  children,
}: GoalTypeBreakdownProps): JSX.Element {
  return (
    <Collapse in={expanded} timeout="auto" unmountOnExit>
      <Stack spacing={1.25} sx={{ pt: 1.25 }}>
        <Divider />
        {children}
      </Stack>
    </Collapse>
  );
};

const formatPercentage = function (value: number): string {
  return `${value.toFixed(2)}%`;
};

const getAssignmentRows = function (
  typeSummaries: readonly GoalTrendsAssignmentGoalTypeSummary[],
  getValue: (summary: GoalTrendsAssignmentGoalTypeSummary) => ReactNode,
): GoalTypeBreakdownDetailRow[] {
  return typeSummaries.map((summary) => ({
    key: summary.assignmentGoalType,
    label: formatAssignmentGoalType(summary.assignmentGoalType),
    value: getValue(summary),
  }));
};

const getSpendingRows = function (
  typeSummaries: readonly GoalTrendsSpendingGoalTypeSummary[],
  getValue: (summary: GoalTrendsSpendingGoalTypeSummary) => ReactNode,
): GoalTypeBreakdownDetailRow[] {
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
              trends.assignmentGoalTypes ?? [],
              (summary) => formatCurrency(summary.totalAmountToAssign),
            ),
          },
          {
            title: "Total amount assigned",
            value: formatCurrency(trends.totalAmountAssigned),
            detailLabel: "Goal type breakdown",
            detailValue: formatCurrency(trends.totalAmountAssigned),
            detailRows: getAssignmentRows(
              trends.assignmentGoalTypes ?? [],
              (summary) => formatCurrency(summary.totalAmountAssigned),
            ),
          },
          {
            title: "Goals met",
            value: `${trends.percentageOfAssignmentGoalsMet.metCount} / ${trends.percentageOfAssignmentGoalsMet.totalCount} (${formatPercentage(trends.percentageOfAssignmentGoalsMet.percentageMet)})`,
            detailLabel: "Goal type breakdown",
            detailValue: `${trends.percentageOfAssignmentGoalsMet.metCount} / ${trends.percentageOfAssignmentGoalsMet.totalCount} (${formatPercentage(trends.percentageOfAssignmentGoalsMet.percentageMet)})`,
            detailRows: getAssignmentRows(
              trends.assignmentGoalTypes ?? [],
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
            detailRows: getSpendingRows(
              trends.spendingGoalTypes ?? [],
              (summary) => formatCurrency(summary.totalAmountToSpend),
            ),
          },
          {
            title: "Total amount spent",
            value: formatCurrency(trends.totalAmountSpent),
            detailLabel: "Goal type breakdown",
            detailValue: formatCurrency(trends.totalAmountSpent),
            detailRows: getSpendingRows(
              trends.spendingGoalTypes ?? [],
              (summary) => formatCurrency(summary.totalAmountSpent),
            ),
          },
          {
            title: "Goals met",
            value: `${trends.percentageOfSpendingGoalsMet.metCount} / ${trends.percentageOfSpendingGoalsMet.totalCount} (${formatPercentage(trends.percentageOfSpendingGoalsMet.percentageMet)})`,
            detailLabel: "Goal type breakdown",
            detailValue: `${trends.percentageOfSpendingGoalsMet.metCount} / ${trends.percentageOfSpendingGoalsMet.totalCount} (${formatPercentage(trends.percentageOfSpendingGoalsMet.percentageMet)})`,
            detailRows: getSpendingRows(
              trends.spendingGoalTypes ?? [],
              (summary) =>
                `${summary.percentageOfGoalsMet.metCount} / ${summary.percentageOfGoalsMet.totalCount} (${formatPercentage(summary.percentageOfGoalsMet.percentageMet)})`,
            ),
          },
        ];

  return (
    <Box
      sx={{
        display: "grid",
        gap: 2,
        gridTemplateColumns: {
          xs: "1fr",
          md: "repeat(2, minmax(0, 1fr))",
          xl: "repeat(3, minmax(0, 1fr))",
        },
      }}
    >
      {cardDefinitions.map((card) => (
        <SummaryCard
          key={card.title}
          title={card.title}
          value={
            <Stack
              direction="row"
              alignItems="center"
              gap={0.5}
              justifyContent="space-between"
            >
              <Box>{card.value}</Box>
              <IconButton
                size="small"
                onClick={handleToggleExpanded}
                sx={{
                  p: 0.25,
                  transform: expanded ? "rotate(180deg)" : "rotate(0deg)",
                  transition: "transform 0.3s ease-in-out",
                }}
              >
                <ExpandMore fontSize="small" />
              </IconButton>
            </Stack>
          }
        >
          <GoalTypeBreakdown expanded={expanded}>
            <Stack spacing={1.25} divider={<Divider flexItem />}>
              <GoalTypeBreakdownSection
                label={card.detailLabel}
                value={card.detailValue}
                detailRows={card.detailRows}
                expanded={expanded}
                onToggle={handleToggleExpanded}
              />
            </Stack>
          </GoalTypeBreakdown>
        </SummaryCard>
      ))}
    </Box>
  );
};

export default GoalTrendsSummaryCards;
