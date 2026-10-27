import { Button, Stack, Typography } from "@mui/material";
import ComparisonBarPair from "@/framework/view/ComparisonBarPair";
import ContentSurface from "@/framework/view/ContentSurface";
import type { JSX } from "react";
import Link from "next/link";
import accountingPeriodRoutes from "@/accounting-periods/routes";
import type { components } from "@/framework/data/api";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the OverviewPeriodCard component.
 */
interface OverviewPeriodCardProps {
  readonly period: components["schemas"]["OverviewPeriodModel"] | null;
}

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
            href={accountingPeriodRoutes.workspace({})}
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
        <Typography variant="h5">{period.accountingPeriod.name}</Typography>
        <Stack spacing={1}>
          <Typography variant="subtitle2">Income</Typography>
          <ComparisonBarPair
            first={{
              label: "Received",
              amount: period.actualIncome,
              color: "success.main",
              differenceLabel: "Shortfall",
              differenceColor: "error.main",
            }}
            second={{
              label: "Expected",
              amount: period.expectedIncome,
              color: "info.main",
              differenceLabel: "Surplus",
              differenceColor: "success.main",
            }}
          />
        </Stack>
        <Stack spacing={1}>
          <Typography variant="subtitle2">Goal contributions</Typography>
          <ComparisonBarPair
            first={{
              label: "Contributed",
              amount: period.actualGoalContributions,
              color: "success.main",
              differenceLabel: "Remaining",
              differenceColor: "error.main",
            }}
            second={{
              label: "Expected",
              amount: period.expectedGoalContributions,
              color: "info.main",
              differenceLabel: "Extra",
              differenceColor: "success.main",
            }}
          />
        </Stack>
        <Stack direction="row" sx={{ justifyContent: "space-between" }}>
          <Typography variant="subtitle2">Spending so far</Typography>
          <Typography>{formatCurrency(period.totalSpending)}</Typography>
        </Stack>
        <Link
          href={accountingPeriodRoutes.workspaceDetail(
            period.accountingPeriod.id,
            {},
          )}
          style={{ textDecoration: "none", alignSelf: "flex-start" }}
        >
          <Button component="span">Open period</Button>
        </Link>
      </Stack>
    </ContentSurface>
  );
};

export default OverviewPeriodCard;
