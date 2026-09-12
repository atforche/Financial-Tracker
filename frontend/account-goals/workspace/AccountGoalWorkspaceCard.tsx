"use client";

import {
  Box,
  Collapse,
  Divider,
  IconButton,
  Stack,
  Tooltip,
  Typography,
} from "@mui/material";
import Frame, { type FrameColor } from "@/framework/view/Frame";
import { type JSX, useState } from "react";
import AccountGoalEndingBalanceRange from "@/account-goals/workspace/AccountGoalEndingBalanceRange";
import type { AccountGoalWithProgress } from "@/account-goals/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import ExpandLess from "@mui/icons-material/ExpandLess";
import ExpandMore from "@mui/icons-material/ExpandMore";
import KeyboardArrowRight from "@mui/icons-material/KeyboardArrowRight";
import { formatCurrency } from "@/framework/currencyHelpers";
import { getAccountGoalDimensionSummary } from "@/account-goals/helpers";

interface AccountGoalWorkspaceCardProps {
  readonly accountingPeriod: AccountingPeriod | null;
  readonly accountGoal: AccountGoalWithProgress;
  readonly detailHref: string;
}

/**
 * Displays a compact Account Goal summary with optional progress detail.
 */
const AccountGoalWorkspaceCard = function ({
  accountingPeriod,
  accountGoal,
  detailHref,
}: AccountGoalWorkspaceCardProps): JSX.Element {
  const [expanded, setExpanded] = useState(false);
  const summary = getAccountGoalDimensionSummary(
    accountGoal.progress,
    accountGoal.maximumEndingBalance !== null &&
      accountGoal.maximumEndingBalance !== undefined,
  );
  const color: FrameColor =
    summary.satisfied === summary.configured
      ? "success"
      : summary.satisfied > 0
        ? "warning"
        : "error";
  const toggleLabel = expanded
    ? "Hide progress details"
    : "Show progress details";
  return (
    <Frame
      title={accountGoal.account.name}
      color={color}
      headerContentInline
      headerContent={
        <Tooltip title="View Account Goal Details">
          <IconButton
            aria-label="View Account Goal details"
            size="small"
            href={detailHref}
          >
            <KeyboardArrowRight />
          </IconButton>
        </Tooltip>
      }
    >
      <Stack spacing={2}>
        <Typography variant="body2" color="text.secondary">
          {accountingPeriod?.name ?? "No accounting period"}
        </Typography>
        <Stack direction="row" justifyContent="space-between" gap={2}>
          <Typography variant="body2" fontWeight={700}>
            Ending Balance
          </Typography>
          <Typography variant="body2" fontWeight={700}>
            {formatCurrency(accountGoal.progress.endingBalance.currentBalance)}
          </Typography>
        </Stack>
        <Box>
          <Collapse in={expanded} unmountOnExit>
            <Stack spacing={2}>
              <Divider />
              <AccountGoalEndingBalanceRange
                endingBalance={accountGoal.progress.endingBalance}
              />
            </Stack>
          </Collapse>
          <Stack
            direction="row"
            alignItems="center"
            justifyContent="space-between"
            sx={(theme) => ({
              mt: expanded ? 2 : 0,
              transition: theme.transitions.create("margin-top"),
            })}
          >
            <Typography variant="body2" color="text.secondary">
              {summary.satisfied} of {summary.configured} Account Goals met
            </Typography>
            <Tooltip title={toggleLabel}>
              <IconButton
                aria-label={toggleLabel}
                size="small"
                onClick={() => {
                  setExpanded((currentExpanded) => !currentExpanded);
                }}
              >
                {expanded ? <ExpandLess /> : <ExpandMore />}
              </IconButton>
            </Tooltip>
          </Stack>
        </Box>
      </Stack>
    </Frame>
  );
};

export default AccountGoalWorkspaceCard;
