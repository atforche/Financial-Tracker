import { Button, Chip, Divider, Paper, Stack, Typography } from "@mui/material";
import type { JSX } from "react";
import type { OverviewData } from "@/overview/types";
import formatCurrency from "@/framework/formatCurrency";
import transactionRoutes from "@/transactions/routes";

/**
 * Props for the OverviewCurrentPeriodPanel component.
 */
interface OverviewCurrentPeriodPanelProps {
  readonly data: OverviewData;
}

/**
 * Displays the current accounting period snapshot and direct navigation.
 */
const OverviewCurrentPeriodPanel = function ({
  data,
}: OverviewCurrentPeriodPanelProps): JSX.Element {
  const { currentAccountingPeriod, totalAccounts, totalFunds } = data;

  if (currentAccountingPeriod === null) {
    return (
      <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
        <Stack spacing={2}>
          <Typography variant="h5">Current period</Typography>
          <Typography color="text.secondary">
            Open a new accounting period to start entering transactions,
            assigning funds, and tracking this month&apos;s work in one place.
          </Typography>
        </Stack>
      </Paper>
    );
  }

  const accountingPeriodBasePath = `/accounting-periods/${currentAccountingPeriod.id}`;

  return (
    <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
      <Stack spacing={2.5}>
        <Stack
          direction={{ xs: "column", sm: "row" }}
          spacing={1.5}
          justifyContent="space-between"
          alignItems={{ xs: "flex-start", sm: "center" }}
        >
          <Stack spacing={0.75}>
            <Typography variant="h5">Current period</Typography>
            <Typography variant="h4">{currentAccountingPeriod.name}</Typography>
          </Stack>
          <Chip label="Open" color="success" variant="outlined" />
        </Stack>
        <Stack direction={{ xs: "column", sm: "row" }} spacing={4}>
          <Stack spacing={0.5} minWidth={180}>
            <Typography variant="overline" color="text.secondary">
              Opening balance
            </Typography>
            <Typography variant="h5">
              {formatCurrency(currentAccountingPeriod.openingBalance)}
            </Typography>
          </Stack>
          <Stack spacing={0.5} minWidth={180}>
            <Typography variant="overline" color="text.secondary">
              Current balance
            </Typography>
            <Typography variant="h5">
              {formatCurrency(currentAccountingPeriod.closingBalance)}
            </Typography>
          </Stack>
        </Stack>
        <Divider />
        <Stack direction={{ xs: "column", md: "row" }} spacing={3}>
          <Stack spacing={0.5} flex={1}>
            <Typography variant="overline" color="text.secondary">
              Period scope
            </Typography>
            <Typography color="text.secondary">
              {totalAccounts} accounts and {totalFunds} funds are currently
              available to work with across the tracker.
            </Typography>
          </Stack>
          <Stack direction="row" spacing={1.5} flexWrap="wrap" useFlexGap>
            <Button
              variant="outlined"
              href={`${accountingPeriodBasePath}?display=accounts`}
            >
              Accounts
            </Button>
            <Button
              variant="outlined"
              href={`${accountingPeriodBasePath}?display=funds`}
            >
              Funds
            </Button>
            <Button
              variant="outlined"
              href={`${accountingPeriodBasePath}?display=transactions`}
            >
              Transactions
            </Button>
            <Button
              variant="contained"
              href={transactionRoutes.create({
                accountingPeriodId: currentAccountingPeriod.id,
              })}
            >
              Add transaction
            </Button>
          </Stack>
        </Stack>
      </Stack>
    </Paper>
  );
};

export default OverviewCurrentPeriodPanel;
