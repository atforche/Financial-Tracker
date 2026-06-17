import { Box, Stack, Typography } from "@mui/material";
import CurrentAccountsList from "@/accounts/current/CurrentAccountsList";
import type { CurrentAccounts as CurrentAccountsModel } from "@/accounts/types";
import CurrentAccountsSummaryCard from "@/accounts/current/CurrentAccountsSummaryCard";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";

const createEmptyCurrent = function (): CurrentAccountsModel {
  return {
    summary: {
      totalBalance: 0,
      totalTrackedBalance: 0,
      totalUntrackedBalance: 0,
      balanceByAccountType: [],
    },
    accounts: [],
  };
};

/**
 * Component that displays the current Accounts snapshot.
 */
const CurrentAccounts = async function (): Promise<JSX.Element> {
  const apiClient = getApiClient();
  const current: CurrentAccountsModel =
    (await apiClient.GET("/accounts/current")).data ?? createEmptyCurrent();

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
            "linear-gradient(135deg, rgba(12,74,110,0.08) 0%, rgba(255,255,255,0.96) 45%, rgba(15,118,110,0.05) 100%)",
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
            Accounts
          </Typography>
          <Typography variant="h5">Current Accounts</Typography>
          <Typography color="text.secondary">
            {current.accounts.length === 0
              ? "No accounts are available yet."
              : "Snapshot of current balances and recent account activity."}
          </Typography>
        </Stack>
      </Box>
      <CurrentAccountsSummaryCard current={current} />
      <CurrentAccountsList current={current} />
    </Stack>
  );
};

export default CurrentAccounts;
