"use client";

import {
  Box,
  Collapse,
  Divider,
  IconButton,
  Paper,
  Stack,
  Typography,
} from "@mui/material";
import {
  type CurrentFund,
  type CurrentFundBalanceEvent,
  type CurrentFunds,
  FundTrendsBalanceEventType,
} from "@/funds/types";
import { type JSX, useState } from "react";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import ExpandLess from "@mui/icons-material/ExpandLess";
import ExpandMore from "@mui/icons-material/ExpandMore";
import formatCurrency from "@/framework/formatCurrency";
import fundRoutes from "@/funds/routes";
import routes from "@/transactions/routes";
import { useRouter } from "next/navigation";

interface CurrentFundsListProps {
  readonly current: CurrentFunds;
}

const dateFormatter = new Intl.DateTimeFormat("en-US", {
  month: "short",
  day: "numeric",
  year: "numeric",
});

const formatEventType = function (
  balanceEvent: CurrentFundBalanceEvent,
): string {
  const baseLabel =
    balanceEvent.type === FundTrendsBalanceEventType.Debit ? "Debit" : "Credit";
  return balanceEvent.isPosted
    ? baseLabel
    : `Pending ${baseLabel.toLowerCase()}`;
};

const getEventColor = function (balanceEvent: CurrentFundBalanceEvent): string {
  return balanceEvent.type === FundTrendsBalanceEventType.Debit
    ? "warning.dark"
    : "info.dark";
};

const getFundTint = function (fund: CurrentFund): string {
  return fund.name === "Unassigned"
    ? "rgba(14, 116, 144, 0.07)"
    : "rgba(16, 185, 129, 0.08)";
};

/**
 * Displays a list of current funds with expandable recent balance events.
 */
const CurrentFundsList = function ({
  current,
}: CurrentFundsListProps): JSX.Element {
  const router = useRouter();
  const [expandedFundIds, setExpandedFundIds] = useState<string[]>([]);

  const toggleFund = function (fundId: string): void {
    setExpandedFundIds((currentFundIds) =>
      currentFundIds.includes(fundId)
        ? currentFundIds.filter((id) => id !== fundId)
        : [...currentFundIds, fundId],
    );
  };

  const openFundWorkspace = function (fund: CurrentFund): void {
    router.push(fundRoutes.workspace({ selectedFundId: fund.id }));
  };

  const openTransactionWorkspace = function (
    fund: CurrentFund,
    balanceEvent: CurrentFundBalanceEvent,
  ): void {
    router.push(
      routes.workspace({
        fundIds: [fund.id],
        selectedTransactionId: balanceEvent.transactionId,
      }),
    );
  };

  if (current.funds.length === 0) {
    return (
      <Paper
        sx={{
          border: "1px solid",
          borderColor: "divider",
          borderRadius: 3,
          p: { xs: 2, md: 2.5 },
          background:
            "linear-gradient(180deg, rgba(16,185,129,0.04) 0%, rgba(255,255,255,0.98) 100%)",
        }}
      >
        <Typography color="text.secondary">
          Current fund cards will appear here once funds have been added.
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
      {current.funds.map((fund) => {
        const isExpanded = expandedFundIds.includes(fund.id);
        const { lastBalanceEventDate } = fund;

        return (
          <Paper
            key={fund.id}
            sx={{
              border: "1px solid",
              borderColor: "divider",
              borderRadius: 3,
              p: 2,
              background: `linear-gradient(180deg, ${getFundTint(fund)} 0%, rgba(255,255,255,0.98) 42%)`,
              boxShadow: "0 10px 26px rgba(15, 23, 42, 0.04)",
            }}
          >
            <Stack spacing={2}>
              <Stack direction="row" justifyContent="space-between" gap={2}>
                <Stack spacing={0.75} sx={{ minWidth: 0 }}>
                  <Typography variant="h6" noWrap>
                    {fund.name}
                  </Typography>
                </Stack>
                <Stack direction="row" alignItems="flex-start" spacing={0.5}>
                  <IconButton
                    size="small"
                    color="primary"
                    onClick={() => {
                      openFundWorkspace(fund);
                    }}
                    sx={{ backgroundColor: "rgba(16, 185, 129, 0.08)" }}
                    aria-label={`Open ${fund.name}`}
                  >
                    <ArrowForwardOutlined fontSize="small" color="action" />
                  </IconButton>
                  <IconButton
                    size="small"
                    onClick={() => {
                      toggleFund(fund.id);
                    }}
                    sx={{ backgroundColor: "rgba(15, 23, 42, 0.04)" }}
                    aria-label={
                      isExpanded
                        ? `Collapse ${fund.name} balance events`
                        : `Expand ${fund.name} balance events`
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
                  {formatCurrency(fund.currentBalance.postedBalance)}
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
                  {fund.recentBalanceEvents.length === 0 ? (
                    <Typography variant="body2" color="text.secondary">
                      No recent balance events are available for this fund.
                    </Typography>
                  ) : (
                    <Stack spacing={0.25}>
                      {fund.recentBalanceEvents.map((balanceEvent, index) => (
                        <Box
                          key={`${fund.id}-${balanceEvent.transactionId}-${index}`}
                        >
                          {index === 0 ? null : <Divider />}
                          <Stack
                            direction="row"
                            justifyContent="space-between"
                            alignItems="center"
                            gap={1.5}
                            sx={{ py: 1 }}
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
                                  openTransactionWorkspace(fund, balanceEvent);
                                }}
                                sx={{
                                  backgroundColor: "rgba(16, 185, 129, 0.08)",
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

export default CurrentFundsList;
