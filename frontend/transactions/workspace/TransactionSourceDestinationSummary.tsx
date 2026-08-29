import Frame, { type FrameColor } from "@/framework/view/Frame";
import { Stack, Typography } from "@mui/material";
import {
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import InsetFrame from "@/framework/view/InsetFrame";
import type { JSX } from "react";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";

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
  const difference = getCurrencyDifference(sourceAmount, destinationAmount);
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
    <ConstrainedContent maxWidth={1200}>
      <Frame title="Summary" color={summaryColor}>
        <Stack spacing={2}>
          <ResponsiveGrid minimumColumnWidth={180} spacing={1.5}>
            {summaryCards.map((card) => (
              <InsetFrame key={card.label}>
                <Typography variant="caption" color="text.secondary">
                  {card.label}
                </Typography>
                <Typography variant="h6" color={card.tone}>
                  {card.value}
                </Typography>
              </InsetFrame>
            ))}
          </ResponsiveGrid>
        </Stack>
      </Frame>
    </ConstrainedContent>
  );
};

export default TransactionSourceDestinationSummary;
