import { Button, Paper, Stack, Typography } from "@mui/material";
import type { JSX } from "react";
import type { OverviewData } from "@/overview/types";
import accountRoutes from "@/accounts/routes";
import accountingPeriodRoutes from "@/accounting-periods/routes";
import fundRoutes from "@/funds/routes";
import goalRoutes from "@/goals/routes";
import transactionRoutes from "@/transactions/routes";

/**
 * Props for the OverviewHero component.
 */
interface OverviewHeroProps {
  readonly data: OverviewData;
}

/**
 * Displays the primary status and top actions for the overview page.
 */
const OverviewHero = function ({ data }: OverviewHeroProps): JSX.Element {
  const { currentAccountingPeriod, openAccountingPeriods } = data;
  const hasOpenAccountingPeriod = currentAccountingPeriod !== null;

  return (
    <Paper
      sx={{
        backgroundColor: "background.paper",
        backgroundImage:
          "linear-gradient(135deg, rgba(66, 165, 245, 0.22) 0%, rgba(255, 255, 255, 0) 58%)",
        border: "1px solid",
        borderColor: "divider",
        overflow: "hidden",
        p: { xs: 3, md: 4 },
        position: "relative",
      }}
    >
      <Stack spacing={3}>
        <Stack spacing={1}>
          <Typography variant="overline" color="text.secondary">
            Financial Tracker
          </Typography>
          <Typography variant="h3">Overview</Typography>
          <Typography color="text.secondary" maxWidth={780}>
            {hasOpenAccountingPeriod
              ? `${currentAccountingPeriod.name} is your active accounting period. Use this page to monitor balances, jump into the current period, and move straight into common workflows.`
              : "There is no open accounting period right now. Use the overview page to review the current financial position and open the next working period when you are ready."}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {hasOpenAccountingPeriod
              ? `${openAccountingPeriods.length} open accounting period${openAccountingPeriods.length === 1 ? "" : "s"} tracked.`
              : "Historical balances remain available across accounts, funds, and previous accounting periods."}
          </Typography>
        </Stack>
        <Stack direction="row" spacing={1.5} flexWrap="wrap" useFlexGap>
          {hasOpenAccountingPeriod ? (
            <>
              <Button
                variant="contained"
                href={transactionRoutes.create({
                  accountingPeriodId: currentAccountingPeriod.id,
                })}
              >
                Create transaction
              </Button>
              <Button
                variant="outlined"
                href={accountRoutes.create({
                  accountingPeriodId: currentAccountingPeriod.id,
                })}
              >
                Create account
              </Button>
              <Button
                variant="outlined"
                href={fundRoutes.create({
                  accountingPeriodId: currentAccountingPeriod.id,
                })}
              >
                Create fund
              </Button>
              <Button
                variant="outlined"
                href={goalRoutes.create({
                  accountingPeriodId: currentAccountingPeriod.id,
                })}
              >
                Create goal
              </Button>
            </>
          ) : (
            <>
              <Button variant="contained" href={accountingPeriodRoutes.create}>
                Create accounting period
              </Button>
              <Button
                variant="outlined"
                href={accountingPeriodRoutes.index({})}
              >
                Browse accounting periods
              </Button>
              <Button variant="outlined" href={accountRoutes.index({})}>
                Review accounts
              </Button>
              <Button variant="outlined" href={fundRoutes.index({})}>
                Review funds
              </Button>
            </>
          )}
        </Stack>
      </Stack>
    </Paper>
  );
};

export default OverviewHero;
