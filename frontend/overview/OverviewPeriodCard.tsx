import { Box, Button, Stack, Typography } from "@mui/material";
import {
  compareCurrencyAmounts,
  formatCurrency,
  formatSignedCurrency,
} from "@/framework/currencyHelpers";
import ChevronRight from "@mui/icons-material/ChevronRight";
import ContentSurface from "@/framework/view/ContentSurface";
import type { JSX } from "react";
import Link from "next/link";
import accountingPeriodRoutes from "@/accounting-periods/routes";
import type { components } from "@/framework/data/api";

/**
 * Props for the OverviewPeriodCard component.
 */
interface OverviewPeriodCardProps {
  readonly period: components["schemas"]["OverviewPeriodModel"] | null;
}

/**
 * Gets the text color for a change: green when it is good, red when it is bad, and grey when there is none.
 */
const getChangeColor = function (
  change: number,
  isIncreaseGood: boolean,
): string {
  const comparison = compareCurrencyAmounts(change, 0);
  if (comparison === 0) {
    return "text.secondary";
  }
  return comparison > 0 === isIncreaseGood ? "success.main" : "error.main";
};

/**
 * Props for the PeriodProgressRow component.
 */
interface PeriodProgressRowProps {
  readonly label: string;
  readonly actual: number;
  readonly expected: number;
  readonly isExtraGood?: boolean;
}

/**
 * Shows an amount against what is expected for the period as one bar, with anything beyond what was expected as its
 * own segment, colored as good or bad.
 */
const PeriodProgressRow = function ({
  label,
  actual,
  expected,
  isExtraGood = true,
}: PeriodProgressRowProps): JSX.Element {
  const extra = actual - expected;
  const extraColor = isExtraGood ? "success.main" : "error.main";
  const differenceColor = getChangeColor(extra, isExtraGood);
  // The bar spans whichever is larger, so anything beyond what was expected shows as its own segment
  const total = Math.max(actual, expected);
  const metShare = total > 0 ? Math.min(actual, expected) / total : 0;
  const extraShare = total > 0 ? Math.max(extra, 0) / total : 0;
  return (
    <Stack spacing={0.75}>
      <Stack
        direction="row"
        spacing={2}
        sx={{ alignItems: "baseline", justifyContent: "space-between" }}
      >
        <Typography variant="subtitle2" noWrap sx={{ minWidth: 0 }}>
          {label}
        </Typography>
        <Typography
          variant="body2"
          sx={{ color: differenceColor, fontWeight: 500, whiteSpace: "nowrap" }}
        >
          {formatSignedCurrency(extra)}
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
        <Box sx={{ width: `${metShare * 100}%`, bgcolor: "primary.main" }} />
        <Box sx={{ width: `${extraShare * 100}%`, bgcolor: extraColor }} />
      </Box>
      <Typography variant="caption" color="text.secondary">
        {formatCurrency(actual)} of {formatCurrency(expected)}
      </Typography>
    </Stack>
  );
};

/**
 * Summarizes income, goal contributions, and spending for the most recent open accounting period.
 */
const OverviewPeriodCard = function ({
  period,
}: OverviewPeriodCardProps): JSX.Element {
  if (period === null) {
    return (
      <ContentSurface>
        <Stack spacing={1.5} sx={{ alignItems: "flex-start" }}>
          <Typography variant="h5">Current period</Typography>
          <Typography color="text.secondary">
            No accounting period is open.
          </Typography>
          <Link
            href={accountingPeriodRoutes.workspace({ returnUrl: "/" })}
            style={{ textDecoration: "none" }}
          >
            <Button component="span">Go to accounting periods</Button>
          </Link>
        </Stack>
      </ContentSurface>
    );
  }

  return (
    <ContentSurface>
      <Stack spacing={2} sx={{ alignItems: "stretch" }}>
        <Link
          href={accountingPeriodRoutes.workspaceDetail(
            period.accountingPeriod.id,
            { returnUrl: "/" },
          )}
          style={{ textDecoration: "none", color: "inherit" }}
        >
          <Stack
            direction="row"
            spacing={1}
            sx={{ alignItems: "center", justifyContent: "space-between" }}
          >
            <Typography variant="h5">{period.accountingPeriod.name}</Typography>
            <ChevronRight aria-hidden sx={{ color: "text.secondary" }} />
          </Stack>
        </Link>
        <PeriodProgressRow
          label="Tracked income"
          actual={period.actualTrackedIncome}
          expected={period.expectedTrackedIncome}
        />
        <PeriodProgressRow
          label="Goal contributions"
          actual={period.actualGoalContributions}
          expected={period.expectedGoalContributions}
        />
        <PeriodProgressRow
          label="Spending"
          actual={period.totalSpending}
          expected={period.actualTrackedIncome}
          isExtraGood={false}
        />
        <Stack
          direction="row"
          spacing={2}
          sx={{
            alignItems: "baseline",
            justifyContent: "space-between",
            borderTop: 1,
            borderColor: "divider",
            pt: 1.5,
          }}
        >
          <Typography variant="subtitle2">Net worth change</Typography>
          <Typography
            sx={{
              fontWeight: 500,
              color: getChangeColor(period.netWorthChange, true),
            }}
          >
            {formatSignedCurrency(period.netWorthChange)}
          </Typography>
        </Stack>
      </Stack>
    </ContentSurface>
  );
};

export default OverviewPeriodCard;
