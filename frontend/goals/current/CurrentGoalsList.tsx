"use client";

import {
  Box,
  Collapse,
  Divider,
  IconButton,
  LinearProgress,
  Paper,
  Stack,
  Typography,
} from "@mui/material";
import type {
  CurrentGoal,
  CurrentGoalBalanceEvent,
  CurrentGoalProgress,
  CurrentGoals,
} from "@/goals/types";
import { type JSX, useState } from "react";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import ExpandLess from "@mui/icons-material/ExpandLess";
import ExpandMore from "@mui/icons-material/ExpandMore";
import formatCurrency from "@/framework/formatCurrency";
import goalRoutes from "@/goals/routes";
import routes from "@/transactions/routes";
import { useRouter } from "next/navigation";

interface CurrentGoalsListProps {
  readonly current: CurrentGoals;
}

interface GoalProgressSummaryProps {
  readonly label: string;
  readonly progress: CurrentGoalProgress | null;
}

interface GoalEventListProps {
  readonly goalId: string;
  readonly emptyDescription: string;
  readonly progress: CurrentGoalProgress | null;
  readonly onOpenTransaction: (balanceEvent: CurrentGoalBalanceEvent) => void;
}

const dateFormatter = new Intl.DateTimeFormat("en-US", {
  month: "short",
  day: "numeric",
  year: "numeric",
});

const getProgressPercent = function (
  progress: CurrentGoalProgress | null,
): number {
  if (progress === null) {
    return 0;
  }
  if (progress.targetAmount === 0) {
    return 100;
  }
  return Math.min((progress.currentAmount / progress.targetAmount) * 100, 100);
};

const formatLastEvent = function (
  label: string,
  progress: CurrentGoalProgress | null,
): string {
  // eslint-disable-next-line @typescript-eslint/prefer-optional-chain
  if (progress === null || progress.lastBalanceEventDate === null) {
    return `No ${label.toLowerCase()} activity recorded yet.`;
  }
  return `Last ${label.toLowerCase()} activity: ${dateFormatter.format(
    new Date(`${progress.lastBalanceEventDate}T00:00:00`),
  )}`;
};

const getProgressBarColor = function (
  progress: CurrentGoalProgress | null,
): string {
  if (progress === null) {
    return "rgba(148, 163, 184, 0.9)";
  }
  return progress.isGoalMet ? "#16a34a" : "#dc2626";
};

const GoalProgressSummary = function ({
  label,
  progress,
}: GoalProgressSummaryProps): JSX.Element {
  return (
    <Stack spacing={1.1}>
      <Stack direction="row" justifyContent="space-between" gap={2}>
        <Typography variant="body2" fontWeight={700}>
          {label}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {progress === null
            ? "No goal"
            : `${formatCurrency(progress.currentAmount)} / ${formatCurrency(progress.targetAmount)}`}
        </Typography>
      </Stack>
      <LinearProgress
        variant="determinate"
        value={getProgressPercent(progress)}
        sx={{
          height: 10,
          borderRadius: 999,
          backgroundColor: "rgba(226, 232, 240, 0.75)",
          "& .MuiLinearProgress-bar": {
            borderRadius: 999,
            backgroundColor: getProgressBarColor(progress),
          },
        }}
      />
      <Stack direction="row" justifyContent="space-between" gap={2}>
        <Typography variant="caption" color="text.secondary">
          {formatLastEvent(label, progress)}
        </Typography>
        <Typography
          variant="caption"
          sx={{
            color:
              (progress?.isGoalMet ?? false)
                ? "success.dark"
                : "text.secondary",
            fontWeight: 700,
          }}
        >
          {progress === null
            ? "No goal"
            : progress.isGoalMet
              ? "Met"
              : "In progress"}
        </Typography>
      </Stack>
    </Stack>
  );
};

const GoalEventList = function ({
  goalId,
  emptyDescription,
  progress,
  onOpenTransaction,
}: GoalEventListProps): JSX.Element {
  if (progress === null) {
    return (
      <Typography variant="body2" color="text.secondary">
        This fund does not have a current goal for this section.
      </Typography>
    );
  }

  if (progress.recentBalanceEvents.length === 0) {
    return (
      <Typography variant="body2" color="text.secondary">
        {emptyDescription}
      </Typography>
    );
  }

  return (
    <Stack divider={<Divider flexItem />}>
      {progress.recentBalanceEvents.map((balanceEvent) => (
        <Stack
          key={`${goalId}-${balanceEvent.transactionId}`}
          direction="row"
          justifyContent="space-between"
          alignItems="center"
          gap={1.5}
          sx={{ py: 0.75 }}
        >
          <Stack spacing={0.15} sx={{ minWidth: 0 }}>
            <Typography variant="body2" fontWeight={600}>
              {dateFormatter.format(new Date(`${balanceEvent.date}T00:00:00`))}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {balanceEvent.isPosted ? "Posted" : "Pending"}
            </Typography>
          </Stack>
          <Stack direction="row" alignItems="center" spacing={0.25}>
            <Typography variant="body2" fontWeight={700}>
              {formatCurrency(balanceEvent.amount)}
            </Typography>
            <IconButton
              size="small"
              color="primary"
              onClick={() => {
                onOpenTransaction(balanceEvent);
              }}
              sx={{ backgroundColor: "rgba(15, 23, 42, 0.04)" }}
              aria-label={`Open transaction ${balanceEvent.transactionId}`}
            >
              <ArrowForwardOutlined fontSize="small" color="action" />
            </IconButton>
          </Stack>
        </Stack>
      ))}
    </Stack>
  );
};

