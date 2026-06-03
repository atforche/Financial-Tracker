"use client";

import {
  Box,
  Collapse,
  Divider,
  IconButton,
  Stack,
  Typography,
} from "@mui/material";
import { type FundDashboard, FundDashboardMode } from "@/funds/types";
import { type JSX, type ReactNode, useState } from "react";
import ExpandMore from "@mui/icons-material/ExpandMore";
import SummaryCard from "@/framework/view/SummaryCard";
import formatCurrency from "@/framework/formatCurrency";

interface FundDashboardSummaryCardsProps {
  readonly dashboard: FundDashboard;
}

/**
 * Summary metrics derived from the selected dashboard range.
 */
interface DashboardSnapshot {
  readonly startLabel: string;
  readonly endLabel: string;
  readonly totalStartingBalance: number;
  readonly totalEndingBalance: number;
  readonly assignedStartingBalance: number;
  readonly assignedEndingBalance: number;
  readonly unassignedStartingBalance: number;
  readonly unassignedEndingBalance: number;
}

const getDashboardSnapshot = function (
  dashboard: FundDashboard,
): DashboardSnapshot {
  if (
    dashboard.mode === FundDashboardMode.AccountingPeriod &&
    typeof dashboard.accountingPeriods !== "undefined" &&
    dashboard.accountingPeriods !== null &&
    dashboard.accountingPeriods.length > 0
  ) {
    const firstPeriod = dashboard.accountingPeriods.at(0);
    const lastPeriod = dashboard.accountingPeriods.at(-1);
    if (
      typeof firstPeriod === "undefined" ||
      typeof lastPeriod === "undefined"
    ) {
      return {
        startLabel: "Start",
        endLabel: "End",
        totalStartingBalance: 0,
        totalEndingBalance: 0,
        assignedStartingBalance: 0,
        assignedEndingBalance: 0,
        unassignedStartingBalance: 0,
        unassignedEndingBalance: 0,
      };
    }
    return {
      startLabel: firstPeriod.accountingPeriodName,
      endLabel: lastPeriod.accountingPeriodName,
      totalStartingBalance: firstPeriod.totalOpeningBalance,
      totalEndingBalance: lastPeriod.totalClosingBalance,
      assignedStartingBalance: firstPeriod.assignedOpeningBalance,
      assignedEndingBalance: lastPeriod.assignedClosingBalance,
      unassignedStartingBalance: firstPeriod.unassignedOpeningBalance,
      unassignedEndingBalance: lastPeriod.unassignedClosingBalance,
    };
  }

  const dates = dashboard.dates ?? [];
  const firstDate = dates.at(0);
  const lastDate = dates.at(-1);

  const dateFormatter = new Intl.DateTimeFormat("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
  return {
    startLabel: firstDate
      ? dateFormatter.format(new Date(`${firstDate.date}T00:00:00`))
      : "Start",
    endLabel: lastDate
      ? dateFormatter.format(new Date(`${lastDate.date}T00:00:00`))
      : "End",
    totalStartingBalance: firstDate?.totalBalance ?? 0,
    totalEndingBalance: lastDate?.totalBalance ?? 0,
    assignedStartingBalance: firstDate?.assignedBalance ?? 0,
    assignedEndingBalance: lastDate?.assignedBalance ?? 0,
    unassignedStartingBalance: firstDate?.unassignedBalance ?? 0,
    unassignedEndingBalance: lastDate?.unassignedBalance ?? 0,
  };
};

interface BalanceBreakdownSectionProps {
  readonly label: string;
  readonly value: ReactNode;
}

const BalanceBreakdownSection = function ({
  label,
  value,
}: BalanceBreakdownSectionProps): JSX.Element {
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
        </Stack>
      </Stack>
    </Stack>
  );
};

interface BalanceBreakdownProps {
  readonly expanded: boolean;
  readonly children: ReactNode;
}

const BalanceBreakdown = function ({
  expanded,
  children,
}: BalanceBreakdownProps): JSX.Element {
  return (
    <Collapse in={expanded} timeout="auto" unmountOnExit>
      <Stack spacing={1.25} sx={{ pt: 1.25 }}>
        <Divider />
        {children}
      </Stack>
    </Collapse>
  );
};

/**
 * Displays the top-level fund balance summary cards with synchronized details.
 */
