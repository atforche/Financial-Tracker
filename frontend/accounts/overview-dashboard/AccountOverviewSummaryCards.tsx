"use client";

import {
  type AccountType,
  type AccountTypeBalance,
  formatAccountType,
  isTrackedAccountType,
} from "@/accounts/types";
import {
  Box,
  Collapse,
  Divider,
  IconButton,
  Stack,
  Typography,
} from "@mui/material";
import { type JSX, type ReactNode, useState } from "react";
import ExpandMore from "@mui/icons-material/ExpandMore";
import SummaryCard from "@/framework/view/SummaryCard";
import formatCurrency from "@/framework/formatCurrency";

interface AccountOverviewSummaryCardsProps {
  readonly startLabel: string;
  readonly endLabel: string;
  readonly totalStartingBalance: number;
  readonly totalEndingBalance: number;
  readonly trackedStartingBalance: number;
  readonly trackedEndingBalance: number;
  readonly untrackedStartingBalance: number;
  readonly untrackedEndingBalance: number;
  readonly startingBalancesByType: readonly AccountTypeBalance[];
  readonly endingBalancesByType: readonly AccountTypeBalance[];
}

interface AccountTypeBreakdownDetail {
  readonly accountType: AccountType;
  readonly startingBalance: number;
  readonly endingBalance: number;
  readonly netChange: number;
}

interface BalanceBreakdownDetailRow {
  readonly key: string;
  readonly label: string;
  readonly value: ReactNode;
}

interface BalanceBreakdownSectionProps {
  readonly label: string;
  readonly value: ReactNode;
  readonly detailRows: readonly BalanceBreakdownDetailRow[];
  readonly expanded: boolean;
  readonly onToggle: () => void;
}

const BalanceBreakdownSection = function ({
  label,
  value,
  detailRows,
  expanded,
  onToggle,
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
                <Typography
                  variant="caption"
                  fontWeight={600}
                  sx={{ textAlign: "right" }}
                >
                  {detailRow.value}
                </Typography>
              </Stack>
            ))}
          </Stack>
        </Collapse>
      )}
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

const getAccountTypeBreakdownDetails = function (
  startingBalancesByType: readonly AccountTypeBalance[],
  endingBalancesByType: readonly AccountTypeBalance[],
): AccountTypeBreakdownDetail[] {
  const startingBalanceByType = new Map(
    startingBalancesByType.map((balance) => [
      balance.accountType,
      balance.totalBalance,
    ]),
  );
  const endingBalanceByType = new Map(
    endingBalancesByType.map((balance) => [
      balance.accountType,
      balance.totalBalance,
    ]),
  );
  const accountTypes = Array.from(
    new Set([...startingBalanceByType.keys(), ...endingBalanceByType.keys()]),
  );

  return accountTypes
    .map((accountType) => {
      const startingBalance = startingBalanceByType.get(accountType) ?? 0;
      const endingBalance = endingBalanceByType.get(accountType) ?? 0;

      return {
        accountType,
        startingBalance,
        endingBalance,
        netChange: endingBalance - startingBalance,
      };
    })
    .sort(
      (left, right) =>
        Math.abs(right.endingBalance) - Math.abs(left.endingBalance),
    );
};

const toBalanceBreakdownDetailRows = function (
  details: readonly AccountTypeBreakdownDetail[],
  getValue: (detail: AccountTypeBreakdownDetail) => ReactNode,
): BalanceBreakdownDetailRow[] {
  return details.map((detail) => ({
    key: detail.accountType,
    label: formatAccountType(detail.accountType),
    value: getValue(detail),
  }));
};

/**
 * Displays the top-level account balance summary cards with synchronized details.
 */
