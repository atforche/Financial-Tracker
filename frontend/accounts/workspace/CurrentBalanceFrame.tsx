"use client";

import { Box, Stack, Typography } from "@mui/material";
import {
  formatCurrency,
  formatSignedCurrency,
} from "@/framework/currencyHelpers";
import type { AccountWithBalance } from "@/accounts/types";
import Frame from "@/framework/view/Frame";
import type { JSX } from "react";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";

/**
 * Props for the CurrentBalanceFrame component.
 */
interface CurrentBalanceFrameProps {
  readonly account: AccountWithBalance;
}

/**
 * Shows the posted balance alongside the net amount of pending activity.
 */
const CurrentBalanceFrame = function ({
  account,
}: CurrentBalanceFrameProps): JSX.Element {
  const { postedBalance } = account.currentBalance;

  return (
    <Frame title="Current Balance" color="info">
      <Box sx={{ px: { xs: 1, md: 1.5 }, py: 1 }}>
        <ResponsiveGrid minimumColumnWidth={260} spacing={3}>
          <Stack spacing={0.75}>
            <Typography variant="body2" color="text.secondary">
              Posted Balance
            </Typography>
            <Typography
              component="p"
              variant="h4"
              sx={{ overflowWrap: "anywhere" }}
            >
              {formatCurrency(postedBalance)}
            </Typography>
          </Stack>
          <Stack spacing={0.75} justifyContent="center">
            <Typography variant="body2" color="text.secondary">
              Pending
            </Typography>
            <Typography
              component="p"
              variant="h5"
              sx={{ overflowWrap: "anywhere" }}
            >
              {formatSignedCurrency(account.pendingAmount)}
            </Typography>
          </Stack>
        </ResponsiveGrid>
      </Box>
    </Frame>
  );
};

export default CurrentBalanceFrame;
