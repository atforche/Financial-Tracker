import { Box, Stack, Typography } from "@mui/material";
import CurrentFundsList from "@/funds/current/CurrentFundsList";
import type { CurrentFunds as CurrentFundsModel } from "@/funds/types";
import CurrentFundsSummaryCard from "@/funds/current/CurrentFundsSummaryCard";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";

const createEmptyCurrent = function (): CurrentFundsModel {
  return {
    summary: {
      totalTrackedBalance: 0,
      totalAssignedBalance: 0,
      totalUnassignedBalance: 0,
    },
    funds: [],
  };
};

/**
 * Component that displays the current Funds snapshot.
 */
const CurrentFunds = async function (): Promise<JSX.Element> {
  const apiClient = getApiClient();
  const current: CurrentFundsModel =
    (await apiClient.GET("/funds/current")).data ?? createEmptyCurrent();

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Box
        sx={{
          maxWidth: 1440,
          width: "100%",
          border: "1px solid",
          borderColor: "divider",
          borderRadius: 3,
          px: { xs: 2, md: 3 },
          py: { xs: 2.5, md: 3 },
          background:
            "linear-gradient(135deg, rgba(15,23,42,0.05) 0%, rgba(255,255,255,0.96) 42%, rgba(16,185,129,0.08) 100%)",
        }}
      >
        <Stack spacing={1}>
          <Typography
            variant="overline"
            sx={{
              color: "text.secondary",
              letterSpacing: 1.4,
              fontWeight: 700,
            }}
          >
            Funds
          </Typography>
          <Typography variant="h5">Current Funds</Typography>
          <Typography color="text.secondary">
            {current.funds.length === 0
              ? "No funds are available yet."
              : "Snapshot of current fund balances and recent balance activity."}
          </Typography>
        </Stack>
      </Box>
      <CurrentFundsSummaryCard current={current} />
      <CurrentFundsList current={current} />
    </Stack>
  );
};

export default CurrentFunds;
