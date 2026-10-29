import { Button, Stack, Typography } from "@mui/material";
import ChevronRight from "@mui/icons-material/ChevronRight";
import ContentSurface from "@/framework/view/ContentSurface";
import type { JSX } from "react";
import Link from "next/link";
import PeriodProgressRow from "@/accounting-periods/PeriodProgressRow";
import SignedChange from "@/framework/view/SignedChange";
import accountingPeriodRoutes from "@/accounting-periods/routes";
import type { components } from "@/framework/data/api";

/**
 * Props for the OverviewPeriodCard component.
 */
interface OverviewPeriodCardProps {
  readonly period: components["schemas"]["OverviewPeriodModel"] | null;
  readonly returnUrl: string;
}

/**
 * Summarizes income, goal contributions, and spending for the most recent open accounting period.
 */
const OverviewPeriodCard = function ({
  period,
  returnUrl,
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
            href={accountingPeriodRoutes.workspace({ returnUrl })}
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
            { returnUrl },
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
          <Typography>
            <SignedChange amount={period.netWorthChange} />
          </Typography>
        </Stack>
      </Stack>
    </ContentSurface>
  );
};

export default OverviewPeriodCard;
