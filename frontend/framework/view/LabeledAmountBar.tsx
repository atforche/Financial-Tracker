import type { JSX, ReactNode } from "react";
import { Stack, Typography } from "@mui/material";
import AmountBar from "@/framework/view/AmountBar";

/**
 * Props for the LabeledAmountBar component.
 */
interface LabeledAmountBarProps {
  readonly label: string;
  readonly value: ReactNode;
  readonly ratio: number;
  readonly color: string;
  readonly barHeight?: number;
  readonly barRadius?: number;
  readonly compact?: boolean;
}

/**
 * Displays a label and value followed by a proportional amount bar.
 */
const LabeledAmountBar = function ({
  label,
  value,
  ratio,
  color,
  barHeight,
  barRadius,
  compact = false,
}: LabeledAmountBarProps): JSX.Element {
  const variant = compact ? "body2" : undefined;

  return (
    <Stack spacing={compact ? 1 : 2}>
      <Stack direction="row" justifyContent="space-between" alignItems="center">
        <Typography variant={variant} color="text.secondary">
          {label}
        </Typography>
        <Typography variant={variant} fontWeight={600} color={color}>
          {value}
        </Typography>
      </Stack>
      <AmountBar
        ratio={ratio}
        color={color}
        height={barHeight}
        borderRadius={barRadius}
      />
    </Stack>
  );
};

export type { LabeledAmountBarProps };
export default LabeledAmountBar;