const getFundTint = function (goal: CurrentGoal): string {
  return goal.fundName === "Unassigned"
    ? "rgba(14, 116, 144, 0.07)"
    : "rgba(245, 158, 11, 0.08)";
};

/**
 * Displays a list of current goal cards with expandable recent event sections.
 */
const CurrentGoalsList = function ({
  current,
}: CurrentGoalsListProps): JSX.Element {
  const router = useRouter();
  const [expandedFundIds, setExpandedFundIds] = useState<string[]>([]);

  const toggleGoal = function (fundId: string): void {
    setExpandedFundIds((currentFundIds) =>
      currentFundIds.includes(fundId)
        ? currentFundIds.filter((id) => id !== fundId)
        : [...currentFundIds, fundId],
    );
  };

  const openGoalWorkspace = function (goal: CurrentGoal): void {
    router.push(
      goalRoutes.workspace({
        ...(current.accountingPeriodId === null
          ? {}
          : { accountingPeriodIds: [current.accountingPeriodId] }),
        fundIds: [goal.fundId],
        view: "assignment",
      }),
    );
  };

  const openTransactionWorkspace = function (
    goal: CurrentGoal,
    balanceEvent: CurrentGoalBalanceEvent,
  ): void {
    router.push(
      routes.workspace({
        fundIds: [goal.fundId],
        selectedTransactionId: balanceEvent.transactionId,
      }),
    );
  };

  if (current.goals.length === 0) {
    return (
      <Paper
        sx={{
          border: "1px solid",
          borderColor: "divider",
          borderRadius: 3,
          p: { xs: 2, md: 2.5 },
          background:
            "linear-gradient(180deg, rgba(245,158,11,0.04) 0%, rgba(255,255,255,0.98) 100%)",
        }}
      >
        <Typography color="text.secondary">
          Current goal cards will appear here once a current accounting period
          and fund goals exist.
        </Typography>
      </Paper>
    );
  }

  return (
    <Box
      sx={{
        display: "grid",
        gap: 2,
        gridTemplateColumns: "repeat(auto-fit, minmax(min(100%, 360px), 1fr))",
      }}
    >
      {current.goals.map((goal) => {
        const isExpanded = expandedFundIds.includes(goal.fundId);

        return (
          <Paper
            key={goal.fundId}
            sx={{
              border: "1px solid",
              borderColor: "divider",
              borderRadius: 3,
              p: 2,
              background: `linear-gradient(180deg, ${getFundTint(goal)} 0%, rgba(255,255,255,0.98) 42%)`,
              boxShadow: "0 10px 26px rgba(15, 23, 42, 0.04)",
            }}
          >
            <Stack spacing={2}>
              <Stack direction="row" justifyContent="space-between" gap={2}>
                <Typography variant="h6" noWrap sx={{ minWidth: 0 }}>
                  {goal.fundName}
                </Typography>
                <Stack direction="row" alignItems="flex-start" spacing={0.5}>
                  <IconButton
                    size="small"
                    color="primary"
                    onClick={() => {
                      openGoalWorkspace(goal);
                    }}
                    sx={{ backgroundColor: "rgba(245, 158, 11, 0.10)" }}
                    aria-label={`Open ${goal.fundName}`}
                  >
                    <ArrowForwardOutlined fontSize="small" color="action" />
                  </IconButton>
                  <IconButton
                    size="small"
                    onClick={() => {
                      toggleGoal(goal.fundId);
                    }}
                    sx={{ backgroundColor: "rgba(15, 23, 42, 0.04)" }}
                    aria-label={
                      isExpanded
                        ? `Collapse ${goal.fundName} goal activity`
                        : `Expand ${goal.fundName} goal activity`
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
              <GoalProgressSummary
                label="Assignment"
                progress={goal.assignmentGoal}
              />
              <GoalProgressSummary
                label="Spending"
                progress={goal.spendingGoal}
              />
              <Collapse in={isExpanded} timeout="auto" unmountOnExit>
                <Stack spacing={1.5}>
                  <Divider />
                  <Stack spacing={0.9}>
                    <Typography
                      variant="overline"
                      sx={{ color: "text.secondary", fontWeight: 700 }}
                    >
                      Recent assignment events
                    </Typography>
                    <GoalEventList
                      goalId={
                        goal.assignmentGoal?.goalId ??
                        `${goal.fundId}-assignment`
                      }
                      emptyDescription="No recent assignment events are available for this fund."
                      progress={goal.assignmentGoal}
                      onOpenTransaction={(balanceEvent) => {
                        openTransactionWorkspace(goal, balanceEvent);
                      }}
                    />
                  </Stack>
                  <Stack spacing={0.9}>
                    <Typography
                      variant="overline"
                      sx={{ color: "text.secondary", fontWeight: 700 }}
                    >
                      Recent spending events
                    </Typography>
                    <GoalEventList
                      goalId={
                        goal.spendingGoal?.goalId ?? `${goal.fundId}-spending`
                      }
                      emptyDescription="No recent spending events are available for this fund."
                      progress={goal.spendingGoal}
                      onOpenTransaction={(balanceEvent) => {
                        openTransactionWorkspace(goal, balanceEvent);
                      }}
                    />
                  </Stack>
                </Stack>
              </Collapse>
            </Stack>
          </Paper>
        );
      })}
    </Box>
  );
};

export default CurrentGoalsList;
