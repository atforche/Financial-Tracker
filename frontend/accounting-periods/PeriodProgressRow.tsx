"use client";

import {
  Box,
  ButtonBase,
  Collapse,
  Divider,
  Stack,
  Typography,
} from "@mui/material";
import { type JSX, type ReactNode, useId, useState } from "react";
import {
  compareCurrencyAmounts,
  formatCurrency,
  getCurrencyDifference,
  getCurrencyTotal,
} from "@/framework/currencyHelpers";
import ExpandMore from "@mui/icons-material/ExpandMore";
import Link from "next/link";
import type { Route } from "next";

/**
 * Props for the PeriodProgressRow component.
 */
interface PeriodProgressRowProps {
  readonly label: string;
  readonly actual: number;
  readonly expected: number;
  readonly isOverGood?: boolean;
  readonly unplanned?: number;
  readonly actualLabel?: string;
  readonly expectedLabel?: string;
  readonly overLabel: string;
  readonly underLabel?: string;
  readonly caption?: ReactNode;
  readonly href?: Route;
}

/**
 * Props for the KeyLine component.
 */
interface KeyLineProps {
  readonly label: string;
  readonly amount: number;
  readonly sign?: "+" | "−";
  readonly swatchColor?: string;
  readonly amountColor?: string;
}

/**
 * Displays one line of a progress row's breakdown. A swatch in the color of the bar segment the line describes lets
 * the breakdown double as the bar's key.
 */
const KeyLine = function ({
  label,
  amount,
  sign,
  swatchColor,
  amountColor,
}: KeyLineProps): JSX.Element {
  return (
    <Stack
      direction="row"
      spacing={1}
      sx={{ alignItems: "center", justifyContent: "space-between" }}
    >
      <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
        <Box
          sx={{
            width: 10,
            height: 10,
            borderRadius: 0.5,
            flexShrink: 0,
            bgcolor: swatchColor ?? "transparent",
          }}
        />
        <Typography variant="body2" color="text.secondary">
          {label}
        </Typography>
      </Stack>
      <Typography
        variant="body2"
        sx={{ fontWeight: 500, color: amountColor, whiteSpace: "nowrap" }}
      >
        {typeof sign === "string" ? `${sign} ` : ""}
        {formatCurrency(amount)}
      </Typography>
    </Stack>
  );
};

/**
 * Shows an amount against what is expected for the period as one bar: the amount up to what was expected, then
 * anything beyond what was expected, colored as good or bad, or the empty track for any shortfall, and last an
 * unplanned amount, which isn't compared against what was expected. The heading shows the total, planned and unplanned.
 * The full breakdown, collapsed beneath the bar until the row is expanded, lists each segment in the bar's order and
 * sums them to the bar's full length. With an href, the row links there instead of expanding. A caption, when given,
 * sits beneath the bar.
 */
const PeriodProgressRow = function ({
  label,
  actual,
  expected,
  isOverGood = true,
  unplanned = 0,
  actualLabel = "Received",
  expectedLabel = "Planned",
  overLabel,
  underLabel = "Short",
  caption,
  href,
}: PeriodProgressRowProps): JSX.Element {
  const [expanded, setExpanded] = useState(false);
  const breakdownId = useId();
  const difference = getCurrencyDifference(actual, expected);
  const overColor = isOverGood ? "success.main" : "error.main";
  const underColor = isOverGood ? "error.main" : "success.main";
  const unplannedAmount = Math.max(unplanned, 0);
  const met = Math.max(Math.min(actual, expected), 0);
  const over = Math.max(difference, 0);
  const under = Math.max(-difference, 0);
  const isShort = compareCurrencyAmounts(under, 0) > 0;
  // Unplanned amounts sit outside the comparison, so the bar's end is always exactly the extra or the shortfall
  const barTotal = Math.max(actual, expected, 0) + unplannedAmount;
  const getShare = (amount: number): string =>
    `${barTotal > 0 ? (amount / barTotal) * 100 : 0}%`;
  return (
    <Stack>
      <ButtonBase
        {...(href === undefined
          ? {
              onClick: (): void => {
                setExpanded(!expanded);
              },
              "aria-label": `${expanded ? "Collapse" : "Expand"} ${label} breakdown`,
              "aria-expanded": expanded,
              "aria-controls": breakdownId,
            }
          : { href, LinkComponent: Link })}
        sx={{
          display: "flex",
          flexDirection: "column",
          alignItems: "stretch",
          gap: 0.75,
          textAlign: "left",
          borderRadius: 1,
        }}
      >
        <Stack
          direction="row"
          spacing={2}
          sx={{ alignItems: "baseline", justifyContent: "space-between" }}
        >
          <Typography variant="subtitle2" noWrap sx={{ minWidth: 0 }}>
            {label}
          </Typography>
          <Typography variant="body2" sx={{ whiteSpace: "nowrap" }}>
            <Box component="span" sx={{ fontWeight: 500 }}>
              {formatCurrency(getCurrencyTotal([actual, unplannedAmount]))}
            </Box>
            {href === undefined ? (
              <ExpandMore
                fontSize="small"
                sx={{
                  ml: 0.5,
                  verticalAlign: "middle",
                  color: "text.secondary",
                  transform: expanded ? "rotate(180deg)" : "rotate(0deg)",
                  transition: "transform 0.3s ease-in-out",
                }}
              />
            ) : null}
          </Typography>
        </Stack>
        <Box
          sx={{
            display: "flex",
            height: 12,
            borderRadius: 6,
            overflow: "hidden",
            bgcolor: "action.hover",
          }}
        >
          <Box sx={{ width: getShare(met), bgcolor: "primary.main" }} />
          <Box sx={{ width: getShare(over), bgcolor: overColor }} />
          <Box sx={{ width: getShare(under) }} />
          <Box
            sx={{ width: getShare(unplannedAmount), bgcolor: "secondary.main" }}
          />
        </Box>
        {caption === undefined ? null : (
          <Typography variant="caption" color="text.secondary">
            {caption}
          </Typography>
        )}
      </ButtonBase>
      <Collapse id={breakdownId} in={expanded} timeout="auto" unmountOnExit>
        <Stack spacing={0.5} sx={{ pt: 1.25 }}>
          {/* Short of what was expected, the bar runs to what was expected, so what was expected bounds the bar rather
              than its first segment */}
          <KeyLine
            label={isShort ? actualLabel : expectedLabel}
            amount={met}
            swatchColor="primary.main"
          />
          {compareCurrencyAmounts(over, 0) > 0 ? (
            <KeyLine
              label={overLabel}
              amount={over}
              sign="+"
              swatchColor={overColor}
              amountColor={overColor}
            />
          ) : isShort ? (
            <KeyLine
              label={underLabel}
              amount={under}
              sign="+"
              swatchColor="action.hover"
              amountColor={underColor}
            />
          ) : null}
          {unplannedAmount > 0 ? (
            <KeyLine
              label="Unplanned"
              amount={unplannedAmount}
              sign="+"
              swatchColor="secondary.main"
            />
          ) : null}
          <Divider />
          <KeyLine
            label={
              isShort
                ? `${expectedLabel}${unplannedAmount > 0 ? " + unplanned" : ""}`
                : label
            }
            amount={barTotal}
          />
        </Stack>
      </Collapse>
    </Stack>
  );
};

export default PeriodProgressRow;
