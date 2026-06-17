import { Box, Paper, Stack, Typography } from "@mui/material";
import type { CurrentGoals } from "@/goals/types";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";

interface CurrentGoalsSummaryCardsProps {
  readonly current: CurrentGoals;
}

const formatPercentage = function (value: number): string {
  return `${value.toFixed(2)}%`;
};

/**
 * Displays the summary cards for the current goals page.
 */
const CurrentGoalsSummaryCards = function ({
  current,
}: CurrentGoalsSummaryCardsProps): JSX.Element {
  const cards = [
    {
      key: "assignment",
      label: "Assignment",
      title: "Current assignment progress",
      tint: "rgba(245, 158, 11, 0.10)",
      amountLabel: "Assigned",
      amountValue: formatCurrency(current.summary.totalAmountAssigned),
      targetLabel: "Target",
      targetValue: formatCurrency(current.summary.totalAmountToAssign),
      goalsMet: `${current.summary.percentageOfAssignmentGoalsMet.metCount} / ${current.summary.percentageOfAssignmentGoalsMet.totalCount}`,
      percentMet: formatPercentage(
        current.summary.percentageOfAssignmentGoalsMet.percentageMet,
      ),
    },
    {
      key: "spending",
      label: "Spending",
      title: "Current spending progress",
      tint: "rgba(14, 165, 233, 0.10)",
      amountLabel: "Spent",
      amountValue: formatCurrency(current.summary.totalAmountSpent),
      targetLabel: "Budget",
      targetValue: formatCurrency(current.summary.totalAmountToSpend),
      goalsMet: `${current.summary.percentageOfSpendingGoalsMet.metCount} / ${current.summary.percentageOfSpendingGoalsMet.totalCount}`,
      percentMet: formatPercentage(
        current.summary.percentageOfSpendingGoalsMet.percentageMet,
      ),
    },
  ] as const;

  return (
    <Box
      sx={{
        display: "grid",
        gap: 2,
        gridTemplateColumns: "repeat(auto-fit, minmax(min(100%, 320px), 1fr))",
      }}
    >
      {cards.map((card) => (
        <Paper
          key={card.key}
          sx={{
            border: "1px solid",
            borderColor: "divider",
            borderRadius: 3,
            p: { xs: 2, md: 2.5 },
            background: `linear-gradient(180deg, ${card.tint} 0%, rgba(255,255,255,0.98) 100%)`,
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
                {card.label}
              </Typography>
              <Typography variant="h6">{card.title}</Typography>
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
              <Stack spacing={1.1}>
                <Stack direction="row" justifyContent="space-between" gap={2}>
                  <Typography variant="body2" color="text.secondary">
                    {card.amountLabel}
                  </Typography>
                  <Typography variant="body2" fontWeight={700}>
                    {card.amountValue}
                  </Typography>
                </Stack>
                <Stack direction="row" justifyContent="space-between" gap={2}>
                  <Typography variant="body2" color="text.secondary">
                    {card.targetLabel}
                  </Typography>
                  <Typography variant="body2" fontWeight={700}>
                    {card.targetValue}
                  </Typography>
                </Stack>
                <Stack direction="row" justifyContent="space-between" gap={2}>
                  <Typography variant="body2" color="text.secondary">
                    Goals met
                  </Typography>
                  <Typography variant="body2" fontWeight={700}>
                    {card.goalsMet}
                  </Typography>
                </Stack>
                <Stack direction="row" justifyContent="space-between" gap={2}>
                  <Typography variant="body2" color="text.secondary">
                    Percent met
                  </Typography>
                  <Typography variant="body2" fontWeight={700}>
                    {card.percentMet}
                  </Typography>
                </Stack>
              </Stack>
            </Box>
          </Stack>
        </Paper>
      ))}
    </Box>
  );
};

export default CurrentGoalsSummaryCards;
