"use client";

import {
  Collapse,
  Divider,
  IconButton,
  Paper,
  Stack,
  Typography,
} from "@mui/material";
import { type JSX, useState } from "react";
import ExpandMore from "@mui/icons-material/ExpandMore";
import type { OverviewData } from "@/overview/types";
import { formatAccountType } from "@/accounts/types";
import formatCurrency from "@/framework/formatCurrency";

interface AccountOverviewProps {
  readonly data: OverviewData;
}

/**
 * Overview component for Accounts.
 */
const AccountOverview = function ({ data }: AccountOverviewProps): JSX.Element {
  const [expanded, setExpanded] = useState(false);

  return (
    <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
      <Stack spacing={2}>
        <Typography variant="h6" color="text.secondary">
          Current Total Account Balances
        </Typography>

        <Stack spacing={1} sx={{ pt: 0.5 }}>
          <Stack direction="row" justifyContent="space-between">
            <Typography variant="h4">
              {formatCurrency(data.accountSummary.totalBalance)}
            </Typography>
            <IconButton
              size="small"
              onClick={() => {
                setExpanded((current) => !current);
              }}
              aria-label={
                expanded
                  ? "Hide account balance breakdown"
                  : "Show account balance breakdown"
              }
              sx={{
                transform: expanded ? "rotate(180deg)" : "rotate(0deg)",
                transition: "transform 0.2s ease-in-out",
              }}
            >
              <ExpandMore fontSize="small" />
            </IconButton>
          </Stack>
          <Collapse in={expanded} timeout="auto" unmountOnExit>
            <Stack
              spacing={1.25}
              divider={<Divider flexItem />}
              sx={{ pt: 0.5 }}
            >
              {data.accountSummary.balanceByAccountType.map((item) => (
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
          </Collapse>
        </Stack>
      </Stack>
    </Paper>
  );
};

export default AccountOverview;
