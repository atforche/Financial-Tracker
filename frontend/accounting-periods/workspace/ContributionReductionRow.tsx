"use client";

import { type JSX, useState } from "react";
import { Stack, Typography } from "@mui/material";
import {
  compareCurrencyAmounts,
  formatCurrency,
  formatSignedCurrency,
  getCurrencyDifference,
  getCurrencyTotal,
} from "@/framework/currencyHelpers";
import BreakdownSection from "@/framework/view/BreakdownSection";

/**
 * Props for the ContributionReductionRow component.
 */
interface ContributionReductionRowProps {
  readonly reduction: number;
  readonly currentBalance: number;
  readonly plannedRemaining: number;
  readonly limit: number;
  readonly limitName: "maximum" | "target";
}

/**
 * Shows how much of a planned contribution would take a fund past its limit, expanding to the calculation behind it.
 */
const ContributionReductionRow = function ({
  reduction,
  currentBalance,
  plannedRemaining,
  limit,
  limitName,
}: ContributionReductionRowProps): JSX.Element {
  const [expanded, setExpanded] = useState(false);
  const projectedBalance = getCurrencyTotal([currentBalance, plannedRemaining]);
  // A reduction can't exceed the planned amount, so a fund already past its limit can project further past it
  const isAlreadyPastLimit =
    compareCurrencyAmounts(
      getCurrencyDifference(projectedBalance, limit),
      reduction,
    ) !== 0;
  return (
    <Stack spacing={0.5}>
      <BreakdownSection
        label={`Would exceed ${limitName}`}
        value={formatCurrency(-reduction)}
        detailRows={[
          {
            key: "current",
            label: "Current balance",
            value: formatCurrency(currentBalance),
          },
          {
            key: "remaining",
            label: "Planned remaining contributions",
            value: formatSignedCurrency(plannedRemaining),
          },
          {
            key: "projected",
            label: "Projected ending balance",
            value: formatCurrency(projectedBalance),
          },
          {
            key: "limit",
            label: limitName === "target" ? "Target" : "Maximum",
            value: formatCurrency(limit),
          },
        ]}
        labelColor="text.secondary"
        expanded={expanded}
        onToggle={() => {
          setExpanded(!expanded);
        }}
      />
      {expanded && isAlreadyPastLimit ? (
        <Typography variant="caption" color="text.secondary" sx={{ pl: 1.5 }}>
          This fund is already above its {limitName}, so nothing more is
          expected.
        </Typography>
      ) : null}
    </Stack>
  );
};

export default ContributionReductionRow;
