"use client";

import { Divider, Stack, Typography } from "@mui/material";
import { type JSX, useState } from "react";
import type { AccountOverviewSummary } from "@/overview/types";
import ExpandableSummaryCard from "@/framework/view/ExpandableSummaryCard";
import { formatAccountType } from "@/accounts/helpers";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Props for the AccountOverview component.
 */
interface AccountOverviewProps {
  readonly summary: AccountOverviewSummary;
}

/**
 * Overview component for Accounts.
 */
const AccountOverview = function ({
  summary,
}: AccountOverviewProps): JSX.Element {
  const [expanded, setExpanded] = useState(false);

  return (
    <ExpandableSummaryCard
      title="Current Total Account Balances"
      value={formatCurrency(summary.totalBalance)}
      expanded={expanded}
      onToggle={() => {
        setExpanded((current) => !current);
      }}
    >
      <Stack spacing={1.25} divider={<Divider flexItem />}>
        {summary.balanceByAccountType.map((item) => (
          <Stack
            key={item.accountType}
            direction="row"
            justifyContent="space-between"
            gap={2}
          >
            <Typography variant="body2">
              {formatAccountType(item.accountType)}
            </Typography>
            <Typography variant="body2" fontWeight={600}>
              {formatCurrency(item.totalBalance)}
            </Typography>
          </Stack>
        ))}
      </Stack>
    </ExpandableSummaryCard>
  );
};

export default AccountOverview;
