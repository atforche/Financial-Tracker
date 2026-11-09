import { Stack, Typography } from "@mui/material";
import {
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import type { JSX } from "react";

/**
 * Props for the TransactionSourceDestinationSummary component.
 */
interface TransactionSourceDestinationSummaryProps {
  readonly sourceAmount: number;
  readonly destinationAmount: number;
}

/**
 * Shows the source and destination totals of a transaction and the difference between them, on one line.
 */
const TransactionSourceDestinationSummary = function ({
  sourceAmount,
  destinationAmount,
}: TransactionSourceDestinationSummaryProps): JSX.Element {
  const difference = getCurrencyDifference(sourceAmount, destinationAmount);
  const totals = [
    { label: "Source", value: sourceAmount, color: "text.primary" },
    { label: "Destinations", value: destinationAmount, color: "text.primary" },
    {
      label: "Difference",
      value: difference,
      color: difference === 0 ? "success.main" : "error.main",
    },
  ];

  return (
    <Stack direction="row" spacing={3} useFlexGap sx={{ flexWrap: "wrap" }}>
      {totals.map((total) => (
        <Stack key={total.label}>
          <Typography variant="caption" color="text.secondary">
            {total.label}
          </Typography>
          <Typography variant="subtitle1" color={total.color}>
            {formatCurrency(total.value)}
          </Typography>
        </Stack>
      ))}
    </Stack>
  );
};

export default TransactionSourceDestinationSummary;
