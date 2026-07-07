"use client";

import {
  AccountTrendsBalanceEventType,
  AccountType,
  type CurrentAccount,
  type CurrentAccountBalanceEvent,
  type CurrentAccounts,
} from "@/accounts/types";
import {
  Box,
  Collapse,
  Divider,
  IconButton,
  Paper,
  Stack,
  Typography,
} from "@mui/material";
import { type JSX, useState } from "react";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import ExpandLess from "@mui/icons-material/ExpandLess";
import ExpandMore from "@mui/icons-material/ExpandMore";
import accountRoutes from "@/accounts/routes";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/transactions/routes";
import { useRouter } from "next/navigation";

interface CurrentAccountsListProps {
  readonly current: CurrentAccounts;
}

const dateFormatter = new Intl.DateTimeFormat("en-US", {
  month: "short",
  day: "numeric",
  year: "numeric",
});

const formatEventType = function (
  balanceEvent: CurrentAccountBalanceEvent,
): string {
  const baseLabel =
    balanceEvent.type === AccountTrendsBalanceEventType.Debit
      ? "Debit"
      : "Credit";
  return balanceEvent.isPosted
    ? baseLabel
    : `Pending ${baseLabel.toLowerCase()}`;
};

const getEventColor = function (
  balanceEvent: CurrentAccountBalanceEvent,
): string {
  return balanceEvent.type === AccountTrendsBalanceEventType.Debit
    ? "warning.dark"
    : "info.dark";
};

const getAccountTint = function (account: CurrentAccount): string {
  switch (account.type) {
    case AccountType.CreditCard:
    case AccountType.Debt:
      return "rgba(180, 83, 9, 0.08)";
    case AccountType.Investment:
    case AccountType.Retirement:
      return "rgba(21, 128, 61, 0.08)";
    case AccountType.Escrow:
      return "rgba(8, 145, 178, 0.08)";
    case AccountType.Standard:
      return "rgba(12, 74, 110, 0.06)";
    default:
      return "rgba(12, 74, 110, 0.06)";
  }
};

/**
 * Displays a list of current accounts with expandable balance events.
 */
