import { Box, LinearProgress, Paper, Stack, Typography } from "@mui/material";
import type { JSX } from "react";
import type { OverviewData } from "@/overview/types";
import { formatAccountType } from "@/accounts/types";
import formatCurrency from "@/framework/formatCurrency";

/**
 * Props for the CompositionList component.
 */
interface CompositionListProps {
  readonly title: string;
  readonly subtitle: string;
  readonly rows: {
    label: string;
    value: number;
  }[];
}

/**
 * Displays a balance composition list with relative bars.
 */
const CompositionList = function ({
  title,
  subtitle,
  rows,
}: CompositionListProps): JSX.Element {
  const maxValue = Math.max(...rows.map((row) => Math.abs(row.value)), 0);

  return (
    <Stack spacing={2}>
      <Stack spacing={0.5}>
        <Typography variant="h6">{title}</Typography>
        <Typography variant="body2" color="text.secondary">
          {subtitle}
        </Typography>
      </Stack>
      {rows.length === 0 ? (
        <Typography color="text.secondary">No data available.</Typography>
      ) : (
        <Stack spacing={1.5}>
          {rows.map((row) => (
            <Stack key={row.label} spacing={0.75}>
              <Stack direction="row" justifyContent="space-between" gap={2}>
                <Typography variant="body2">{row.label}</Typography>
                <Typography variant="body2" fontWeight={600}>
                  {formatCurrency(row.value)}
                </Typography>
              </Stack>
              <LinearProgress
                variant="determinate"
                value={
                  maxValue === 0 ? 0 : (Math.abs(row.value) / maxValue) * 100
                }
                sx={{ height: 8, borderRadius: 999 }}
              />
            </Stack>
          ))}
        </Stack>
      )}
    </Stack>
  );
};

/**
 * Props for the OverviewCompositionPanel component.
 */
interface OverviewCompositionPanelProps {
  readonly data: OverviewData;
}

/**
 * Displays account and fund composition summaries side by side.
 */
const OverviewCompositionPanel = function ({
  data,
}: OverviewCompositionPanelProps): JSX.Element {
  const accountRows = data.accountSummary.balanceByAccountType.map((item) => ({
    label: formatAccountType(item.accountType),
    value: item.totalBalance,
  }));
  const fundRows = [
    {
      label: "Assigned",
      value: data.fundSummary.totalAssignedBalance,
    },
    {
      label: "Unassigned",
      value: data.fundSummary.totalUnassignedBalance,
    },
    {
      label: "Tracked Total",
      value: data.fundSummary.totalTrackedBalance,
    },
  ];

  return (
    <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
      <Stack spacing={3}>
        <Typography variant="h5">Financial composition</Typography>
        <Box
          sx={{
            display: "grid",
            gap: 3,
            gridTemplateColumns: {
              xs: "1fr",
              lg: "repeat(2, minmax(0, 1fr))",
            },
          }}
        >
          <CompositionList
            title="Balances by account type"
            subtitle="Posted balances grouped by the kind of account holding the money."
            rows={accountRows}
          />
          <CompositionList
            title="Fund allocation"
            subtitle="How tracked money is currently split between assigned and unassigned balances."
            rows={fundRows}
          />
        </Box>
      </Stack>
    </Paper>
  );
};

export default OverviewCompositionPanel;