const AccountOverviewSummaryCards = function ({
  startLabel,
  endLabel,
  totalStartingBalance,
  totalEndingBalance,
  trackedStartingBalance,
  trackedEndingBalance,
  untrackedStartingBalance,
  untrackedEndingBalance,
  startingBalancesByType,
  endingBalancesByType,
}: AccountOverviewSummaryCardsProps): JSX.Element {
  const [expanded, setExpanded] = useState(false);
  const [trackedTypesExpanded, setTrackedTypesExpanded] = useState(false);
  const [untrackedTypesExpanded, setUntrackedTypesExpanded] = useState(false);
  const netChange = totalEndingBalance - totalStartingBalance;
  const percentChange =
    totalStartingBalance === 0
      ? 0
      : (netChange / Math.abs(totalStartingBalance)) * 100;
  const isPositive = netChange >= 0;
  const valueColor = isPositive ? "success.main" : "error.main";
  const trackedNetChange = trackedEndingBalance - trackedStartingBalance;
  const untrackedNetChange = untrackedEndingBalance - untrackedStartingBalance;
  const trackedPercentChange =
    trackedStartingBalance === 0
      ? 0
      : (trackedNetChange / Math.abs(trackedStartingBalance)) * 100;
  const untrackedPercentChange =
    untrackedStartingBalance === 0
      ? 0
      : (untrackedNetChange / Math.abs(untrackedStartingBalance)) * 100;
  const accountTypeBreakdownDetails = getAccountTypeBreakdownDetails(
    startingBalancesByType,
    endingBalancesByType,
  );
  const trackedAccountTypeDetails = accountTypeBreakdownDetails.filter(
    (detail) => isTrackedAccountType(detail.accountType),
  );
  const untrackedAccountTypeDetails = accountTypeBreakdownDetails.filter(
    (detail) => !isTrackedAccountType(detail.accountType),
  );
  const trackedStartingDetailRows = toBalanceBreakdownDetailRows(
    trackedAccountTypeDetails,
    (detail) => formatCurrency(detail.startingBalance),
  );
  const untrackedStartingDetailRows = toBalanceBreakdownDetailRows(
    untrackedAccountTypeDetails,
    (detail) => formatCurrency(detail.startingBalance),
  );
  const trackedEndingDetailRows = toBalanceBreakdownDetailRows(
    trackedAccountTypeDetails,
    (detail) => formatCurrency(detail.endingBalance),
  );
  const untrackedEndingDetailRows = toBalanceBreakdownDetailRows(
    untrackedAccountTypeDetails,
    (detail) => formatCurrency(detail.endingBalance),
  );
  const trackedNetDetailRows = toBalanceBreakdownDetailRows(
    trackedAccountTypeDetails,
    (detail) => {
      const trackedDetailPercentChange =
        detail.startingBalance === 0
          ? 0
          : (detail.netChange / Math.abs(detail.startingBalance)) * 100;
      return (
        <Box
          component="span"
          sx={{ color: detail.netChange >= 0 ? "success.main" : "error.main" }}
        >
          {formatCurrency(detail.netChange)} ({detail.netChange >= 0 ? "+" : ""}
          {trackedDetailPercentChange.toFixed(2)}%)
        </Box>
      );
    },
  );
  const untrackedNetDetailRows = toBalanceBreakdownDetailRows(
    untrackedAccountTypeDetails,
    (detail) => {
      const untrackedDetailPercentChange =
        detail.startingBalance === 0
          ? 0
          : (detail.netChange / Math.abs(detail.startingBalance)) * 100;
      return (
        <Box
          component="span"
          sx={{ color: detail.netChange >= 0 ? "success.main" : "error.main" }}
        >
          {formatCurrency(detail.netChange)} ({detail.netChange >= 0 ? "+" : ""}
          {untrackedDetailPercentChange.toFixed(2)}%)
        </Box>
      );
    },
  );
  const handleToggleExpanded = function (): void {
    setExpanded((currentValue) => !currentValue);
  };
  const handleToggleTrackedTypesExpanded = function (): void {
    setTrackedTypesExpanded((currentValue) => !currentValue);
  };
  const handleToggleUntrackedTypesExpanded = function (): void {
    setUntrackedTypesExpanded((currentValue) => !currentValue);
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
        title={`Starting balance (${startLabel})`}
        value={
          <Stack
            direction="row"
            alignItems="center"
            gap={0.5}
            justifyContent="space-between"
          >
            <Box>{formatCurrency(totalStartingBalance)}</Box>
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
              value={formatCurrency(trackedStartingBalance)}
              detailRows={trackedStartingDetailRows}
              expanded={trackedTypesExpanded}
              onToggle={handleToggleTrackedTypesExpanded}
            />
            <BalanceBreakdownSection
              label="Untracked"
              value={formatCurrency(untrackedStartingBalance)}
              detailRows={untrackedStartingDetailRows}
              expanded={untrackedTypesExpanded}
              onToggle={handleToggleUntrackedTypesExpanded}
            />
          </Stack>
        </BalanceBreakdown>
      </SummaryCard>
      <SummaryCard
        title={`Ending balance (${endLabel})`}
        value={
          <Stack
            direction="row"
            alignItems="center"
            gap={0.5}
            justifyContent="space-between"
          >
            <Box>{formatCurrency(totalEndingBalance)}</Box>
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
              value={formatCurrency(trackedEndingBalance)}
              detailRows={trackedEndingDetailRows}
              expanded={trackedTypesExpanded}
              onToggle={handleToggleTrackedTypesExpanded}
            />
            <BalanceBreakdownSection
              label="Untracked"
              value={formatCurrency(untrackedEndingBalance)}
              detailRows={untrackedEndingDetailRows}
              expanded={untrackedTypesExpanded}
              onToggle={handleToggleUntrackedTypesExpanded}
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
                      trackedNetChange >= 0 ? "success.main" : "error.main",
                  }}
                >
                  {formatCurrency(trackedNetChange)} (
                  {trackedNetChange >= 0 ? "+" : ""}
                  {trackedPercentChange.toFixed(2)}%)
                </Box>
              }
              detailRows={trackedNetDetailRows}
              expanded={trackedTypesExpanded}
              onToggle={handleToggleTrackedTypesExpanded}
            />
            <BalanceBreakdownSection
              label="Untracked"
              value={
                <Box
                  component="span"
                  sx={{
                    color:
                      untrackedNetChange >= 0 ? "success.main" : "error.main",
                  }}
                >
                  {formatCurrency(untrackedNetChange)} (
                  {untrackedNetChange >= 0 ? "+" : ""}
                  {untrackedPercentChange.toFixed(2)}%)
                </Box>
              }
              detailRows={untrackedNetDetailRows}
              expanded={untrackedTypesExpanded}
              onToggle={handleToggleUntrackedTypesExpanded}
            />
          </Stack>
        </BalanceBreakdown>
      </SummaryCard>
    </Box>
  );
};

export default AccountOverviewSummaryCards;
