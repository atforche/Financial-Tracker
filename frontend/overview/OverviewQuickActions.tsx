import { Button, Paper, Stack, Typography } from "@mui/material";
import type { JSX } from "react";
import type { OverviewData } from "@/overview/types";
import accountRoutes from "@/accounts/routes";
import accountingPeriodRoutes from "@/accounting-periods/routes";
import goalRoutes from "@/goals/routes";
import transactionRoutes from "@/transactions/routes";

/**
 * Represents a quick action card on the overview page.
 */
interface QuickAction {
  readonly title: string;
  readonly description: string;
  readonly href: string;
  readonly buttonLabel: string;
}

/**
 * Props for the OverviewQuickActions component.
 */
interface OverviewQuickActionsProps {
  readonly data: OverviewData;
}

/**
 * Displays the quick-start action grid for the overview page.
 */
const OverviewQuickActions = function ({
  data,
}: OverviewQuickActionsProps): JSX.Element {
  const { currentAccountingPeriod } = data;

  const actions: QuickAction[] =
    currentAccountingPeriod === null
      ? [
          {
            title: "Open the next period",
            description:
              "Create the next accounting period before entering fresh activity.",
            href: accountingPeriodRoutes.create,
            buttonLabel: "Create period",
          },
          {
            title: "Review past periods",
            description:
              "Inspect prior balances and drill into historical monthly snapshots.",
            href: accountingPeriodRoutes.index({}),
            buttonLabel: "View periods",
          },
          {
            title: "Check account balances",
            description:
              "Review tracked and untracked balances across all accounts.",
            href: accountRoutes.dashboard({}),
            buttonLabel: "View accounts",
          },
        ]
      : [
          {
            title: "Record activity",
            description:
              "Start a new transaction inside the current accounting period.",
            href: transactionRoutes.create({
              accountingPeriodId: currentAccountingPeriod.id,
            }),
            buttonLabel: "Create transaction",
          },
          {
            title: "Add a new account",
            description:
              "Create another account and tie it to the active accounting period.",
            href: accountRoutes.workspace({ action: "create" }),
            buttonLabel: "Create account",
          },
          {
            title: "Define a goal",
            description:
              "Create a goal tied to the active period and an existing fund.",
            href: goalRoutes.create({
              accountingPeriodId: currentAccountingPeriod.id,
            }),
            buttonLabel: "Create goal",
          },
          {
            title: "Audit all accounts",
            description:
              "Review the full account list, balances, and account-type totals.",
            href: accountRoutes.dashboard({}),
            buttonLabel: "Open accounts",
          },
        ];

  return (
    <Stack spacing={2}>
      <Typography variant="h5">What to do next</Typography>
      <Stack
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
        {actions.map((action) => (
          <Paper
            key={action.title}
            sx={{ border: "1px solid", borderColor: "divider", p: 2.5 }}
          >
            <Stack spacing={2} height="100%" justifyContent="space-between">
              <Stack spacing={0.75}>
                <Typography variant="h6">{action.title}</Typography>
                <Typography color="text.secondary">
                  {action.description}
                </Typography>
              </Stack>
              <Button variant="outlined" href={action.href}>
                {action.buttonLabel}
              </Button>
            </Stack>
          </Paper>
        ))}
      </Stack>
    </Stack>
  );
};

export default OverviewQuickActions;
