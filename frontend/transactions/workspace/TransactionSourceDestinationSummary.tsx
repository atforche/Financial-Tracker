import { Box, Stack, Typography, alpha } from "@mui/material";
import Frame, { type FrameColor } from "@/framework/view/Frame";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";

/**
 * Props for the TransactionSourceDestinationSummary component.
 */
interface TransactionSourceDestinationSummaryProps {
  readonly sourceAmount: number;
  readonly destinationAmount: number;
  readonly isValid: boolean;
}

/**
 * Displays a shared source, destination, and difference summary for transaction flows.
 */
const TransactionSourceDestinationSummary = function ({
  sourceAmount,
  destinationAmount,
  isValid,
}: TransactionSourceDestinationSummaryProps): JSX.Element {
  const difference = sourceAmount - destinationAmount;
  const isBalanced = difference === 0;
  const summaryColor: FrameColor = isValid ? "info" : "error";
  const summaryCards = [
    {
      label: "Source Total",
      value: formatCurrency(sourceAmount),
      tone: "text.primary",
    },
    {
      label: "Destination Total",
      value: formatCurrency(destinationAmount),
      tone: "text.primary",
    },
    {
      label: "Difference",
      value: formatCurrency(difference),
      tone: isBalanced ? "success.main" : "error.main",
    },
  ] as const;

  return (
    <Box sx={{ maxWidth: 1200, width: "100%" }}>
      <Frame title="Summary" color={summaryColor}>
        <Stack spacing={2}>
          <Box
            sx={{
              display: "grid",
              gap: 1.5,
              gridTemplateColumns:
                "repeat(auto-fit, minmax(min(100%, 180px), 1fr))",
            }}
          >
            {summaryCards.map((card) => (
              <Box
                key={card.label}
                sx={(theme) => ({
                  p: 2,
                  borderRadius: 3,
                  border: `1px solid ${alpha(theme.palette.divider, 0.72)}`,
                  backgroundColor: alpha(theme.palette.text.primary, 0.02),
                })}
              >
                <Typography variant="caption" color="text.secondary">
                  {card.label}
                </Typography>
                <Typography variant="h6" color={card.tone}>
                  {card.value}
                </Typography>
              </Box>
            ))}
          </Box>
        </Stack>
      </Frame>
    </Box>
  );
};

export default TransactionSourceDestinationSummary;
