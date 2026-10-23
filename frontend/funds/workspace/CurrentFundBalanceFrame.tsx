"use client";

import { Box, Stack, Typography } from "@mui/material";
import {
  formatCurrency,
  formatSignedCurrency,
} from "@/framework/currencyHelpers";
import Frame from "@/framework/view/Frame";
import type { FundWithBalance } from "@/funds/types";
import type { JSX } from "react";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";

interface CurrentFundBalanceFrameProps {
  readonly fund: FundWithBalance;
}

/** Shows the posted balance alongside the net amount of pending activity. */
const CurrentFundBalanceFrame = function ({
  fund,
}: CurrentFundBalanceFrameProps): JSX.Element {
  const { postedBalance } = fund.currentBalance;

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
              {formatSignedCurrency(fund.pendingAmount)}
            </Typography>
          </Stack>
        </ResponsiveGrid>
      </Box>
    </Frame>
  );
};

export default CurrentFundBalanceFrame;