const CurrentAccountsList = function ({
  current,
}: CurrentAccountsListProps): JSX.Element {
  const router = useRouter();
  const [expandedAccountIds, setExpandedAccountIds] = useState<string[]>([]);

  const toggleAccount = function (accountId: string): void {
    setExpandedAccountIds((currentAccountIds) =>
      currentAccountIds.includes(accountId)
        ? currentAccountIds.filter((id) => id !== accountId)
        : [...currentAccountIds, accountId],
    );
  };

  const openAccountWorkspace = function (account: CurrentAccount): void {
    router.push(accountRoutes.workspaceDetail(account.id, {}));
  };

  const openTransactionWorkspace = function (
    account: CurrentAccount,
    balanceEvent: CurrentAccountBalanceEvent,
  ): void {
    router.push(
      routes.workspace({
        accountIds: [account.id],
        selectedTransactionId: balanceEvent.transactionId,
      }),
    );
  };

  if (current.accounts.length === 0) {
    return (
      <Paper
        sx={{
          border: "1px solid",
          borderColor: "divider",
          borderRadius: 3,
          p: { xs: 2, md: 2.5 },
          background:
            "linear-gradient(180deg, rgba(12,74,110,0.04) 0%, rgba(255,255,255,0.98) 100%)",
        }}
      >
        <Typography color="text.secondary">
          Current account cards will appear here once accounts have been added.
        </Typography>
      </Paper>
    );
  }

  return (
    <Box
      sx={{
        display: "grid",
        gap: 2,
        gridTemplateColumns: "repeat(auto-fit, minmax(min(100%, 320px), 1fr))",
      }}
    >
      {current.accounts.map((account) => {
        const isExpanded = expandedAccountIds.includes(account.id);
        const { lastBalanceEventDate } = account;

        return (
          <Paper
            key={account.id}
            sx={{
              border: "1px solid",
              borderColor: "divider",
              borderRadius: 3,
              p: 2,
              background: `linear-gradient(180deg, ${getAccountTint(account)} 0%, rgba(255,255,255,0.98) 42%)`,
              boxShadow: "0 10px 26px rgba(15, 23, 42, 0.04)",
            }}
          >
            <Stack spacing={2}>
              <Stack direction="row" justifyContent="space-between" gap={2}>
                <Stack spacing={0.75} sx={{ minWidth: 0 }}>
                  <Typography variant="h6" noWrap>
                    {account.name}
                  </Typography>
                </Stack>
                <Stack direction="row" alignItems="flex-start" spacing={0.5}>
                  <IconButton
                    size="small"
                    color="primary"
                    onClick={() => {
                      openAccountWorkspace(account);
                    }}
                    sx={{ backgroundColor: "rgba(12, 74, 110, 0.06)" }}
                    aria-label={`Open ${account.name}`}
                  >
                    <ArrowForwardOutlined fontSize="small" color="action" />
                  </IconButton>
                  <IconButton
                    size="small"
                    onClick={() => {
                      toggleAccount(account.id);
                    }}
                    sx={{ backgroundColor: "rgba(15, 23, 42, 0.04)" }}
                    aria-label={
                      isExpanded
                        ? `Collapse ${account.name} balance events`
                        : `Expand ${account.name} balance events`
                    }
                  >
                    {isExpanded ? (
                      <ExpandLess fontSize="small" />
                    ) : (
                      <ExpandMore fontSize="small" />
                    )}
                  </IconButton>
                </Stack>
              </Stack>
              <Stack spacing={0.5}>
                <Typography
                  variant="overline"
                  sx={{ color: "text.secondary", fontWeight: 700 }}
                >
                  Current Balance
                </Typography>
                <Typography variant="h4">
                  {formatCurrency(account.currentBalance.postedBalance)}
                </Typography>
              </Stack>
              <Box
                sx={{
                  borderRadius: 2,
                  px: 1.25,
                  py: 1,
                  backgroundColor: "rgba(248, 250, 252, 0.9)",
                }}
              >
                <Typography variant="body2" color="text.secondary">
                  {lastBalanceEventDate === null
                    ? "No balance events recorded yet."
                    : `Last balance event: ${dateFormatter.format(
                        new Date(`${lastBalanceEventDate}T00:00:00`),
                      )}`}
                </Typography>
              </Box>
              <Collapse in={isExpanded} timeout="auto" unmountOnExit>
                <Stack spacing={1.25}>
                  <Divider />
                  {account.recentBalanceEvents.length === 0 ? (
                    <Typography variant="body2" color="text.secondary">
                      No recent balance events are available for this account.
                    </Typography>
                  ) : (
                    <Stack spacing={1}>
                      {account.recentBalanceEvents.map((balanceEvent) => (
                        <Box
                          key={`${account.id}-${balanceEvent.transactionId}-${balanceEvent.type}`}
                          sx={{
                            borderRadius: 2,
                            px: 1.25,
                            py: 1,
                            backgroundColor: "rgba(248, 250, 252, 0.92)",
                            border: "1px solid rgba(148, 163, 184, 0.18)",
                          }}
                        >
                          <Stack
                            direction="row"
                            justifyContent="space-between"
                            alignItems="center"
                            gap={1.5}
                          >
                            <Stack spacing={0.25} sx={{ minWidth: 0 }}>
                              <Typography variant="body2" fontWeight={600}>
                                {dateFormatter.format(
                                  new Date(`${balanceEvent.date}T00:00:00`),
                                )}
                              </Typography>
                              <Typography
                                variant="caption"
                                sx={{
                                  color: getEventColor(balanceEvent),
                                  fontWeight: 700,
                                }}
                              >
                                {formatEventType(balanceEvent)}
                              </Typography>
                            </Stack>
                            <Stack
                              direction="row"
                              alignItems="center"
                              spacing={0.25}
                            >
                              <Typography variant="body2" fontWeight={700}>
                                {formatCurrency(balanceEvent.amount)}
                              </Typography>
                              <IconButton
                                size="small"
                                color="primary"
                                onClick={() => {
                                  openTransactionWorkspace(
                                    account,
                                    balanceEvent,
                                  );
                                }}
                                sx={{
                                  backgroundColor: "rgba(12, 74, 110, 0.06)",
                                }}
                                aria-label={`Open transaction ${balanceEvent.transactionId}`}
                              >
                                <ArrowForwardOutlined
                                  fontSize="small"
                                  color="action"
                                />
                              </IconButton>
                            </Stack>
                          </Stack>
                        </Box>
                      ))}
                    </Stack>
                  )}
                </Stack>
              </Collapse>
            </Stack>
          </Paper>
        );
      })}
    </Box>
  );
};

export default CurrentAccountsList;
