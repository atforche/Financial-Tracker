"use client";

import { Box, Divider, Paper, Stack, Typography } from "@mui/material";
import { type CurrentAccounts, formatAccountType } from "@/accounts/types";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";

interface CurrentAccountsSummaryCardProps {
  readonly current: CurrentAccounts;
}

/**
 * Displays the current aggregate account balance with the visible type breakdown.
 */
const CurrentAccountsSummaryCard = function ({
  current,
}: CurrentAccountsSummaryCardProps): JSX.Element {
  const description =
    current.accounts.length === 0
      ? "Add an account to start building this snapshot."
      : null;

  return (
    <Paper
      sx={{
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        p: { xs: 2, md: 2.5 },
        background:
          "linear-gradient(180deg, rgba(12,74,110,0.06) 0%, rgba(255,255,255,0.98) 100%)",
        boxShadow: "0 8px 24px rgba(15, 23, 42, 0.04)",
      }}
    >
      <Stack spacing={2}>
        <Stack spacing={0.75}>
          <Typography
            variant="overline"
            sx={{
              color: "text.secondary",
              fontWeight: 700,
              letterSpacing: 1.1,
            }}
          >
            Current total balance
          </Typography>
          <Typography variant="h4">
            {formatCurrency(current.summary.totalBalance)}
          </Typography>
          {description === null ? null : (
            <Typography variant="body2" color="text.secondary">
              {description}
            </Typography>
          )}
        </Stack>
        <Box
          sx={{
            border: "1px solid",
            borderColor: "divider",
            borderRadius: 2,
            px: 1.5,
            py: 1.25,
            backgroundColor: "rgba(248, 250, 252, 0.9)",
          }}
        >
          <Stack spacing={1.25}>
            <Typography
              variant="body2"
              sx={{ color: "text.secondary", fontWeight: 600 }}
            >
              Balance by account type
            </Typography>
            {current.summary.balanceByAccountType.length === 0 ? (
              <Typography variant="body2" color="text.secondary">
                No account balances are available yet.
              </Typography>
            ) : (
              <Stack divider={<Divider flexItem />}>
                {current.summary.balanceByAccountType.map((balanceByType) => (
                  <Stack
                    key={balanceByType.accountType}
                    direction="row"
                    justifyContent="space-between"
                    alignItems="center"
                    gap={2}
                    sx={{ py: 0.65 }}
                  >
                    <Stack direction="row" alignItems="center" spacing={1}>
                      <Box
                        sx={{
                          width: 8,
                          height: 8,
                          borderRadius: "50%",
                          backgroundColor: "primary.main",
                          opacity: 0.75,
                        }}
                      />
                      <Typography variant="body2" color="text.secondary">
                        {formatAccountType(balanceByType.accountType)}
                      </Typography>
                    </Stack>
                    <Typography variant="body2" fontWeight={700}>
                      {formatCurrency(balanceByType.totalBalance)}
                    </Typography>
                  </Stack>
                ))}
              </Stack>
            )}
          </Stack>
        </Box>
      </Stack>
    </Paper>
  );
};

export default CurrentAccountsSummaryCard;