const FundDashboardSummaryCards = function ({
  dashboard,
}: FundDashboardSummaryCardsProps): JSX.Element {
  const snapshot = getDashboardSnapshot(dashboard);

  const [expanded, setExpanded] = useState(false);
  const netChange = snapshot.totalEndingBalance - snapshot.totalStartingBalance;
  const percentChange =
    snapshot.totalStartingBalance === 0
      ? 0
      : (netChange / Math.abs(snapshot.totalStartingBalance)) * 100;
  const isPositive = netChange >= 0;
  const valueColor = isPositive ? "success.main" : "error.main";
  const assignedNetChange =
    snapshot.assignedEndingBalance - snapshot.assignedStartingBalance;
  const unassignedNetChange =
    snapshot.unassignedEndingBalance - snapshot.unassignedStartingBalance;
  const assignedPercentChange =
    snapshot.assignedStartingBalance === 0
      ? 0
      : (assignedNetChange / Math.abs(snapshot.assignedStartingBalance)) * 100;
  const unassignedPercentChange =
    snapshot.unassignedStartingBalance === 0
      ? 0
      : (unassignedNetChange / Math.abs(snapshot.unassignedStartingBalance)) *
        100;
  const handleToggleExpanded = function (): void {
    setExpanded((currentValue) => !currentValue);
  };

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
      <SummaryCard
        title={`Starting balance (${snapshot.startLabel})`}
        value={
          <Stack
            direction="row"
            alignItems="center"
            gap={0.5}
            justifyContent="space-between"
          >
            <Box>{formatCurrency(snapshot.totalStartingBalance)}</Box>
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
        <BalanceBreakdown expanded={expanded}>
          <Stack spacing={1.25} divider={<Divider flexItem />}>
            <BalanceBreakdownSection
              label="Assigned"
              value={formatCurrency(snapshot.assignedStartingBalance)}
            />
            <BalanceBreakdownSection
              label="Unassigned"
              value={formatCurrency(snapshot.unassignedStartingBalance)}
            />
          </Stack>
        </BalanceBreakdown>
      </SummaryCard>
      <SummaryCard
        title={`Ending balance (${snapshot.endLabel})`}
        value={
          <Stack
            direction="row"
            alignItems="center"
            gap={0.5}
            justifyContent="space-between"
          >
            <Box>{formatCurrency(snapshot.totalEndingBalance)}</Box>
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
        <BalanceBreakdown expanded={expanded}>
          <Stack spacing={1.25} divider={<Divider flexItem />}>
            <BalanceBreakdownSection
              label="Assigned"
              value={formatCurrency(snapshot.assignedEndingBalance)}
            />
            <BalanceBreakdownSection
              label="Unassigned"
              value={formatCurrency(snapshot.unassignedEndingBalance)}
            />
          </Stack>
        </BalanceBreakdown>
      </SummaryCard>
      <SummaryCard
        title="Net change"
        value={
          <Stack
            direction="row"
            alignItems="center"
            gap={0.5}
            justifyContent="space-between"
          >
            <Box component="span" sx={{ color: valueColor }}>
              {formatCurrency(netChange)} ({isPositive ? "+" : ""}
              {percentChange.toFixed(2)}%)
            </Box>
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
        <BalanceBreakdown expanded={expanded}>
          <Stack spacing={1.25} divider={<Divider flexItem />}>
            <BalanceBreakdownSection
              label="Tracked"
              value={
                <Box
                  component="span"
                  sx={{
                    color:
                      assignedNetChange >= 0 ? "success.main" : "error.main",
                  }}
                >
                  {formatCurrency(assignedNetChange)} (
                  {assignedNetChange >= 0 ? "+" : ""}
                  {assignedPercentChange.toFixed(2)}%)
                </Box>
              }
            />
            <BalanceBreakdownSection
              label="Unassigned"
              value={
                <Box
                  component="span"
                  sx={{
                    color:
                      unassignedNetChange >= 0 ? "success.main" : "error.main",
                  }}
                >
                  {formatCurrency(unassignedNetChange)} (
                  {unassignedNetChange >= 0 ? "+" : ""}
                  {unassignedPercentChange.toFixed(2)}%)
                </Box>
              }
            />
          </Stack>
        </BalanceBreakdown>
      </SummaryCard>
    </Box>
  );
};

export default FundDashboardSummaryCards;
