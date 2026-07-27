"use client";

import {
  Box,
  Collapse,
  IconButton,
  Stack,
  Tooltip,
  Typography,
} from "@mui/material";
import {
  EndingBalanceStatus,
  type FundGoalWithProgress,
  FundedBalanceStatus,
} from "@/fund-goals/types";
import Frame, { type FrameColor } from "@/framework/view/Frame";
import { type JSX, useState } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import ExpandLess from "@mui/icons-material/ExpandLess";
import ExpandMore from "@mui/icons-material/ExpandMore";
import FundGoalAvailableBalance from "@/fund-goals/workspace/FundGoalAvailableBalance";
import FundGoalProgressBars from "@/fund-goals/workspace/FundGoalProgressBars";
import KeyboardArrowRight from "@mui/icons-material/KeyboardArrowRight";
import { isNotNullOrUndefined } from "@/framework/nullHelpers";

/**
 * Props for the FundGoalWorkspaceCard component.
 */
interface FundGoalWorkspaceCardProps {
  readonly accountingPeriod: AccountingPeriod | null;
  readonly detailHref: string;
  readonly fundGoal: FundGoalWithProgress;
}

/**
 * Displays a compact Fund Goal summary with optional progress detail.
 */
const FundGoalWorkspaceCard = function ({
  accountingPeriod,
  detailHref,
  fundGoal,
}: FundGoalWorkspaceCardProps): JSX.Element {
  const [expanded, setExpanded] = useState(false);
  const toggleLabel = expanded
    ? "Hide progress details"
    : "Show progress details";
  const configured = [
    fundGoal.progress.availableBalance,
    isNotNullOrUndefined(fundGoal.regularContribution) &&
      fundGoal.progress.contribution,
    fundGoal.progress.fundedBalance?.minimumBalance,
    fundGoal.progress.fundedBalance?.maximumBalance,
    fundGoal.progress.endingBalance,
  ].filter(Boolean).length;
  const satisfied = [
    fundGoal.progress.availableBalance.isSatisfied,
    fundGoal.progress.contribution?.isSatisfied,
    fundGoal.progress.fundedBalance?.minimumBalance !== null &&
      fundGoal.progress.fundedBalance?.minimumBalance !== undefined &&
      fundGoal.progress.fundedBalance.status !==
        FundedBalanceStatus.BelowMinimum,
    fundGoal.progress.fundedBalance?.maximumBalance !== null &&
      fundGoal.progress.fundedBalance?.maximumBalance !== undefined &&
      fundGoal.progress.fundedBalance.status !==
        FundedBalanceStatus.AboveMaximum,
    fundGoal.progress.endingBalance?.status === EndingBalanceStatus.AtTarget,
  ].filter(Boolean).length;
  const color: FrameColor =
    configured === 0
      ? "primary"
      : satisfied === configured
        ? "success"
        : satisfied > 0
          ? "warning"
          : "error";

  return (
    <Frame
      title={fundGoal.fund.name}
      color={color}
      headerContent={
        <Tooltip title="View Fund Goal details">
          <IconButton
            aria-label="View Fund Goal details"
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
        <FundGoalAvailableBalance
          availableBalance={fundGoal.progress.availableBalance}
        />
        <Box>
          <Collapse in={expanded} unmountOnExit>
            <FundGoalProgressBars
              fundGoal={fundGoal}
              progress={fundGoal.progress}
              showAvailableBalance={false}
            />
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
              {satisfied} of {configured} goals met
            </Typography>
            <Tooltip title={toggleLabel}>
              <IconButton
                aria-label={toggleLabel}
                size="small"
                onClick={() => {
                  setExpanded((currentExpanded) => !currentExpanded);
                }}
                sx={{ visibility: configured === 1 ? "hidden" : "visible" }}
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
export default FundGoalWorkspaceCard;
