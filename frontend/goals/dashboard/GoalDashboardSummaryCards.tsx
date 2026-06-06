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
  type GoalDashboard,
  type GoalTypeSummary,
  formatGoalType,
} from "@/goals/types";
import { type JSX, type ReactNode, useState } from "react";
import ExpandMore from "@mui/icons-material/ExpandMore";
import SummaryCard from "@/framework/view/SummaryCard";
import formatCurrency from "@/framework/formatCurrency";

interface GoalDashboardSummaryCardsProps {
  readonly dashboard: GoalDashboard;
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

const getGoalTypeRows = function (
  typeSummaries: readonly GoalTypeSummary[],
  getValue: (summary: GoalTypeSummary) => ReactNode,
): GoalTypeBreakdownDetailRow[] {
  return typeSummaries.map((summary) => ({
    key: summary.goalType,
    label: formatGoalType(summary.goalType),
    value: getValue(summary),
  }));
};

const formatPercentage = function (value: number): string {
  return `${value.toFixed(2)}%`;
};

/**
 * Displays the main summary cards for the goal dashboard with goal-type breakdowns.
 */
const GoalDashboardSummaryCards = function ({
  dashboard,
}: GoalDashboardSummaryCardsProps): JSX.Element {
  const [expanded, setExpanded] = useState(false);

  const handleToggleExpanded = function (): void {
    setExpanded((currentValue) => !currentValue);
  };

  const goalTypeSummaries = dashboard.goalTypes ?? [];

  const goalAmountRows = getGoalTypeRows(goalTypeSummaries, (summary) =>
    formatCurrency(summary.goalAmount),
  );
  const amountAssignedRows = getGoalTypeRows(goalTypeSummaries, (summary) =>
    formatCurrency(summary.amountAssigned),
  );
  const amountSpentRows = getGoalTypeRows(goalTypeSummaries, (summary) =>
    formatCurrency(summary.amountSpent),
  );
  const percentageRows = getGoalTypeRows(goalTypeSummaries, (summary) =>
    formatPercentage(summary.percentageOfGoalsMet),
  );

  return (
    <Box
      sx={{
        display: "grid",
        gap: 2,
        gridTemplateColumns: {
          xs: "1fr",
          md: "repeat(2, minmax(0, 1fr))",
          xl: "repeat(4, minmax(0, 1fr))",
        },
      }}
    >
      <SummaryCard
        title="Total Goal amounts"
        value={
          <Stack
            direction="row"
            alignItems="center"
            gap={0.5}
            justifyContent="space-between"
          >
            <Box>{formatCurrency(dashboard.totalGoalAmount)}</Box>
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
              label="Goal type breakdown"
              value={formatCurrency(dashboard.totalGoalAmount)}
              detailRows={goalAmountRows}
              expanded={expanded}
              onToggle={handleToggleExpanded}
            />
          </Stack>
        </GoalTypeBreakdown>
      </SummaryCard>

      <SummaryCard
        title="Total Amount assigned"
        value={
          <Stack
            direction="row"
            alignItems="center"
            gap={0.5}
            justifyContent="space-between"
          >
            <Box>{formatCurrency(dashboard.totalAmountAssigned)}</Box>
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
              label="Goal type breakdown"
              value={formatCurrency(dashboard.totalAmountAssigned)}
              detailRows={amountAssignedRows}
              expanded={expanded}
              onToggle={handleToggleExpanded}
            />
          </Stack>
        </GoalTypeBreakdown>
      </SummaryCard>

      <SummaryCard
        title="Total Amount spent"
        value={
          <Stack
            direction="row"
            alignItems="center"
            gap={0.5}
            justifyContent="space-between"
          >
            <Box>{formatCurrency(dashboard.totalAmountSpent)}</Box>
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
              label="Goal type breakdown"
              value={formatCurrency(dashboard.totalAmountSpent)}
              detailRows={amountSpentRows}
              expanded={expanded}
              onToggle={handleToggleExpanded}
            />
          </Stack>
        </GoalTypeBreakdown>
      </SummaryCard>

      <SummaryCard
        title="Total Percentage of goals met"
        value={
          <Stack
            direction="row"
            alignItems="center"
            gap={0.5}
            justifyContent="space-between"
          >
            <Box>{formatPercentage(dashboard.percentageOfGoalsMet)}</Box>
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
              label="Goal type breakdown"
              value={formatPercentage(dashboard.percentageOfGoalsMet)}
              detailRows={percentageRows}
              expanded={expanded}
              onToggle={handleToggleExpanded}
            />
          </Stack>
        </GoalTypeBreakdown>
      </SummaryCard>
    </Box>
  );
};

export default GoalDashboardSummaryCards;
